using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class KoboldEvent : UnityEvent<Kobold> {}
public class GenericGrabbable : MonoBehaviourPun, IGrabbable {
    [System.Serializable]
    public class RendererMaterialPair {
        public Renderer renderer;
        public Material pickedUpMaterial;
        [HideInInspector]
        public Material defaultMaterial;
    }
    public RendererMaterialPair[] rendererMaterialPairs;
    public KoboldEvent onGrab;
    public KoboldEvent onRelease;
    public KoboldEvent onThrow;
    public Rigidbody[] bodies;
    public Renderer[] renderers;
    public Transform center;
    //public GrabbableType grabbableType;
    public bool OnGrab(Kobold kobold) {
        onGrab.Invoke(kobold);
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.pickedUpMaterial;
            }
        }
        if (kobold.photonView.IsMine) {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        return true;
    }

    public void Start() {
        foreach(var pair in rendererMaterialPairs) {
            pair.defaultMaterial = pair.renderer.material;
        }
    }

    public void OnRelease(Kobold kobold) {
        onRelease.Invoke(kobold);
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.defaultMaterial;
            }
        }
    }
    public void OnThrow(Kobold kobold) {
        onThrow.Invoke(kobold);
    }
    public Vector3 GrabOffset() {
        return Vector3.zero;
    }

    public Rigidbody[] GetRigidBodies() {
        return bodies;
    }

    public Renderer[] GetRenderers() {
        return renderers;
    }

    public Transform GrabTransform(Rigidbody r) {
        return center;
    }

    //Deprecated
    /*public GrabbableType GetGrabbableType() {
        return grabbableType;
    }*/
    void OnValidate() {
        if (renderers == null) {
            return;
        }
        if (rendererMaterialPairs == null || rendererMaterialPairs.Length != renderers.Length) {
            rendererMaterialPairs = new RendererMaterialPair[renderers.Length];
            for(int i=0;i<renderers.Length;i++) {
                rendererMaterialPairs[i] = new RendererMaterialPair();
                rendererMaterialPairs[i].renderer = renderers[i];
                rendererMaterialPairs[i].defaultMaterial = renderers[i].sharedMaterial;
            }
        }
        foreach(var pair in rendererMaterialPairs) {
            if (pair.renderer != null && pair.defaultMaterial == null) {
                pair.defaultMaterial = pair.renderer.sharedMaterial;
            }
        }
    }
}
