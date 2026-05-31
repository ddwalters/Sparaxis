using System.Collections.Generic;
using NodeTree;
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

    private void Start() => UpdateGardenContext();

    private void OnEnable()  => SaveManager.OnContextReady += UpdateGardenContext;
    private void OnDisable() => SaveManager.OnContextReady -= UpdateGardenContext;

    public bool TryPlant(Seedling seedling)
    {
        foreach (GardenSlot slot in slots)
        {
            if (!slot.IsOccupied)
            {
                slot.Plant(seedling);
                UpdateGardenContext();
                return true;
            }
        }

        Debug.LogWarning("[GardenManager] All slots are occupied.");
        return false;
    }

    public void UpdateGardenContext()
    {
        bool hasGrowing = false;
        foreach (GardenSlot slot in slots)
        {
            if (slot.IsOccupied && !slot.IsGrown) { hasGrowing = true; break; }
        }
        ConditionContext.SetBool("hasGrowingPlant", hasGrowing);
    }
}
