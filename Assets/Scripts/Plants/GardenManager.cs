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

    private void OnEnable()
    {
        SaveManager.OnContextReady += UpdateGardenContext;
        NodeTreeEvents.Subscribe("CollectPlant", HarvestFirstGrownSlot);
    }

    private void OnDisable()
    {
        SaveManager.OnContextReady -= UpdateGardenContext;
        NodeTreeEvents.Unsubscribe("CollectPlant", HarvestFirstGrownSlot);
    }

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

    public Seedling LastHarvestedSeedling { get; private set; }

    private void HarvestFirstGrownSlot()
    {
        foreach (GardenSlot slot in slots)
        {
            if (slot.IsOccupied && slot.IsGrown)
            {
                slot.Ship();
                return;
            }
        }
    }

    public void OnPlantHarvested(Seedling seedling)
    {
        LastHarvestedSeedling = seedling;
        UpdateGardenContext();
        NodeTreeEvents.Fire("HarvestPlant");
    }

    public void UpdateGardenContext()
    {
        bool hasGrowing = false;
        bool hasWatered = false;
        bool hasGrownSlot = false;
        foreach (GardenSlot slot in slots)
        {
            if (slot.IsOccupied && !slot.IsGrown) hasGrowing = true;
            if (slot.IsOccupied && slot.IsGrown)  hasGrownSlot = true;
            if (slot.IsWatered)                   hasWatered = true;
        }
        ConditionContext.SetBool("hasGrowingPlant", hasGrowing);
        ConditionContext.SetBool("hasGrownSlot",    hasGrownSlot);
        ConditionContext.SetBool("hasWatered",      hasWatered);
    }
}
