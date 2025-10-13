using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using Photon.Pun;
using UnityEngine.InputSystem;

public class EquipmentUIDisplay : MonoBehaviour {
    [SerializeField] private Transform targetDisplay;
    [SerializeField] private LocalizedString noneString;
    [SerializeField] private LocalizeStringEvent detailDescription;
    [SerializeField] private LocalizeStringEvent detailTitle;
    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Image detailedDisplay;
    private KoboldInventory inventory;
    [SerializeField] private GameObject inventoryUIPrefab;
    [SerializeField] private List<EquipmentSlotDisplay> slots = new List<EquipmentSlotDisplay>();
    [SerializeField] private InputActionReference cancel;
    [SerializeField] private InputActionReference otherCancel;
    [Serializable]
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
        foreach(var slot in slots) {
            EventTrigger et = slot.targetImage.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { DisplayDetail(slot.equipped); });
            et.triggers.Add(entry);
        }
    }

    private void OnEnable() {
        // Try really hard to find the player
        if (PhotonNetwork.LocalPlayer.TagObject is not Kobold kobold) {
            if (PlayerPossession.TryGetPlayerInstance(out var koboldPossession)) {
                kobold = koboldPossession.kobold;
            } else {
                return;
            }
        }
        inventory = kobold.GetComponent<KoboldInventory>();
        inventory.equipmentChanged += UpdateDisplay;
        UpdateDisplay(inventory.GetAllEquipment());
        cancel.action.Enable();
        cancel.action.performed += OnCancel;
        otherCancel.action.Enable();
        otherCancel.action.performed += OnCancel;
    }

    private IEnumerator WaitThenDisable() {
        yield return null;
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
    }

    private void OnCancel(InputAction.CallbackContext obj) {
        StartCoroutine(WaitThenDisable());
    }

    private void OnDisable() {
        if (inventory != null) {
            inventory.equipmentChanged -= UpdateDisplay;
        }
        cancel.action.performed -= OnCancel;
        otherCancel.action.performed -= OnCancel;
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
    private void UpdateDisplay(List<Equipment> equipment) {
        foreach(GameObject g in spawnedUI) {
            Destroy(g);
        }
        foreach (var slot in slots) {
            slot.containerImage.color = Color.white;
            slot.targetImage.sprite = slot.defaultSprite;
            slot.targetImage.color = new Color(0.5f,0.5f,0.8f,0.25f);
            slot.equipped = null;
        }
        foreach(var e in equipment) {
            foreach (var slot in slots) {
                if (slot.slot == e.slot) {
                    slot.containerImage.color = Color.yellow;
                    slot.equipped = e;
                    slot.targetImage.sprite = e.sprite;
                    slot.targetImage.color = Color.white;
                }
            }
            Debug.LogError($"{inventoryUIPrefab}:{targetDisplay}");
            GameObject ui = UnityEngine.Object.Instantiate(inventoryUIPrefab, targetDisplay);
            ui.transform.Find("Label").GetComponent<LocalizeStringEvent>().StringReference = e.localizedName;
            var DropButton = ui.transform.Find("DropButton").GetComponent<Button>();
            DropButton.interactable = e.canManuallyUnequip;
            DropButton.onClick.AddListener(() => {
                if (inventory.photonView.IsMine) {
                    inventory.RemoveEquipment(e, true);
                }
                DisplayDetail(null);
            });
            ui.transform.Find("InspectButton").GetComponent<Button>().onClick.AddListener(() => { DisplayDetail(e); });
            ui.transform.Find("Icon").GetComponent<Image>().sprite = e.sprite;

            EventTrigger et = ui.transform.Find("Icon").gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { DisplayDetail(e); });
            et.triggers.Add(entry);

            spawnedUI.Add(ui);
        }

        if (spawnedUI.Count <= 0) return;
        if (spawnedUI[0] == null) return;
        foreach (Selectable selectable in spawnedUI[0].GetComponentsInChildren<Selectable>()) {
            if (selectable == null || !selectable.IsInteractable()) continue;
            selectable.Select();
            break;
        }
    }
}
