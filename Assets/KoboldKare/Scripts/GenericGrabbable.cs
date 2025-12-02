using Photon.Pun;
using UnityEngine;

public class GenericGrabbable : MonoBehaviour, IGrabbable {
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
    //public GrabbableType grabbableType;
    public bool CanGrab(Kobold kobold) {
        return true;
    }

    // FIXME FISHNET
    //[PunRPC]
    public void OnGrabRPC(int koboldID) {
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.pickedUpMaterial;
            }
        }
        PhotonProfiler.LogReceive(sizeof(int));
    }

    public void Start() {
        foreach(var pair in rendererMaterialPairs) {
            if (pair == null || pair.renderer == null) {
                continue;
            }
            pair.defaultMaterial = pair.renderer.material;
        }
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        // FIXME FISHNET
        //PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    // FIXME FISHNET
    //[PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
        foreach(var pair in rendererMaterialPairs) {
            if (pair.pickedUpMaterial != null) {
                pair.renderer.material = pair.defaultMaterial;
            }
        }
    }
    public Transform GrabTransform() {
        return center;
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
