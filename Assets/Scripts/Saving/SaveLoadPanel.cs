using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    private bool isSaving;

    public void Open(bool saving)
    {
        isSaving = saving;
        headerText.text = isSaving ? "Save Game" : "Load Game";
        PopulateSlots();
    }

    private void PopulateSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        foreach (SaveSlotMeta meta in SaveManager.Instance.GetAllSlotMeta())
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            SaveSlotButton slotButton = slotObj.GetComponent<SaveSlotButton>();
            slotButton.Populate(meta);

            int capturedSlot = meta.slot;
            slotObj.GetComponentInChildren<Button>().onClick.AddListener(() => OnSlotPressed(capturedSlot));
            slotButton.SetDeleteAction(() => OnDeletePressed(capturedSlot));
        }
    }

    private void OnSlotPressed(int slot)
    {
        if (isSaving)
        {
            SaveManager.Instance.Save(slot);
            PopulateSlots();
        }
        else
        {
            GameManager.Instance.LoadGame(slot);
        }
    }

    private void OnDeletePressed(int slot)
    {
        SaveManager.Instance.DeleteSlot(slot);
        PopulateSlots();
    }
}
