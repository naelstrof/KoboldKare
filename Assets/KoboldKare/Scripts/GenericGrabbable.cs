using Photon.Pun;
using UnityEngine;

public class GenericGrabbable : MonoBehaviour {
    [System.Serializable]
    public class RendererMaterialPair {
        public Renderer renderer;
        public Material pickedUpMaterial;
        [HideInInspector]
        public Material defaultMaterial;
    }
    public RendererMaterialPair[] rendererMaterialPairs;
    public Renderer[] renderers;
    public Transform center;

    private NetworkedEntity networkedEntity;
    
    public bool OnGrabRequested(NetworkedKobold by) {
        return true;
    }

    private void OnGrab(NetworkedKobold by) {
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.pickedUpMaterial;
            }
        }
    }

    public void Start() {
        foreach(var pair in rendererMaterialPairs) {
            if (pair == null || pair.renderer == null) {
                continue;
            }
            pair.defaultMaterial = pair.renderer.material;
        }
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        networkedEntity.grabbed += OnGrab;
        networkedEntity.released += OnRelease;
        networkedEntity.grabRequested += OnGrabRequested;
        networkedEntity.SetGrabTransform(center != null ? center : transform);
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        // FIXME FISHNET
        //PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    private void OnRelease(NetworkedKobold by, Vector3 velocity) {
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.defaultMaterial;
            }
        }
    }

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
