using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour
{
    [SerializeField] private Transform itemHolder;
    [SerializeField] private GameObject seedlingPrefab;

    private void Start() => UpdateInventoryContext();

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("PrintGenome", OnPrintGenome);
        NodeTreeEvents.Subscribe("PlantSeedling", OnPlantSeedling);
        SaveManager.OnContextReady += UpdateInventoryContext;
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("PrintGenome", OnPrintGenome);
        NodeTreeEvents.Unsubscribe("PlantSeedling", OnPlantSeedling);
        SaveManager.OnContextReady -= UpdateInventoryContext;
    }

    public void UpdateInventoryContext()
    {
        SeedlingItem current = itemHolder.GetComponentInChildren<SeedlingItem>();
        ConditionContext.SetBool("hasInventorySpace", current == null);
        ConditionContext.SetBool("hasSeedling", current != null);

        bool hasDuplicate = false;
        if (current != null)
        {
            var collection = SaveManager.Instance.Milestones.genomeCollection;
            if (collection.Count > 0)
                hasDuplicate = current.Data.sourcePlant.name == collection[collection.Count - 1].plant.name;
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
            Destroy(item.gameObject);
            UpdateInventoryContext();
        }
    }

    private void OnPrintGenome()
    {
        var collection = SaveManager.Instance.Milestones.genomeCollection;
        Debug.Log($"[Printer] itemHolder={itemHolder}, genomes={collection.Count}, hasSeedling={itemHolder?.GetComponentInChildren<SeedlingItem>() != null}");

        if (collection.Count == 0) { Debug.LogWarning("[Printer] No genomes in collection."); return; }
        if (itemHolder.GetComponentInChildren<SeedlingItem>() != null) return;

        Seedling seedling = GenerateSeedling(collection[collection.Count - 1]);
        Instantiate(seedlingPrefab, itemHolder).GetComponent<SeedlingItem>().Initialize(seedling);
        SaveManager.Instance.SetMilestone("hasSequence", false);
        UpdateInventoryContext();
    }

    private static Seedling GenerateSeedling(GenomeRecord record)
    {
        float budget = Mathf.Clamp(record.score * 0.001f, 0.05f, 0.5f);

        float w1 = Random.value, w2 = Random.value, w3 = Random.value;
        float total = w1 + w2 + w3;

        Seedling seedling = new Seedling
        {
            name        = record.plant.name,
            effective   = (w1 / total) * budget,
            speed       = (w2 / total) * budget,
            resistance  = (w3 / total) * budget,
            sourcePlant = record.plant
        };

        if (!string.IsNullOrEmpty(record.plant.bonusStat))
        {
            switch (record.plant.bonusStat)
            {
                case "Effective":  seedling.effective  += record.plant.bonusAmount; break;
                case "Speed":      seedling.speed      += record.plant.bonusAmount; break;
                case "Resistance": seedling.resistance += record.plant.bonusAmount; break;
            }
        }

        return seedling;
    }
}
