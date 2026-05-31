using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject menuPanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void SetPanels(bool hud = false, bool dialog = false, bool pause = false, bool menu = false)
    {
        hudPanel.SetActive(hud);
        dialogPanel.SetActive(dialog);
        pausePanel.SetActive(pause);
        menuPanel.SetActive(menu);
    }

    public void ShowHUD() => SetPanels(hud: true);
    public void ShowDialog() => SetPanels(dialog: true);
    public void ShowPause() => SetPanels(hud: true, pause: true);
    public void ShowMenu() => SetPanels(menu: true);

    public void OnStartButtonPressed() => GameManager.Instance.StartGame();
    public void OnResumeButtonPressed() => GameManager.Instance.ResumeGame();
    public void OnMenuButtonPressed() => GameManager.Instance.ReturnToMenu();
    public void OnExitButtonPressed() => Application.Quit();
}