using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour
{
    public static PrinterInteractable Instance { get; private set; }

    [SerializeField] private Transform itemHolder;
    [SerializeField] private GameObject seedlingPrefab;

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
        SeedlingItem current = itemHolder.GetComponentInChildren<SeedlingItem>();
        ConditionContext.SetBool("hasInventorySpace", current == null);
        ConditionContext.SetBool("playerHasSeedling", current != null && !current.IsGrown);
        ConditionContext.SetBool("hasGrownPlant", current != null && current.IsGrown);

        bool hasDuplicate = false;
        if (current != null)
        {
            var collection = SaveManager.Instance.Milestones.genomeCollection;
            if (collection.Count > 0)
                hasDuplicate = current.Data.sourcePlant.name == collection[collection.Count - 1].plantName;
        }

        ConditionContext.SetBool("hasDuplicateSeedling", hasDuplicate);
    }

    private void OnPlantSeedling()
    {
        SeedlingItem item = itemHolder.GetComponentInChildren<SeedlingItem>();
        if (item == null)
        {
            Debug.LogWarning("[Printer] No seedling in inventory to plant.");
            return;
        }

        if (GardenManager.Instance.TryPlant(item.Data))
        {
            item.transform.SetParent(null);
            Destroy(item.gameObject);
            ConditionContext.SetBool("playerHasSeedling", false);
            ConditionContext.SetBool("hasInventorySpace", true);
        }
    }

    private void OnHarvestPlant()
    {
        Debug.Log($"[Printer] OnHarvestPlant called. LastHarvested={GardenManager.Instance.LastHarvestedSeedling?.name ?? "null"}, itemHolder={itemHolder}");
        Seedling seedling = GardenManager.Instance.LastHarvestedSeedling;
        if (seedling == null) return;

        GameObject obj = Instantiate(seedlingPrefab, itemHolder);
        obj.transform.localScale = Vector3.one * 2f;
        obj.GetComponent<SeedlingItem>().Initialize(seedling, isGrown: true);
        UpdateInventoryContext();
    }

    private void OnShuttleGenome()
    {
        SeedlingItem item = itemHolder.GetComponentInChildren<SeedlingItem>();
        if (item == null || !item.IsGrown) return;

        var milestones = SaveManager.Instance.Milestones;
        milestones.earthPercent     += item.Data.effective;
        milestones.earthEfficiency  += item.Data.speed;
        milestones.earthGrowthSpeed += item.Data.resistance;

        item.transform.SetParent(null);
        Destroy(item.gameObject);
        ConditionContext.SetBool("hasGrownPlant", false);
        ConditionContext.SetBool("hasInventorySpace", true);
    }

    private void OnPrintGenome()
    {
        var collection = SaveManager.Instance.Milestones.genomeCollection;
        Debug.Log($"[Printer] itemHolder={itemHolder}, genomes={collection.Count}, playerHasSeedling={itemHolder?.GetComponentInChildren<SeedlingItem>() != null}");

        if (collection.Count == 0) { Debug.LogWarning("[Printer] No genomes in collection."); return; }
        if (itemHolder.GetComponentInChildren<SeedlingItem>() != null) return;

        Seedling seedling = GenerateSeedling(collection[collection.Count - 1]);
        Instantiate(seedlingPrefab, itemHolder).GetComponent<SeedlingItem>().Initialize(seedling);
        SaveManager.Instance.SetMilestone("hasSequence", false);
        UpdateInventoryContext();
    }

    private static Seedling GenerateSeedling(GenomeRecord record)
    {
        PlantData plant = record.Plant;
        float budget = Mathf.Clamp(record.score * 0.001f, 0.05f, 0.5f);

        float w1 = Random.value, w2 = Random.value, w3 = Random.value;
        float total = w1 + w2 + w3;

        Seedling seedling = new Seedling
        {
            name        = plant.name,
            effective   = (w1 / total) * budget,
            speed       = (w2 / total) * budget,
            resistance  = (w3 / total) * budget,
            sourcePlant = plant
        };

        if (!string.IsNullOrEmpty(plant.bonusStat))
        {
            switch (plant.bonusStat)
            {
                case "Effective":  seedling.effective  += plant.bonusAmount; break;
                case "Speed":      seedling.speed      += plant.bonusAmount; break;
                case "Resistance": seedling.resistance += plant.bonusAmount; break;
            }
        }

        return seedling;
    }
}
