using NodeTree;
using UnityEngine;

public class GardenSlot : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite plantIndicatorSprite;

    private const float GrowDuration    = 0f;
    private const float WaterReduction  = 45f;
    private const float IndicatorStartScale = 0.08f;
    private const float IndicatorFullScale  = 0.15f;

    private Seedling _seedling;
    private float _timeRemaining;
    private bool _watered;
    private bool _grown;
    private bool _occupied;
    private SpriteRenderer _indicator;

    public bool IsOccupied => _occupied;
    public bool IsGrown    => _grown;
    public bool IsWatered  => _watered;

    public void Plant(Seedling seedling)
    {
        _seedling = seedling;
        _timeRemaining = GrowDuration;
        _watered = false;
        _grown = false;
        _occupied = true;
        ConditionContext.SetBool("hasWatered", false);

        GameObject obj = new GameObject("PlantIndicator");
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        _indicator = obj.AddComponent<SpriteRenderer>();
        _indicator.sprite = plantIndicatorSprite;
        _indicator.color = Color.yellow;
        _indicator.sortingOrder = 11;
        _indicator.transform.localScale = Vector3.one * IndicatorStartScale;
    }

    private void Update()
    {
        if (!_occupied || _grown) return;

        _timeRemaining -= Time.deltaTime;

        if (GrowDuration > 0f && _indicator != null)
        {
            float progress = 1f - (_timeRemaining / GrowDuration);
            float scale = Mathf.Lerp(IndicatorStartScale, IndicatorFullScale, progress);
            _indicator.transform.localScale = Vector3.one * scale;
        }

        if (_timeRemaining <= 0f)
            OnFullyGrown();
    }

    public void OnFocused() { }
    public void OnUnfocused() { }

    public void Interact()
    {
        if (!_occupied) return;
        if (!_grown) { Water(); return; }
        Ship();
    }

    private void Water()
    {
        if (_watered) return;
        _watered = true;
        _timeRemaining = Mathf.Max(0f, _timeRemaining - WaterReduction);

        AudioManager.Instance.PlayWaterSound();
        NodeTreeEvents.Fire("WaterPlant");
        ConditionContext.SetBool("hasWatered", true);
    }

    private void OnFullyGrown()
    {
        _grown = true;
        if (_indicator != null)
        {
            _indicator.color = Color.green;
            _indicator.transform.localScale = Vector3.one * IndicatorFullScale;
        }
        GardenManager.Instance.UpdateGardenContext();
    }

    public void Ship()
    {
        Debug.Log($"[GardenSlot] Ship called. seedling={_seedling?.name ?? "null"}");
        GardenManager.Instance.OnPlantHarvested(_seedling);
        _occupied = false;
        _grown = false;
        _watered = false;
        _seedling = null;

        if (_indicator != null)
        {
            Destroy(_indicator.gameObject);
            _indicator = null;
        }

        ConditionContext.SetBool("hasWatered", false);
    }
}
