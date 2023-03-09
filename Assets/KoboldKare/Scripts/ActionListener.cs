using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionListener : MonoBehaviour {
    [SerializeField]
    private InputActionReference action;
    [SerializeField]
    private UnityEvent onPerformed;
    private void OnEnable() {
        action.action.performed += OnPerformed;
    }

    private void OnDisable() {
        action.action.performed -= OnPerformed;
    }

    void OnPerformed(InputAction.CallbackContext ctx) {
        onPerformed?.Invoke();
    }
}
