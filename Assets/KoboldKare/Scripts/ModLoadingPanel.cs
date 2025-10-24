using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModLoadingPanel : MonoBehaviour {
    [SerializeField]
    private CanvasGroup group;
    void Update() {
        group.alpha = ModManager.GetReady() ? 0f : 1f;
    }
}
