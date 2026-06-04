using NodeTree;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform menuCameraTarget;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private NodeTreeGraphSO openingDialog;
    [SerializeField] private NodeTreeGraphSO completionDialog;

    [Header("World Stats HUD")]
    [SerializeField] private GameObject statsContainer;
    [SerializeField] private Slider recoverySlider;
    [SerializeField] private TextMeshProUGUI recoveryText;
    [SerializeField] private TextMeshProUGUI growthSpeedText;
    [SerializeField] private TextMeshProUGUI efficiencyText;
    [SerializeField] private TextMeshProUGUI resistanceText;

    private bool isPaused;
    private bool isPlaying;
    public float PlayTime { get; set; }

    public static event Action<float> OnWorldRecoveryChanged;

    public float WorldRecoveryPercent => SaveManager.Instance.Milestones.earthPercent;

    public void AddWorldRecovery(float effective, float speed, float resistance)
    {
        var m = SaveManager.Instance.Milestones;
        m.earthPercent     += effective;
        m.earthGrowthSpeed += speed;
        m.earthResistance  += resistance;
        OnWorldRecoveryChanged?.Invoke(m.earthPercent);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!isPlaying) return;
        PlayTime += Time.deltaTime;

        var m = SaveManager.Instance.Milestones;
        float totalStats = m.earthGrowthSpeed + m.earthEfficiency + m.earthResistance;
        if (totalStats <= 0f) return;

        bool wasComplete = m.earthPercent >= 100f;
        m.earthPercent = Mathf.Min(100f, m.earthPercent + totalStats * 0.005f * Time.deltaTime);
        if (recoverySlider != null) recoverySlider.value = Mathf.Clamp01(m.earthPercent / 100f);
        if (recoveryText   != null) recoveryText.text   = $"{m.earthPercent:0.#}%";

        if (!wasComplete && m.earthPercent >= 100f)
            TriggerCompletion();
    }

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPause;
        OnWorldRecoveryChanged       += OnRecoveryChanged;
        SaveManager.OnContextReady   += RefreshStatsDisplay;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPause;
        OnWorldRecoveryChanged       -= OnRecoveryChanged;
        SaveManager.OnContextReady   -= RefreshStatsDisplay;
    }

    private void Start()
    {
        inputActions.FindActionMap("Player").Disable();
        virtualCamera.Follow = menuCameraTarget;
        ShowCursor(true);
        UIManager.Instance.ShowMenu();
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void StartGame()
    {
        SaveManager.Instance.NewGame();
        isPlaying = true;
        inputActions.FindActionMap("Player").Enable();
        virtualCamera.Follow = player.transform;
        ShowCursor(false);

        if (openingDialog != null)
        {
            UIManager.Instance.ShowDialog();
            DialogRunner.Instance.StartDialog(openingDialog);
            StartCoroutine(WaitForOpeningDialog());
        }
        else
        {
            UIManager.Instance.ShowHUD();
        }
    }

    public void LoadGame(int slot)
    {
        SaveManager.Instance.Load(slot);
        isPlaying = true;
        inputActions.FindActionMap("Player").Enable();
        virtualCamera.Follow = player.transform;
        ShowCursor(false);
        UIManager.Instance.ShowHUD();
    }

    public void PauseGame()
    {
        // TODO: @DW When I add plants, I will need to make sure they don't grow on pause.
        isPaused = true;
        isPlaying = false;
        inputActions.FindActionMap("Player").Disable();
        virtualCamera.Follow = menuCameraTarget;
        ShowCursor(true);
        UIManager.Instance.ShowPause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        isPlaying = true;
        inputActions.FindActionMap("Player").Enable();
        virtualCamera.Follow = player.transform;
        ShowCursor(false);
        UIManager.Instance.ShowHUD();
    }

    public void ReturnToMenu()
    {
        isPaused = false;
        isPlaying = false;
        inputActions.FindActionMap("Player").Disable();
        virtualCamera.Follow = menuCameraTarget;
        ShowCursor(true);
        UIManager.Instance.ShowMenu();
    }

    private void ShowCursor(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnRecoveryChanged(float _)
    {
        var m = SaveManager.Instance.Milestones;
        m.earthPercent = Mathf.Min(100f, m.earthPercent);
        RefreshStatsDisplay();
        if (m.earthPercent >= 100f)
            TriggerCompletion();
    }

    private void TriggerCompletion()
    {
        if (completionDialog == null) return;
        isPlaying = false;
        UIManager.Instance.ShowDialog();
        DialogRunner.Instance.StartDialog(completionDialog);
    }

    private void RefreshStatsDisplay()
    {
        if (statsContainer == null) return;
        var m = SaveManager.Instance.Milestones;
        bool unlocked = m.seedlingCaptain;
        statsContainer.SetActive(unlocked);
        if (!unlocked) return;

        if (recoverySlider  != null) recoverySlider.value  = Mathf.Clamp01(m.earthPercent / 100f);
        if (recoveryText    != null) recoveryText.text     = $"{m.earthPercent:0.#}%";
        if (growthSpeedText != null) growthSpeedText.text = m.earthGrowthSpeed.ToString("0.###");
        if (resistanceText  != null) resistanceText.text  = m.earthResistance.ToString("0.###");

        if (efficiencyText != null)
        {
            var collection = m.genomeCollection;
            float avgScore = collection.Count > 0
                ? (float)collection.Sum(r => r.score) / collection.Count
                : 0f;
            efficiencyText.text = avgScore.ToString("0");
        }
    }

    public void TriggerDialog(NodeTreeTrigger trigger)
    {
        OnDialogRequested?.Invoke(trigger);
    }

    private System.Collections.IEnumerator WaitForOpeningDialog()
    {
        yield return new WaitUntil(() => !DialogRunner.Instance.IsDialogActive);
        UIManager.Instance.ShowHUD();
    }

    public void DisablePlayerInput() => inputActions.FindActionMap("Player").Disable();
    public void EnablePlayerInput() => inputActions.FindActionMap("Player").Enable();

    public event Action<NodeTreeTrigger> OnDialogRequested;
}
