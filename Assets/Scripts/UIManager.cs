using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private SaveLoadPanel saveLoadPanel;
    [SerializeField] private GameObject sequenceMinigamePanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void SetPanels(GameObject active)
    {
        hudPanel.SetActive(hudPanel == active);
        dialogPanel.SetActive(dialogPanel == active);
        pausePanel.SetActive(pausePanel == active);
        menuPanel.SetActive(menuPanel == active);
        saveLoadPanel.gameObject.SetActive(saveLoadPanel.gameObject == active);
        sequenceMinigamePanel.SetActive(sequenceMinigamePanel == active);
    }

    public void ShowHUD()
    {
        if (sequenceMinigamePanel.activeSelf) return;
        SetPanels(hudPanel);
    }
    public void ShowDialog() => SetPanels(dialogPanel);
    public void ShowPause() => SetPanels(pausePanel);
    public void ShowMenu() => SetPanels(menuPanel);
    public void ShowSequenceMinigame() => SetPanels(sequenceMinigamePanel);
    public void ShowSaveMenu() { SetPanels(saveLoadPanel.gameObject); saveLoadPanel.Open(true); }
    public void ShowLoadMenu() { SetPanels(saveLoadPanel.gameObject); saveLoadPanel.Open(false); }

    public void OnStartButtonPressed() => GameManager.Instance.StartGame();
    public void OnResumeButtonPressed() => GameManager.Instance.ResumeGame();
    public void OnMenuButtonPressed() => GameManager.Instance.ReturnToMenu();
    public void OnExitButtonPressed() => Application.Quit();
}