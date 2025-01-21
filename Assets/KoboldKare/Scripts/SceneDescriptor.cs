using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ObjectiveManager))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PlayAreaEnforcer))]
[RequireComponent(typeof(OcclusionArea))]
[RequireComponent(typeof(MusicManager))]
[RequireComponent(typeof(DayNightCycle))]
[RequireComponent(typeof(CloudPool))]
public class SceneDescriptor : OrbitCameraPivotBase {
    private static SceneDescriptor instance;
    
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField] private bool canGrabFly = true;
    [SerializeField, SerializeReference, SerializeReferenceButton, HideInInspector] private OrbitCameraConfiguration baseCameraConfiguration;
    private AudioListener audioListener;
    private OrbitCamera orbitCamera;

    public override OrbitCameraData GetData(Camera cam) {
        return baseCameraConfiguration?.GetData(cam) ?? new OrbitCameraData(){};
    }

    protected override void Awake() {
        base.Awake();
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) {
            //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
        
        var configuration = new OrbitCameraBasicConfiguration();
        var pivot = new GameObject("OrbitCameraPivot", typeof(OrbitCameraPivotBasic));
        configuration.SetPivot(pivot.GetComponent<OrbitCameraPivotBase>());
        OrbitCamera.AddConfiguration(configuration);
        
        var orbitCamera = new GameObject("OrbitCamera", typeof(Camera), typeof(UniversalAdditionalCameraData), typeof(OrbitCamera), typeof(AudioListener), typeof(CameraConfigurationListener)) {
            layer = LayerMask.NameToLayer("Default")
        };
        orbitCamera.tag = "MainCamera";

    }

    public static void GetSpawnLocationAndRotation(out Vector3 position, out Quaternion rotation) {
        if (instance == null || instance.spawnLocations == null || instance.spawnLocations.Length == 0) {
            Debug.Log(instance);
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }
        var t = instance.spawnLocations[Random.Range(0, instance.spawnLocations.Length)];
        Vector3 flattenedForward = t.forward.With(y:0);
        if (flattenedForward.magnitude == 0) {
            flattenedForward = Vector3.forward;
        }
        rotation = Quaternion.FromToRotation(Vector3.forward,flattenedForward.normalized); 
        position = t.position;
    }
    public static bool CanGrabFly() {
        return instance == null || instance.canGrabFly;
    }
}
