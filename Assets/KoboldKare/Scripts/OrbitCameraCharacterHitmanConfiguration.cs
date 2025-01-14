using Naelstrof.Easing;
using UnityEngine;

[System.Serializable]
public class OrbitCameraCharacterHitmanConfiguration : OrbitCameraConfiguration {
    [SerializeField]
    private OrbitCameraPivotBase standPivot; //Headpivot
    [SerializeField]
    private OrbitCameraPivotBase crouchPivot;
    [SerializeField]
    private OrbitCameraPivotBase buttPivotCenter;
    
    private Kobold character;
    private KoboldCharacterController controller;

    private OrbitCameraData? lastData;

    private LayerMask cullingMask;

    public void SetPivots(Kobold character, OrbitCameraPivotBase standPivot, OrbitCameraPivotBase crouchPivot, OrbitCameraPivotBase buttPivotCenter) {
        this.standPivot = standPivot;
        this.crouchPivot = crouchPivot;
        this.buttPivotCenter = buttPivotCenter;
        this.character = character;
        controller = character.GetComponent<KoboldCharacterController>();
    }

    public override OrbitCameraData GetData(Camera cam) {
        if (standPivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }
        
        // The forward unit vector of the camera, goes from (0,-1,0) to (0,1,0) based on how down/up we're looking.
        Vector3 forward = cam.transform.forward;

        float downUpSoftReveresed = 1f - Easing.Sinusoidal.In(Mathf.Clamp01(forward.y));

        OrbitCameraData crouchCamera = crouchPivot.GetData(cam);
        OrbitCameraData shoulderCamera = standPivot.GetData(cam);
        OrbitCameraData hitmanCamera = OrbitCameraData.Lerp(shoulderCamera,crouchCamera, controller.crouchAmount);
        OrbitCameraData hitmanCameraWithButt = OrbitCameraData.Lerp(buttPivotCenter.GetData(cam),hitmanCamera, downUpSoftReveresed);
        lastData = hitmanCameraWithButt;

        return lastData.Value;
    }
    
    public override LayerMask GetCullingMask() {
        return cullingMask;
    }

    public void SetCullingMask(LayerMask mask) {
        cullingMask = mask;
    }
}
