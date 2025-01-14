using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements.Experimental;

[System.Serializable]
public class OrbitCameraCharacterBaseConfiguration : OrbitCameraConfiguration {
    [FormerlySerializedAs("shoulderPivot")] [SerializeField]
    private OrbitCameraPivotBase standPivot; //Headpivot
    [SerializeField]
    private OrbitCameraPivotBase crouchPivot;
    [SerializeField]
    private OrbitCameraPivotBase buttPivot;
    [SerializeField]
    private OrbitCameraPivotBase dickPivot;
    [SerializeField]
    private Kobold character;

    //private const float indulgenceWaitDuration = 1f; 
    //private float indulgenceLerp;

    private OrbitCameraData? lastData;
    
    public void SetPivots(OrbitCameraPivotBase shoulder, OrbitCameraPivotBase butt, OrbitCameraPivotBase dick) {
        standPivot = shoulder;
        buttPivot = butt;
        dickPivot = dick;
    }
    
    public override OrbitCameraData GetData(Camera cam) {
        if (standPivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }

        //if (Time.time - lastMovementTime > indulgenceWaitDuration || (character as Player).IsCockvoring()) {
            //indulgenceLerp = Mathf.MoveTowards(indulgenceLerp, 1f, (character as Player).IsCockvoring() ? Time.deltaTime*2f : Time.deltaTime*0.8f); 
            ////Its indulgange, make it a ease with the slow speed
        //} else {
            //indulgenceLerp = Mathf.MoveTowards(indulgenceLerp, 0f, Time.deltaTime*2f);//Return to normal
        //}

        
        const float standOffToSideTarget = 0.33f;

        // The forward unit vector of the camera, goes from (0,-1,0) to (0,1,0) based on how down/up we're looking.
        Vector3 forward = cam.transform.forward;


        float downUpSoft = Easing.InSine(Mathf.Clamp01(-forward.y));
        float downUpSoftReveresed = 1f - Easing.InSine(Mathf.Clamp01(forward.y));


        var standPivotBasic = (standPivot as OrbitCameraPivotBasic);
        if (standPivotBasic != null) {
            standPivotBasic.SetScreenOffset(new Vector2(Mathf.Lerp(standOffToSideTarget, 0.5f, downUpSoft), 0.5f));
            standPivotBasic.SetDesiredDistanceFromPivot(Mathf.Lerp(0.7f, 1.2f, downUpSoft));
            standPivotBasic.SetBaseFOV(65f);
        }

        OrbitCameraData buttCamera = crouchPivot.GetData(cam);
        OrbitCameraData shoulderCamera = standPivot.GetData(cam);

        // Now I construct the "regular" camera, without indulgence, by lerping between a static butt and shoulder cam. No camera wobble!
        OrbitCameraData regularCamera = OrbitCameraData.Lerp(buttCamera,shoulderCamera, downUpSoftReveresed);
        

        OrbitCameraData dickData = dickPivot.GetData(cam);
        OrbitCameraData buttData = buttPivot.GetData(cam);

        // This is a rotation that represents the player camera rotation without pitch. This is important to deal with the fact that looking straight up or down doesn't have a "forward" direction.
        Quaternion rot = Quaternion.AngleAxis(OrbitCamera.GetPlayerIntendedScreenAim().x, Vector3.up);

        // A dot product that goes from 0 to 1, based on if we're looking at the dick or butt. 0 for dick (facing toward the camera), 1 for butt (facing away from camera)
        float forwardBack = (Vector3.Dot(character.transform.forward, rot * Vector3.forward)+1f)/2f;
        // Due to the nature of facing dirrection it is already Sine like
        //Easing.InOutSine((Vector3.Dot(character.GetFacingDirection()*Vector3.forward, rot * Vector3.forward)+1f)/2f);

        //float middleUpwardsSoft = Easing.InOutSine((1f-Mathf.Clamp01(forward.y))*2f);
        //OrbitCameraData ballsDickLerp = OrbitCameraData.Lerp(ballData, dickData, middleUpwardsSoft);

        OrbitCameraData indulgenceCamera = OrbitCameraData.Lerp(dickData, buttData, forwardBack);
        
        //Switches between camera pairs based on the indulgence lerp
        return OrbitCameraData.Lerp(regularCamera, indulgenceCamera, Easing.InOutSine(Mathf.Clamp01(forward.y+0.1f)));
    }
}
