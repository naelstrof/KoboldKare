using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBeam : MonoBehaviour {
    private Camera targetCamera;
    [SerializeField,Range(0f,1f)]
    private float scaleFactor = 1/10f;
    [SerializeField]
    private Renderer targetRenderer;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int Visibility = Shader.PropertyToID("_Visibility");

    void Start() {
        // This disrupts SaveManager from understanding what prefab we are, we don't want to be saved
        // I hope vilar never sees this.
        name = "SpaceBeamDontSave";
        propertyBlock = new MaterialPropertyBlock();
    }

    void Update() {
        if (targetCamera == null || targetCamera.enabled == false) {
            targetCamera = Camera.main;
        }
        if (targetCamera == null || targetCamera.enabled == false) {
            targetCamera = Camera.current;
        }

        if (targetCamera == null || targetCamera.enabled == false) {
            return;
        }
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        transform.localScale = Vector3.one * Mathf.Max(distance*scaleFactor*scaleFactor,1f);
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(Visibility, Mathf.Clamp01(distance * scaleFactor * scaleFactor));
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
