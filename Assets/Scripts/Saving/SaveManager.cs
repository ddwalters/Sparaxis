using System;
using System.IO;
using NodeTree;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static event Action OnContextReady;

    public MilestoneTracker Milestones { get; private set; } = new MilestoneTracker();

    [SerializeField] private GameObject player;
    private PlayerMovement playerMovement;

    private const int SlotCount = 5;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("SetHasSeenPrinter",  OnSetHasSeenPrinter);
        NodeTreeEvents.Subscribe("SetHasSeenGarden",   OnSetHasSeenGarden);
        NodeTreeEvents.Subscribe("SetHasSeenShuttle",  OnSetHasSeenShuttle);
        NodeTreeEvents.Subscribe("SetSeedlingCaptain", OnSetSeedlingCaptain);
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("SetHasSeenPrinter",  OnSetHasSeenPrinter);
        NodeTreeEvents.Unsubscribe("SetHasSeenGarden",   OnSetHasSeenGarden);
        NodeTreeEvents.Unsubscribe("SetHasSeenShuttle",  OnSetHasSeenShuttle);
        NodeTreeEvents.Unsubscribe("SetSeedlingCaptain", OnSetSeedlingCaptain);
    }

    private void OnSetHasSeenPrinter()  => SetMilestone("hasSeenPrinter",  true);
    private void OnSetHasSeenGarden()   => SetMilestone("hasSeenGarden",   true);
    private void OnSetHasSeenShuttle()  => SetMilestone("hasSeenShuttle",  true);
    private void OnSetSeedlingCaptain() => SetMilestone("seedlingCaptain", true);

    public void Save(int slot)
    {
        SaveData data = new SaveData
        {
            playerPosition = player.transform.position,
            playerFacingDirection = playerMovement.FacingDirection,
            playTimeSeconds = GameManager.Instance.PlayTime,
            milestones = Milestones
        };

        File.WriteAllText(SavePath(slot), JsonUtility.ToJson(data));
        File.WriteAllText(MetaPath(slot), JsonUtility.ToJson(new SaveSlotMeta
        {
            slot = slot,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            playTimeSeconds = data.playTimeSeconds,
            earthPercent = Milestones.earthPercent,
            isEmpty = false
        }));
    }

    public void NewGame()
    {
        Milestones = new MilestoneTracker();
        GameManager.Instance.PlayTime = 0f;
        player.transform.position = new Vector3(-0.54f, -0.04f, 0f); // looks cooler if player starts here...
        playerMovement.SetFacingDirection(new Vector2(1f, 1f));
        ConditionContext.Clear();
        ApplyMilestonesToContext();
        OnContextReady?.Invoke();
    }

    public void Load(int slot)
    {
        if (!File.Exists(SavePath(slot)))
        {
            Debug.LogWarning($"No save file found for slot {slot}!");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath(slot)));
        player.transform.position = data.playerPosition;
        playerMovement.SetFacingDirection(data.playerFacingDirection);
        GameManager.Instance.PlayTime = data.playTimeSeconds;
        Milestones = data.milestones;
        ConditionContext.Clear();
        ApplyMilestonesToContext();
        OnContextReady?.Invoke();
    }

    public void SetMilestone(string key, bool value)
    {
        switch (key)
        {
            case "hasSeenComputer": Milestones.hasSeenComputer = value; break;
            case "hasSeenPrinter": Milestones.hasSeenPrinter = value; break;
            case "hasSeenGarden": Milestones.hasSeenGarden = value; break;
            case "hasSeenShuttle": Milestones.hasSeenShuttle = value; break;
            case "seedlingCaptain": Milestones.seedlingCaptain = value; break;
            case "hasSequence": Milestones.hasSequence = value; break;
            default: Debug.LogWarning($"[SaveManager] Unknown milestone key: '{key}'"); break;
        }
        ConditionContext.SetBool(key, value);
        OnContextReady?.Invoke();
    }

    public void ApplyMilestonesToContext()
    {
        ConditionContext.SetBool("hasSeenComputer", Milestones.hasSeenComputer);
        ConditionContext.SetBool("hasSeenPrinter",  Milestones.hasSeenPrinter);
        ConditionContext.SetBool("hasSeenGarden",   Milestones.hasSeenGarden);
        ConditionContext.SetBool("hasSeenShuttle",  Milestones.hasSeenShuttle);
        ConditionContext.SetBool("seedlingCaptain", Milestones.seedlingCaptain);
        ConditionContext.SetBool("hasSequence",     Milestones.hasSequence);
    }

    public SaveSlotMeta GetSlotMeta(int slot)
    {
        if (!File.Exists(MetaPath(slot)))
            return new SaveSlotMeta { slot = slot, isEmpty = true };

        return JsonUtility.FromJson<SaveSlotMeta>(File.ReadAllText(MetaPath(slot)));
    }

    public SaveSlotMeta[] GetAllSlotMeta()
    {
        SaveSlotMeta[] metas = new SaveSlotMeta[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            metas[i] = GetSlotMeta(i);
        return metas;
    }

    private string SavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    private string MetaPath(int slot) => Path.Combine(Application.persistentDataPath, $"meta_{slot}.json");
}
