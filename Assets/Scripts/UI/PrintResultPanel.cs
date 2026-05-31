using TMPro;
using UnityEngine;

public class PrintResultPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI plantNameText;
    [SerializeField] private TextMeshProUGUI effectiveText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI resistanceText;

    public void Show(Seedling seedling)
    {
        plantNameText.text   = seedling.name;
        effectiveText.text   = $"Effective:   {seedling.effective:P0}";
        speedText.text       = $"Speed:       {seedling.speed:P0}";
        resistanceText.text  = $"Resistance:  {seedling.resistance:P0}";

        UIManager.Instance.ShowPrintResult();
    }

    public void Dismiss()
    {
        UIManager.Instance.ShowHUD();
    }
}
