using System.Collections.Generic;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    public static GardenManager Instance { get; private set; }

    [SerializeField] private List<GardenSlot> slots;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryPlant(Seedling seedling)
    {
        foreach (GardenSlot slot in slots)
        {
            if (!slot.IsOccupied)
            {
                slot.Plant(seedling);
                return true;
            }
        }

        Debug.LogWarning("[GardenManager] All slots are occupied.");
        return false;
    }
}
