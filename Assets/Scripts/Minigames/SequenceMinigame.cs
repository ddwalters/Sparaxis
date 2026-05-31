using NodeTree;
using UnityEngine;

public class SequenceMinigame : MonoBehaviour
{
    public static SequenceMinigame Instance { get; private set; }

    [SerializeField] private SequenceMinigamePanel panel;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public PlantData CurrentPlant => panel.CurrentPlant;
    public PlantData LastCompletedPlant => panel.LastCompletedPlant;
    public int LastCompletedScore => panel.LastCompletedScore;

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("SequenceGenome", OpenMinigame);
        NodeTreeEvents.Subscribe("SetHasSeenComputer", SetHasSeenComputer);
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("SequenceGenome", OpenMinigame);
        NodeTreeEvents.Unsubscribe("SetHasSeenComputer", SetHasSeenComputer);
    }

    private void OpenMinigame()
    {
        SaveManager.Instance.SetMilestone("hasSeenComputer", true);
        GameManager.Instance.DisablePlayerInput();
        panel.OnComplete = CloseMinigame;
        panel.StartFresh();
        UIManager.Instance.ShowSequenceMinigame();
    }

    public void CloseMinigame()
    {
        if (panel.LastCompletedPlant != null)
        {
            SaveManager.Instance.Milestones.genomeCollection.Add(new GenomeRecord
            {
                plant = panel.LastCompletedPlant,
                score = panel.LastCompletedScore
            });
            SaveManager.Instance.SetMilestone("hasSequence", true);
        }

        GameManager.Instance.EnablePlayerInput();
        UIManager.Instance.ShowDialog();
    }

    private void SetHasSeenComputer()
    {
        SaveManager.Instance.SetMilestone("hasSeenComputer", true);
    }
}
