using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour
{
    public static PrinterInteractable Instance { get; private set; }

    [SerializeField] private Transform itemHolder;
    [SerializeField] private GameObject seedlingPrefab;

    private SeedlingItem _currentItem;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => UpdateInventoryContext();

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("PrintGenome", OnPrintGenome);
        NodeTreeEvents.Subscribe("PlantSeedling", OnPlantSeedling);
        NodeTreeEvents.Subscribe("HarvestPlant", OnHarvestPlant);
        NodeTreeEvents.Subscribe("ShuttleGenome", OnShuttleGenome);
        SaveManager.OnContextReady += UpdateInventoryContext;
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("PrintGenome", OnPrintGenome);
        NodeTreeEvents.Unsubscribe("PlantSeedling", OnPlantSeedling);
        NodeTreeEvents.Unsubscribe("HarvestPlant", OnHarvestPlant);
        NodeTreeEvents.Unsubscribe("ShuttleGenome", OnShuttleGenome);
        SaveManager.OnContextReady -= UpdateInventoryContext;
    }

    public void UpdateInventoryContext()
    {
        if (_currentItem == null)
            _currentItem = itemHolder.GetComponentInChildren<SeedlingItem>();

        ConditionContext.SetBool("hasInventorySpace",  _currentItem == null);
        ConditionContext.SetBool("playerHasSeedling",  _currentItem != null && !_currentItem.IsGrown);
        ConditionContext.SetBool("hasGrownPlant",      _currentItem != null && _currentItem.IsGrown);


        bool hasDuplicate = false;
        if (_currentItem != null)
        {
            var collection = SaveManager.Instance.Milestones.genomeCollection;
            if (collection.Count > 0)
                hasDuplicate = _currentItem.Data.sourcePlant.name == collection[collection.Count - 1].plantName;
        }

        ConditionContext.SetBool("hasDuplicateSeedling", hasDuplicate);
    }

    public void DropCurrentItem()
    {
        if (_currentItem == null) return;
        _currentItem.transform.SetParent(null);
        Destroy(_currentItem.gameObject);
        _currentItem = null;
        ConditionContext.SetBool("playerHasSeedling", false);
        ConditionContext.SetBool("hasGrownPlant",     false);
        ConditionContext.SetBool("hasInventorySpace", true);
    }

    private void OnPlantSeedling()
    {
        if (_currentItem == null)
        {
            Debug.LogWarning("[Printer] No seedling in inventory to plant.");
            return;
        }

        GardenSlot slot = GardenManager.Instance.GetNearestEmptySlot(SaveManager.Instance.PlayerPosition);
        if (slot == null)
        {
            Debug.LogWarning("[Printer] No empty garden slot near player.");
            return;
        }

        slot.Plant(_currentItem.Data);
        GardenManager.Instance.UpdateGardenContext();

        _currentItem.transform.SetParent(null);
        Destroy(_currentItem.gameObject);
        _currentItem = null;
        ConditionContext.SetBool("playerHasSeedling", false);
        ConditionContext.SetBool("hasInventorySpace", true);
    }

    private void OnHarvestPlant()
    {
        Seedling seedling = GardenManager.Instance.LastHarvestedSeedling;
        if (seedling == null) return;

        GameObject obj = Instantiate(seedlingPrefab, itemHolder);
        obj.transform.localScale = Vector3.one;
        _currentItem = obj.GetComponent<SeedlingItem>();
        _currentItem.Initialize(seedling, isGrown: true);
        ConditionContext.SetBool("hasInventorySpace", false);
        UpdateInventoryContext();
    }

    private void OnShuttleGenome()
    {
        if (_currentItem == null || !_currentItem.IsGrown) return;

        GameManager.Instance.AddWorldRecovery(_currentItem.Data.effective, _currentItem.Data.speed, _currentItem.Data.resistance);

        _currentItem.transform.SetParent(null);
        Destroy(_currentItem.gameObject);
        _currentItem = null;
        ConditionContext.SetBool("hasGrownPlant", false);
        ConditionContext.SetBool("hasInventorySpace", true);
    }

    private void OnPrintGenome()
    {
        var collection = SaveManager.Instance.Milestones.genomeCollection;

        if (collection.Count == 0) { Debug.LogWarning("[Printer] No genomes in collection."); return; }
        if (_currentItem != null) return;

        PlantData plant = collection[collection.Count - 1].Plant;
        if (plant == null) { Debug.LogError("[Printer] Plant not found in database."); return; }

        Seedling seedling = GenerateSeedling(collection[collection.Count - 1]);
        GameObject obj = Instantiate(seedlingPrefab, itemHolder);
        _currentItem = obj.GetComponent<SeedlingItem>();
        _currentItem.Initialize(seedling);
        SaveManager.Instance.SetMilestone("hasSequence", false);
        UpdateInventoryContext();
    }

    private static Seedling GenerateSeedling(GenomeRecord record)
    {
        PlantData plant = record.Plant;
        float budget = Mathf.Clamp(record.score * 0.003f, 0.05f, 3f);

        float w1 = Mathf.Max(0.15f, Random.value);
        float w2 = Mathf.Max(0.15f, Random.value);
        float w3 = Mathf.Max(0.15f, Random.value);
        float total = w1 + w2 + w3;

        Seedling seedling = new Seedling
        {
            name = plant.name,
            effective = (w1 / total) * budget,
            speed = (w2 / total) * budget,
            resistance = (w3 / total) * budget,
            sourcePlant = plant
        };

        if (!string.IsNullOrEmpty(plant.bonusStat))
        {
            switch (plant.bonusStat)
            {
                case "Effective":
                    seedling.effective += plant.bonusAmount;
                    break;
                case "Speed":
                    seedling.speed += plant.bonusAmount;
                    break;
                case "Resistance":
                    seedling.resistance += plant.bonusAmount;
                    break;
            }
        }

        return seedling;
    }
}
