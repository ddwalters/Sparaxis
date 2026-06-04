using System.Collections;
using NodeTree;
using UnityEngine;

[RequireComponent(typeof(NodeTreeTrigger))]
public class GardenSlot : MonoBehaviour, IInteractable
{

    private const float GrowDuration    = 10f;
    private const float WaterReduction  = 45f;
    private const float IndicatorStartScale = 0.08f;
    private const float IndicatorFullScale  = 0.15f;

    private Seedling _seedling;
    private float _timeRemaining;
    private bool _watered;
    private bool _grown;
    private bool _occupied;
    private SpriteRenderer _indicator;
    private NodeTreeTrigger _trigger;

    private void Awake()
    {
        _trigger = GetComponent<NodeTreeTrigger>();
    }

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

        GameObject obj = new GameObject("PlantIndicator");
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        _indicator = obj.AddComponent<SpriteRenderer>();
        _indicator.color = Color.white;
        _indicator.sortingOrder = 11;
        _indicator.transform.localScale = Vector3.one * IndicatorStartScale;
        SetIndicatorSprite(0f);
    }

    private void Update()
    {
        if (!_occupied || _grown) return;

        _timeRemaining -= Time.deltaTime;

        if (GrowDuration > 0f && _indicator != null)
        {
            float progress = 1f - (_timeRemaining / GrowDuration);
            _indicator.transform.localScale = Vector3.one * Mathf.Lerp(IndicatorStartScale, IndicatorFullScale, progress);
            SetIndicatorSprite(progress);
        }

        if (_timeRemaining <= 0f)
            OnFullyGrown();
    }

    public void OnFocused() { }
    public void OnUnfocused() { }

    public void Interact()
    {
        GardenManager.Instance.CurrentSlot = this;
        SetSlotContext();
        SaveManager.Instance.ApplyMilestonesToContext();
        UIManager.Instance.ShowDialog();
        _trigger.Interact();
        StartCoroutine(WaitForDialogEnd());
    }

    private IEnumerator WaitForDialogEnd()
    {
        yield return new WaitUntil(() => !DialogRunner.Instance.IsDialogActive);
        GardenManager.Instance.UpdateGardenContext();
        UIManager.Instance.ShowHUD();
    }

    private void SetSlotContext()
    {
        ConditionContext.SetBool("hasGrowingPlant", _occupied && !_grown);
        ConditionContext.SetBool("hasGrownSlot",    _occupied && _grown);
        ConditionContext.SetBool("hasWatered",      _watered);
    }

    public void Water()
    {
        if (_watered) return;
        _watered = true;
        _timeRemaining = Mathf.Max(0f, _timeRemaining - WaterReduction);

        AudioManager.Instance.PlayWaterSound();
        ConditionContext.SetBool("hasWatered", true);
    }

    private void SetIndicatorSprite(float progress)
    {
        if (_indicator == null || _seedling?.sourcePlant == null) return;
        PlantData plant = _seedling.sourcePlant;
        int stage = Mathf.Min(3, Mathf.FloorToInt(progress * 4));
        _indicator.sprite = stage switch
        {
            0 => plant.seedlingSprite,
            1 => plant.younglingSprite,
            2 => plant.maturingSprite,
            _ => plant.grownSprite
        };
    }

    private void OnFullyGrown()
    {
        _grown = true;
        if (_indicator != null)
        {
            _indicator.transform.localScale = Vector3.one * IndicatorFullScale;
            SetIndicatorSprite(1f);
        }
        GardenManager.Instance.UpdateGardenContext();
    }

    public void Ship()
    {
        Debug.Log($"[GardenSlot] Ship called. seedling={_seedling?.name ?? "null"}");
        Seedling harvested = _seedling;
        _occupied = false;
        _grown = false;
        _watered = false;
        _seedling = null;

        if (_indicator != null)
        {
            Destroy(_indicator.gameObject);
            _indicator = null;
        }

        GardenManager.Instance.OnPlantHarvested(harvested);
    }
}
