using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ActionButtonListener : MonoBehaviour {
    private static List<ActionButtonListener> actionStack = new List<ActionButtonListener>();

    [SerializeField, SerializeReference, SubclassSelector] private List<GameEventResponse> onButtonPress;

    private Button button;

    public static bool HasAction() => actionStack.Count > 0;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        if (onButtonPress == null) return;
        foreach(var response in onButtonPress) {
            response?.Invoke(this);
        }
    }

    private void OnEnable() {
        var controls = GameManager.GetPlayerControls();
        controls.UI.Cancel.performed += OnPerformed;
        actionStack.Add(this);
    }

    private void OnDisable() {
        var controls = GameManager.GetPlayerControls();
        controls.UI.Cancel.performed -= OnPerformed;
        actionStack.Remove(this);
    }

    void OnPerformed(InputAction.CallbackContext ctx) {
        if (actionStack.Count == 0) return;
        
        if (actionStack[^1] != this) {
            return;
        }
        if (EventSystem.current.currentSelectedGameObject == button.gameObject) {
            button.onClick?.Invoke();
        } else if (button.IsInteractable() && (!PopupHandler.instance.PopupIsActive() || button.GetComponentInParent<Popup>() != null)) {
            button.Select();
        }
    }
}
