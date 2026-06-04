using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private Button deleteButton;

    public void Populate(SaveSlotMeta meta)
    {
        if (meta.isEmpty)
        {
            mainText.text = $"Slot {meta.slot + 1} — Empty";
            timestampText.text = string.Empty;
            playtimeText.text = string.Empty;

            if (deleteButton != null)
                deleteButton.gameObject.SetActive(false);

            return;
        }

        mainText.text = $"Slot {meta.slot + 1} - {meta.earthPercent:0.#}%";
        timestampText.text = meta.timestamp;
        playtimeText.text = TimeSpan.FromSeconds(meta.playTimeSeconds).ToString(@"hh\:mm\:ss");

        if (deleteButton != null)
            deleteButton.gameObject.SetActive(true);
    }

    public void SetDeleteAction(Action onDelete)
    {
        if (deleteButton == null)
            return;

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete());
    }
}
