using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ActionListener : MonoBehaviour {
    [SerializeField]
    private InputActionReference action;

    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
    }

    private void OnEnable() {
        action.action.performed += OnPerformed;
    }

    private void OnDisable() {
        action.action.performed -= OnPerformed;
    }

    void OnPerformed(InputAction.CallbackContext ctx) {
        if (EventSystem.current.currentSelectedGameObject == button.gameObject) {
            button.onClick?.Invoke();
        } else if (button.IsInteractable() && (!PopupHandler.instance.PopupIsActive() || button.GetComponentInParent<Popup>() != null)) {
            button.Select();
        }
    }
}
