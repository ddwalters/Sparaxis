using System.Collections.Generic;
using NodeTree;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    public static GardenManager Instance { get; private set; }

    [SerializeField] private List<GardenSlot> slots;

    public GardenSlot CurrentSlot { get; set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => UpdateGardenContext();

    private void OnEnable()
    {
        SaveManager.OnContextReady += UpdateGardenContext;
        NodeTreeEvents.Subscribe("WaterPlant",   OnWaterPlant);
        NodeTreeEvents.Subscribe("CollectPlant", OnCollectPlant);
    }

    private void OnDisable()
    {
        SaveManager.OnContextReady -= UpdateGardenContext;
        NodeTreeEvents.Unsubscribe("WaterPlant",   OnWaterPlant);
        NodeTreeEvents.Unsubscribe("CollectPlant", OnCollectPlant);
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

    private void OnWaterPlant()
    {
        GardenSlot slot = CurrentSlot ?? GetNearestGrowingSlot(SaveManager.Instance.PlayerPosition);
        slot?.Water();
    }

    private void OnCollectPlant()
    {
        GardenSlot slot = CurrentSlot ?? GetNearestGrownSlot(SaveManager.Instance.PlayerPosition);
        slot?.Ship();
    }

    public GardenSlot GetNearestGrowingSlot(Vector3 position)
    {
        GardenSlot nearest = null;
        float minDist = float.MaxValue;
        foreach (GardenSlot slot in slots)
        {
            if (!slot.IsOccupied || slot.IsGrown) continue;
            float dist = Vector3.Distance(slot.transform.position, position);
            if (dist < minDist) { minDist = dist; nearest = slot; }
        }
        return nearest;
    }

    public GardenSlot GetNearestGrownSlot(Vector3 position)
    {
        GardenSlot nearest = null;
        float minDist = float.MaxValue;
        foreach (GardenSlot slot in slots)
        {
            if (!slot.IsOccupied || !slot.IsGrown) continue;
            float dist = Vector3.Distance(slot.transform.position, position);
            if (dist < minDist) { minDist = dist; nearest = slot; }
        }
        return nearest;
    }

    public GardenSlot GetNearestEmptySlot(Vector3 position)
    {
        GardenSlot nearest = null;
        float minDist = float.MaxValue;
        foreach (GardenSlot slot in slots)
        {
            if (slot.IsOccupied) continue;
            float dist = Vector3.Distance(slot.transform.position, position);
            if (dist < minDist) { minDist = dist; nearest = slot; }
        }
        return nearest;
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
            if (slot.IsOccupied && !slot.IsGrown)
                hasGrowing = true;

            if (slot.IsOccupied && slot.IsGrown)
                hasGrownSlot = true;

            if (slot.IsWatered)
                hasWatered = true;
        }

        ConditionContext.SetBool("hasGrowingPlant", hasGrowing);
        ConditionContext.SetBool("hasGrownSlot", hasGrownSlot);
        ConditionContext.SetBool("hasWatered", hasWatered);
    }
}
