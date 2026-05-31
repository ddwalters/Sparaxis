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

    private bool isPaused;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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
        if (player != null)
            player.SetActive(false);

        virtualCamera.Follow = menuCameraTarget;
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
        if (player != null)
            player.SetActive(true);

        inputActions.FindActionMap("Player").Enable();
        virtualCamera.Follow = player.transform;
        UIManager.Instance.ShowHUD();
    }

    public void PauseGame()
    {
        isPaused = true;
        inputActions.FindActionMap("Player").Disable();
        virtualCamera.Follow = menuCameraTarget;
        UIManager.Instance.ShowPause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        inputActions.FindActionMap("Player").Enable();
        virtualCamera.Follow = player.transform;
        UIManager.Instance.ShowHUD();
    }

    public void ReturnToMenu()
    {
        isPaused = false;
        inputActions.FindActionMap("Player").Disable();

        if (player != null)
            player.SetActive(false);

        virtualCamera.Follow = menuCameraTarget;
        UIManager.Instance.ShowMenu();
    }
}
