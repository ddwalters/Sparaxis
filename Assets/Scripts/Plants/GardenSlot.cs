using NodeTree;
using UnityEngine;

public class GardenSlot : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject seedlingVisual;

    private const float GrowDuration = 90f;
    private const float WaterReduction = 45f;

    private Seedling _seedling;
    private float _timeRemaining;
    private bool _watered;
    private bool _grown;
    private bool _occupied;

    public bool IsOccupied => _occupied;
    public bool IsGrown    => _grown;

    public void Plant(Seedling seedling)
    {
        _seedling = seedling;
        _timeRemaining = GrowDuration;
        _watered = false;
        _grown = false;
        _occupied = true;

        if (seedlingVisual != null)
            seedlingVisual.SetActive(true);
    }

    private void Update()
    {
        if (!_occupied || _grown) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
            OnFullyGrown();
    }

    public void OnFocused() { }
    public void OnUnfocused() { }

    public void Interact()
    {
        if (!_occupied)
            return;

        if (!_grown)
        {
            Water();
            return;
        }

        Ship();
    }

    private void Water()
    {
        if (_watered) return;
        _watered = true;
        _timeRemaining = Mathf.Max(0f, _timeRemaining - WaterReduction);

        AudioManager.Instance.PlayWaterSound();
        NodeTreeEvents.Fire("WaterPlant");
    }

    private void OnFullyGrown()
    {
        _grown = true;
        // visual feedback — scale up or tint change handled in editor via animator or here
        if (seedlingVisual != null)
            seedlingVisual.transform.localScale = Vector3.one * 1.5f;
    }

    private void Ship()
    {
        ApplyStatsToEarth();
        _occupied = false;
        _grown = false;
        _seedling = null;

        if (seedlingVisual != null)
        {
            seedlingVisual.transform.localScale = Vector3.one;
            seedlingVisual.SetActive(false);
        }
    }

    private void ApplyStatsToEarth()
    {
        var milestones = SaveManager.Instance.Milestones;
        milestones.earthPercent    += _seedling.effective;
        milestones.earthEfficiency += _seedling.speed;
        milestones.earthGrowthSpeed += _seedling.resistance;
    }
}
