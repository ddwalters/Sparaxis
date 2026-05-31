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
        RecordGenome();
        GameManager.Instance.EnablePlayerInput();
        UIManager.Instance.ShowDialog();
    }

    private void RecordGenome()
    {
        PlantData plant = panel.LastCompletedPlant ?? panel.CurrentPlant;
        if (plant == null) return;

        int score = panel.LastCompletedPlant != null ? panel.LastCompletedScore : 0;

        var collection = SaveManager.Instance.Milestones.genomeCollection;
        GenomeRecord existing = collection.Find(r => r.plant.name == plant.name);

        if (existing != null)
            existing.score = Mathf.Max(existing.score, score);
        else
            collection.Add(new GenomeRecord { plant = plant, score = score });

        SaveManager.Instance.SetMilestone("hasSequence", true);
    }

    private void SetHasSeenComputer()
    {
        SaveManager.Instance.SetMilestone("hasSeenComputer", true);
    }
}