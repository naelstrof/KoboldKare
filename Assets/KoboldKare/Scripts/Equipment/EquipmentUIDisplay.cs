using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using Photon.Pun;

public class EquipmentUIDisplay : MonoBehaviourPun {
    public Transform targetDisplay;
    public LocalizedString noneString;
    public LocalizeStringEvent detailDescription;
    public LocalizeStringEvent detailTitle;
    public Sprite noneSprite;
    public Image detailedDisplay;
    public Kobold k;
    public GameObject inventoryUIPrefab;
    public List<EquipmentSlotDisplay> slots = new List<EquipmentSlotDisplay>();
    [System.Serializable]
    public class EquipmentSlotDisplay {
        public Image targetImage;
        public Sprite defaultSprite;
        public Image containerImage;
        public Equipment.EquipmentSlot slot;
        [HideInInspector]
        public Equipment equipped;
    }
    private List<GameObject> spawnedUI = new List<GameObject>();
    void Awake() {
        k.inventory.EquipmentChangedEvent += UpdateDisplay;
        UpdateDisplay(k.inventory, EquipmentInventory.EquipmentChangeSource.Network);
        foreach(var slot in slots) {
            EventTrigger et = slot.targetImage.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { DisplayDetail(slot.equipped); });
            et.triggers.Add(entry);
        }
    }

    private void OnDestroy() {
        if (k != null) {
            k.inventory.EquipmentChangedEvent -= UpdateDisplay;
        }
    }
    public void DisplayDetail(Equipment e) {
        if (e == null) {
            detailedDisplay.sprite = noneSprite;
            detailDescription.StringReference = noneString;
            detailTitle.StringReference = noneString;
            return;
        }
        detailedDisplay.sprite = e.sprite;
        detailDescription.StringReference = e.localizedDescription;
        detailTitle.StringReference = e.localizedName;
    }
    public void UpdateDisplay(EquipmentInventory inventory, EquipmentInventory.EquipmentChangeSource source) {
        foreach(GameObject g in spawnedUI) {
            Destroy(g);
        }
        foreach (var slot in slots) {
            slot.containerImage.color = Color.white;
            slot.targetImage.sprite = slot.defaultSprite;
            slot.targetImage.color = new Color(0.5f,0.5f,0.8f,0.25f);
            slot.equipped = null;
        }
        foreach(var e in inventory.equipment) {
            foreach (var slot in slots) {
                if (slot.slot == e.equipment.slot) {
                    slot.containerImage.color = Color.yellow;
                    slot.equipped = e.equipment;
                    slot.targetImage.sprite = e.equipment.sprite;
                    slot.targetImage.color = Color.white;
                }
            }
            GameObject ui = GameObject.Instantiate(inventoryUIPrefab, targetDisplay);


            ui.transform.Find("Label").GetComponent<LocalizeStringEvent>().StringReference = e.equipment.localizedName;
            // DropEquipment RPC is found in Kobold.cs
            ui.transform.Find("DropButton").GetComponent<Button>().onClick.AddListener(() => {
                if (k.photonView.IsMine) {
                    int id = 0;
                    for (int i = 0; i < k.inventory.equipment.Count; i++) {
                        if (k.inventory.equipment[i] == e) {
                            id = i;
                            break;
                        }
                    }
                    k.inventory.RemoveEquipment(e, EquipmentInventory.EquipmentChangeSource.Drop);
                    SaveManager.RPC(k.photonView, "DropEquipment", RpcTarget.OthersBuffered, new object[] { id });
                }
                DisplayDetail(null);
            });
            ui.transform.Find("InspectButton").GetComponent<Button>().onClick.AddListener(() => { DisplayDetail(e.equipment); });
            ui.transform.Find("Icon").GetComponent<Image>().sprite = e.equipment.sprite;

            EventTrigger et = ui.transform.Find("Icon").gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { DisplayDetail(e.equipment); });
            et.triggers.Add(entry);

            spawnedUI.Add(ui);
        }
    }
}
