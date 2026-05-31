using NodeTree;
using UnityEngine;

public class PrinterInteractable : MonoBehaviour
{
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject seedlingPrefab;

    private void OnEnable() => NodeTreeEvents.Subscribe("PrintGenome", OnPrintGenome);
    private void OnDisable() => NodeTreeEvents.Unsubscribe("PrintGenome", OnPrintGenome);

    private void OnPrintGenome()
    {
        PlantData plant = SequenceMinigame.Instance.LastCompletedPlant;
        int score = SequenceMinigame.Instance.LastCompletedScore;

        if (plant == null)
        {
            Debug.LogWarning("[Printer] No completed sequence to print from.");
            return;
        }

        Transform slot = FindAvailableSlot();
        if (slot == null)
        {
            Debug.LogWarning("[Printer] No available inventory slots.");
            return;
        }

        Seedling seedling = GenerateSeedling(plant, score);
        GameObject obj = Instantiate(seedlingPrefab, slot);
        obj.GetComponent<SeedlingItem>().Initialize(seedling);
    }

    private Transform FindAvailableSlot()
    {
        foreach (Transform slot in inventoryContainer)
            if (slot.childCount == 0) return slot;
        return null;
    }

    private static Seedling GenerateSeedling(PlantData plant, int score)
    {
        float budget = Mathf.Clamp(score * 0.001f, 0.05f, 0.5f);

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
