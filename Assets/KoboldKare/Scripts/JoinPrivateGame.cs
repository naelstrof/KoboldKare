using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JoinPrivateGame : MonoBehaviour {
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => PopupHandler.instance.SpawnPopup("JoinPrivateGame"));
    }
}
