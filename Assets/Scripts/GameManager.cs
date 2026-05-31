using NodeTree;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform menuCameraTarget;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private NodeTreeGraphSO openingDialog;

    private bool isPaused;
    private bool isPlaying;
    public float PlayTime { get; set; }

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
        if (isPlaying)
            PlayTime += Time.deltaTime;
    }

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPause;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPause;
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
