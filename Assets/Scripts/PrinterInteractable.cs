using System.Collections;
using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject seedlingPrefab;
    [SerializeField] private GameObject indicator;
    [SerializeField] private PrintResultPanel resultPanel;
    [SerializeField] private float printDuration = 45f;

    private bool _isPrinting;

    private void Awake()
    {
        if (indicator != null) indicator.SetActive(false);
    }

    private void Start() => UpdateInventoryContext();

    public void OnFocused()
    {
        if (indicator != null) indicator.SetActive(true);
    }

    public void OnUnfocused()
    {
        if (indicator != null) indicator.SetActive(false);
    }

    public void Interact()
    {
        if (_isPrinting) return;

        var collection = SaveManager.Instance.Milestones.genomeCollection;
        if (collection.Count == 0) return;

        if (FindAvailableSlot() == null) return;

        StartCoroutine(PrintRoutine(collection[collection.Count - 1]));
    }

    public void UpdateInventoryContext()
    {
        ConditionContext.SetBool("hasInventorySpace", FindAvailableSlot() != null);

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

    private IEnumerator PrintRoutine(GenomeRecord record)
    {
        _isPrinting = true;
        AudioManager.Instance.PlayPrinterSound();
        yield return null;

        Transform slot = FindAvailableSlot();
        if (slot == null)
        {
            _isPrinting = false;
            yield break;
        }

        Seedling seedling = GenerateSeedling(record);
        Instantiate(seedlingPrefab, slot).GetComponent<SeedlingItem>().Initialize(seedling);

        SaveManager.Instance.SetMilestone("hasSequence", false);
        UpdateInventoryContext();

        _isPrinting = false;

        if (resultPanel != null)
            resultPanel.Show(seedling);
        else
            UIManager.Instance.ShowHUD();
    }

    private Transform FindAvailableSlot()
    {
        foreach (Transform slot in inventoryContainer)
            if (slot.childCount == 0) return slot;
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
