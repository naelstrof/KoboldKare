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
    
    [SerializeField]
    private OrbitCameraPivotBase buttOogle;
    [SerializeField]
    private OrbitCameraPivotBase headOogle;
    
    private Kobold character;
    private KoboldCharacterController controller;
    private CharacterControllerAnimator koboldAnimator; 

    private OrbitCameraData? lastData;

    private LayerMask cullingMask;
    private float forwardBackSoftMemory = 0f;

    public void SetPivots(Kobold character, OrbitCameraPivotBase standPivot, OrbitCameraPivotBase crouchPivot, OrbitCameraPivotBase buttPivotCenter, OrbitCameraPivotBase buttOogle, OrbitCameraPivotBase headOogle) {
        this.standPivot = standPivot;
        this.crouchPivot = crouchPivot;
        this.buttPivotCenter = buttPivotCenter;
        this.character = character;
        this.buttOogle = buttOogle;
        this.headOogle = headOogle;
        controller = character.GetComponent<KoboldCharacterController>();
        koboldAnimator = character.GetComponentInChildren<CharacterControllerAnimator>();
    }

    public override OrbitCameraData GetData(Camera cam) {
        if (standPivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }
        
        // The forward unit vector of the camera, goes from (0,-1,0) to (0,1,0) based on how down/up we're looking.
        Vector3 forward = cam.transform.forward;

        float downUpSoftReversed = 1f - Easing.Sinusoidal.In(Mathf.Clamp01(forward.y));
        float downUpReversed = 1f - Easing.Sinusoidal.InOut(Mathf.Clamp01((forward.y+1f)*0.5f));

        float forwardBackSoft = Easing.Sinusoidal.In(Mathf.Clamp01(Vector3.Dot(-koboldAnimator.GetFacingDirection(), forward)));
        forwardBackSoftMemory = forwardBackSoftMemory.ExpDecay(forwardBackSoft, 0.16f, Time.deltaTime);

        OrbitCameraData crouchCamera = crouchPivot.GetData(cam);
        OrbitCameraData shoulderCamera = standPivot.GetData(cam);
        OrbitCameraData hitmanCamera = OrbitCameraData.Lerp(shoulderCamera,crouchCamera, controller.crouchAmount);
        OrbitCameraData hitmanCameraWithButt = OrbitCameraData.Lerp(buttPivotCenter.GetData(cam),hitmanCamera, downUpSoftReversed);
        
        OrbitCameraData oogleCamera = OrbitCameraData.Lerp(buttOogle.GetData(cam),headOogle.GetData(cam), downUpReversed);
        lastData = OrbitCameraData.Lerp(hitmanCameraWithButt, oogleCamera, forwardBackSoftMemory);

        return lastData.Value;
    }
    
    public override LayerMask GetCullingMask() {
        return cullingMask;
    }

    public void SetCullingMask(LayerMask mask) {
        cullingMask = mask;
    }
}
