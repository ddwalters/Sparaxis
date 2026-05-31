using System;
using TMPro;
using UnityEngine;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI playtimeText;

    public void Populate(SaveSlotMeta meta)
    {
        if (meta.isEmpty)
        {
            mainText.text = $"Slot {meta.slot + 1} — Empty";
            timestampText.text = string.Empty;
            playtimeText.text = string.Empty;
            return;
        }

        mainText.text = $"Slot {meta.slot + 1}";
        timestampText.text = meta.timestamp;
        playtimeText.text = TimeSpan.FromSeconds(meta.playTimeSeconds).ToString(@"hh\:mm\:ss");
    }
}
