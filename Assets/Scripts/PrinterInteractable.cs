using System.Collections;
using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour
{
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject seedlingPrefab;

    private const float PrintDuration = 45f;

    private bool _isPrinting;

    private void Start() => UpdateInventoryContext();

    private void OnEnable() => NodeTreeEvents.Subscribe("PrintGenome", OnPrintGenome);
    private void OnDisable() => NodeTreeEvents.Unsubscribe("PrintGenome", OnPrintGenome);

    public void UpdateInventoryContext()
    {
        ConditionContext.SetBool("hasInventorySpace", true);

        var collection = SaveManager.Instance.Milestones.genomeCollection;
        bool hasDuplicate = false;

        if (collection.Count > 0)
        {
            string latestName = collection[collection.Count - 1].plant.name;
            foreach (Transform slot in inventoryContainer)
            {
                SeedlingItem item = slot.GetComponentInChildren<SeedlingItem>();
                if (item != null && item.Data.sourcePlant.name == latestName)
                {
                    hasDuplicate = true;
                    break;
                }
            }
        }

        ConditionContext.SetBool("hasDuplicateSeedling", hasDuplicate);
    }

    private void OnPrintGenome()
    {
        if (_isPrinting) return;

        var collection = SaveManager.Instance.Milestones.genomeCollection;
        if (collection.Count == 0)
        {
            Debug.LogWarning("[Printer] No genomes in collection.");
            return;
        }

        GenomeRecord record = collection[collection.Count - 1];
        StartCoroutine(PrintRoutine(record));
    }

    private IEnumerator PrintRoutine(GenomeRecord record)
    {
        _isPrinting = true;

        yield return new WaitForSeconds(PrintDuration);

        Transform slot = FindAvailableSlot();
        if (slot == null)
        {
            Debug.LogWarning("[Printer] No available inventory slots.");
            _isPrinting = false;
            yield break;
        }

        Seedling seedling = GenerateSeedling(record);
        GameObject obj = Instantiate(seedlingPrefab, slot);
        obj.GetComponent<SeedlingItem>().Initialize(seedling);
        SaveManager.Instance.SetMilestone("hasSequence", false);
        UpdateInventoryContext();

        _isPrinting = false;
    }

    private Transform FindAvailableSlot()
    {
        foreach (Transform slot in inventoryContainer)
            if (slot.childCount == 0)
                return slot;
        return null;
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
