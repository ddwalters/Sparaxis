using NodeTree;
using UnityEngine;

public class SequenceMinigame : MonoBehaviour
{
    public static SequenceMinigame Instance { get; private set; }

    [SerializeField] private SequenceMinigamePanel panel;

    private bool _playingForFun;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public PlantData CurrentPlant       => panel.CurrentPlant;
    public PlantData LastCompletedPlant => panel.LastCompletedPlant;
    public int       LastCompletedScore => panel.LastCompletedScore;

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("SequenceGenome",     OpenMinigame);
        NodeTreeEvents.Subscribe("SequenceForFun",     OpenMinigameForFun);
        NodeTreeEvents.Subscribe("SetHasSeenComputer", SetHasSeenComputer);
        NodeTreeEvents.Subscribe("SetComputerContext", UpdateComputerContext);
        SaveManager.OnContextReady += UpdateComputerContext;
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("SequenceGenome",     OpenMinigame);
        NodeTreeEvents.Unsubscribe("SequenceForFun",     OpenMinigameForFun);
        NodeTreeEvents.Unsubscribe("SetHasSeenComputer", SetHasSeenComputer);
        NodeTreeEvents.Unsubscribe("SetComputerContext", UpdateComputerContext);
        SaveManager.OnContextReady -= UpdateComputerContext;
    }

    private void OpenMinigameForFun()
    {
        _playingForFun = true;
        OpenMinigame();
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
        Debug.Log($"[Minigame] CloseMinigame called. LastCompletedPlant={panel.LastCompletedPlant?.name ?? "null"}, Score={panel.LastCompletedScore}, ForFun={_playingForFun}");

        if (!_playingForFun && panel.LastCompletedPlant != null)
        {
            SaveManager.Instance.Milestones.genomeCollection.Add(new GenomeRecord
            {
                plantName = panel.LastCompletedPlant.name,
                score     = panel.LastCompletedScore
            });
            SaveManager.Instance.SetMilestone("hasSequence", true);
        }

        _playingForFun = false;
        GameManager.Instance.EnablePlayerInput();
        UIManager.Instance.ShowDialog();
    }

    private void SetHasSeenComputer() =>
        SaveManager.Instance.SetMilestone("hasSeenComputer", true);

    private void UpdateComputerContext()
    {
        PlantData[] unlocked = PlantDatabase.GetUnlocked();
        var collection = SaveManager.Instance.Milestones.genomeCollection;
        bool allSequenced = unlocked.Length > 0 &&
            System.Array.TrueForAll(unlocked, p => collection.Exists(g => g.plantName == p.name));
        ConditionContext.SetBool("allGenomesSequenced", allSequenced);
    }
}
