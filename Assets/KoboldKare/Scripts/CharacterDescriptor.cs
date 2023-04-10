using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Threading.Tasks;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Vilar.IK;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CharacterDescriptor))]
public class CharacterDescriptorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var characterDescriptor = (CharacterDescriptor)target;
        if (characterDescriptor.InitializeIfNeeded(false)) {
            EditorUtility.SetDirty(characterDescriptor);
        }

        if (GUILayout.Button("Reset default assets")) {
            characterDescriptor.InitializeIfNeeded(true);
            EditorUtility.SetDirty(characterDescriptor);
        }

        if (characterDescriptor.GetDisplayAnimator() != null && GUILayout.Button("Create Ragdoll")) {
            ((CharacterDescriptor)target).CreateBasicRagdoll();
        }
    }
}
#endif

[RequireComponent(typeof(Ragdoller), typeof(PhotonView), typeof(Kobold))]
public class CharacterDescriptor : MonoBehaviour, IPunInstantiateMagicCallback {
    private KoboldCharacterController characterController;
    private CapsuleCollider characterCollider;
    private Rigidbody body;
    private CharacterControllerAnimator characterAnimator;
    private ThirdPersonMeshDisplay thirdPersonMeshDisplay;
    private PlayerPossession possession;
    private MoneyHolder moneyHolder;
    private KoboldInventory koboldInventory;
    private PrecisionGrabber precisionGrabber;
    private Grabber grabber;
    private SmoothCharacterPhoton smoothCharacterPhoton;
    private PhotonView photonView;
    private Chatter chatter;
    private ControlType controlType = ControlType.AIPlayer;
    private Kobold kobold;
    private Vector3 eyeDir;
    private LODGroup lodGroup;
    private bool hideGizmos;
    
    [Header("Main settings")]
    [SerializeField] private Animator displayAnimator;

    [SerializeField] private Vector3 colliderOffset;
    [SerializeField] private float colliderHeight = 1.2f;
    [SerializeField] private float colliderRadius = 0.2f;
    
    [SerializeField] private List<SkinnedMeshRenderer> bodyRenderers;

    private AudioPack footLand;
    private AudioPack footstepPack;
    private PhysicMaterial spaceLubeMaterial;
    private VisualEffectAsset circlePoof;
    private VisualEffectAsset walkDust;
    private PlayerPossession playerPossessionPrefab;
    private GameObject handDisplayPrefab;
    private VisualEffectAsset freezeVFXAsset;
    private AudioPack unfreezeAudioPack;
    private TMPro.TMP_Text floatingTextPrefab;
    private AudioPack chatYowlPack;
    private ClassicIK classicIK;
    private KoboldAIPossession koboldAIPossession;
    
    private List<AsyncOperationHandle> tasks;
    public delegate void FinishedLoadingAssetAction(PhotonView view);

    public event FinishedLoadingAssetAction finishedLoading;
    
    [Header("Special Settings")]
    [SerializeField] private AnimationCurve antiPopCurveIK;
    [SerializeField] private AnimationClip tposeIK;
    private Coroutine coroutine;
    public Animator GetDisplayAnimator() {
        return displayAnimator;
    }

    private void Awake() {
        coroutine = GameManager.StartCoroutineStatic(AwakeRoutine());
    }

    private IEnumerator AwakeRoutine() {
        InitializeImmediately();
        gameObject.SetActive(false);
        yield return new WaitUntil(ModManager.GetReady);
        var task = FindAssetsAsync();
        yield return new WaitUntil(()=>task.IsCompleted);
        InitializePreEnable();
        gameObject.SetActive(true);
        InitializePostEnable();
        finishedLoading?.Invoke(photonView);
    }

    private async Task FindAssetsAsync() {
        tasks = new List<AsyncOperationHandle>();
        var footlandsTask = Addressables.LoadAssetAsync<AudioPack>( "Assets/KoboldKare/ScriptableObjects/SoundPacks/FootLands.asset");
        var defaultFootstepTask =  Addressables.LoadAssetAsync<AudioPack>( "Assets/KoboldKare/ScriptableObjects/SoundPacks/DefaultFootsteps.asset");
        var physicsMaterialTask =  Addressables.LoadAssetAsync<PhysicMaterial>("Assets/KoboldKare/Scripts/Physics/SpaceLube.physicMaterial");
        var circlePoofVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/CirclePoof.vfx");
        var walkDustVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/WalkDust.vfx");
        var freezeVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/Freeze.vfx");
        var playerPossessionPrefabTask = Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/PlayerController.prefab");
        var handDisplayPrefabTask = Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/koboldhand.prefab");
        var unfreezeAudioPackTask = Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Unfreeze.asset");
        var floatingTextPrefabTask = Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/FloatingText.prefab");
        var chatYowlPackTask = Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Yowl.asset");
        tasks.Add(footlandsTask);
        tasks.Add(defaultFootstepTask);
        tasks.Add(physicsMaterialTask);
        tasks.Add(circlePoofVFXTask);
        tasks.Add(walkDustVFXTask);
        tasks.Add(freezeVFXTask);
        tasks.Add(playerPossessionPrefabTask);
        tasks.Add(handDisplayPrefabTask);
        tasks.Add(unfreezeAudioPackTask);
        tasks.Add(floatingTextPrefabTask);
        tasks.Add(chatYowlPackTask);
        await Task.WhenAll(footlandsTask.Task, defaultFootstepTask.Task, physicsMaterialTask.Task,
            circlePoofVFXTask.Task, walkDustVFXTask.Task, freezeVFXTask.Task, playerPossessionPrefabTask.Task,
            handDisplayPrefabTask.Task, unfreezeAudioPackTask.Task, floatingTextPrefabTask.Task, chatYowlPackTask.Task);
        footLand = footlandsTask.Result;
        footstepPack = defaultFootstepTask.Result;
        spaceLubeMaterial = physicsMaterialTask.Result;
        circlePoof = circlePoofVFXTask.Result;
        walkDust = walkDustVFXTask.Result;
        playerPossessionPrefab = playerPossessionPrefabTask.Result.GetComponent<PlayerPossession>();
        handDisplayPrefab = handDisplayPrefabTask.Result;
        freezeVFXAsset = freezeVFXTask.Result;
        unfreezeAudioPack = unfreezeAudioPackTask.Result;
        floatingTextPrefab = floatingTextPrefabTask.Result.GetComponent<TMPro.TMP_Text>();
        chatYowlPack = chatYowlPackTask.Result;
    }

    void InitializeImmediately() {
        body = gameObject.AddComponent<Rigidbody>();
        classicIK = displayAnimator.gameObject.AddComponent<ClassicIK>();
        gameObject.AddComponent<PhysicsAudio>();
        characterCollider = gameObject.AddComponent<CapsuleCollider>();
        characterController = gameObject.AddComponent<KoboldCharacterController>();
        characterAnimator = gameObject.AddComponent<CharacterControllerAnimator>();
        grabber = gameObject.AddComponent<Grabber>();
        koboldInventory = gameObject.AddComponent<KoboldInventory>();
        precisionGrabber = gameObject.AddComponent<PrecisionGrabber>();
        moneyHolder = gameObject.AddComponent<MoneyHolder>();
        smoothCharacterPhoton = gameObject.AddComponent<SmoothCharacterPhoton>();
    }

    void InitializePreEnable() {
        lodGroup = GetComponentInChildren<LODGroup>();
        
        if (lodGroup == null) {
            lodGroup = gameObject.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[] { new LOD(0.01f, bodyRenderers.ToArray()) });
        }

        kobold = GetComponent<Kobold>();
        body.mass = 25f;
        body.drag = 0f;
        body.angularDrag = 10f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        classicIK.SetAntiPopAndTPose(tposeIK, antiPopCurveIK);
        classicIK.enabled = false;
        
        characterCollider.center = colliderOffset;
        characterCollider.height = colliderHeight;
        characterCollider.radius = colliderRadius;
        characterCollider.material = spaceLubeMaterial;
        
        characterController.footland = footLand;
        characterController.worldModel = displayAnimator.transform;
        characterController.collider = characterCollider;
        characterController.crouchHeight = colliderHeight * 0.5f;

        GameObject circlePoofEffectGameObject = new GameObject("CirclePoof", typeof(VisualEffect));
        circlePoofEffectGameObject.transform.SetParent(displayAnimator.transform);
        circlePoofEffectGameObject.transform.localPosition = Vector3.up*0.1f;
        circlePoofEffectGameObject.transform.localRotation = Quaternion.identity;
        VisualEffect circlePoofEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();
        circlePoofEffect.visualEffectAsset = circlePoof;
        
        GameObject walkDustEffectGameObject = new GameObject("WalkDust", typeof(VisualEffect));
        walkDustEffectGameObject.transform.SetParent(displayAnimator.transform);
        walkDustEffectGameObject.transform.localPosition = Vector3.up*0.1f;
        walkDustEffectGameObject.transform.localRotation = Quaternion.identity;
        VisualEffect walkDustEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();
        walkDustEffect.visualEffectAsset = walkDust;
        
        characterAnimator.SetPlayerModel(displayAnimator);
        characterAnimator.SetVisualEffectSources(circlePoofEffect, walkDustEffect);
        characterAnimator.SetDefaultFootstepPack(footstepPack);
        characterAnimator.SetBody(body);

        precisionGrabber.InitializeWithAssets(handDisplayPrefab, freezeVFXAsset, unfreezeAudioPack);
        
        var playerPossessionInstance = Instantiate(playerPossessionPrefab, transform);
        possession = playerPossessionInstance.GetComponent<PlayerPossession>();
        
        chatter = gameObject.AddComponent<Chatter>();

        var floatingTextPrefabInstance = Instantiate(floatingTextPrefab.gameObject, transform);
        chatter.SetTextOutput(floatingTextPrefabInstance.GetComponent<TMPro.TMP_Text>());
        chatter.SetYowlPack(chatYowlPack);

        photonView = GetComponent<PhotonView>();
        photonView.ObservedComponents.Clear();
        photonView.FindObservables(true);
        possession.gameObject.SetActive(controlType == ControlType.LocalPlayer);
        Physics.SyncTransforms();
    }

    void InitializePostEnable() {
        characterAnimator.SetHeadTransform(displayAnimator.GetBoneTransform(HumanBodyBones.Head));
        precisionGrabber.SetView(displayAnimator.GetBoneTransform(HumanBodyBones.Head));
        grabber.SetView(displayAnimator.GetBoneTransform(HumanBodyBones.Head));

        koboldAIPossession = GetComponentInChildren<KoboldAIPossession>(true);
        if (koboldAIPossession == null) {
            koboldAIPossession = gameObject.AddComponent<KoboldAIPossession>();
        }
        koboldAIPossession.enabled = controlType == ControlType.AIPlayer;
        
        precisionGrabber.SetIgnoreColliders(displayAnimator.GetBoneTransform(HumanBodyBones.Neck).GetComponentsInChildren<Collider>());
        
        thirdPersonMeshDisplay = possession.GetComponent<ThirdPersonMeshDisplay>();
        thirdPersonMeshDisplay.SetDissolveTargets(bodyRenderers.ToArray());
        classicIK.Initialize();
        kobold.SetGenes(kobold.GetGenes());
    }

    private void OnDestroy() {
        foreach(var task in tasks) {
            Addressables.Release(task);
        }
        GameManager.StopCoroutineStatic(coroutine);
    }
    public void SetEyeDir(Vector3 dir) {
        eyeDir = dir;
        if (controlType == ControlType.LocalPlayer) {
            OrbitCamera.SetPlayerIntendedFacingDirection(eyeDir);
        }
    }

#if UNITY_EDITOR
    public bool InitializeIfNeeded(bool force) {
        if (force == false && GetComponent<PhotonView>().OwnershipTransfer == OwnershipOption.Request) return false;
        var serializedObject = new SerializedObject(this);
        
        photonView = GetComponent<PhotonView>();
        var photonViewSerializedObject = new SerializedObject(photonView);
        photonViewSerializedObject.FindProperty("OwnershipTransfer").intValue = (int)OwnershipOption.Request;
        photonViewSerializedObject.FindProperty("observableSearch").intValue = (int)PhotonView.ObservableSearch.AutoFindAll;
        var popCurve = new AnimationCurve();
        popCurve.AddKey(new Keyframe { time = 0f, value = 0f, outTangent = 1.3f });
        popCurve.AddKey(new Keyframe { time = 1.1f, value = 1f, inTangent = 0.1f });
        serializedObject.FindProperty("antiPopCurveIK").animationCurveValue = popCurve;
        var TPoseAvatar = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath("46bd2d6ffa5c8c14f850b597913018ee"));
        foreach(var asset in TPoseAvatar) {
            if (asset is not AnimationClip clip || !clip.name.Contains("T-Pose")) continue;
            serializedObject.FindProperty("tposeIK").objectReferenceValue = clip;
            break;
        }

        var genericBounceCurve = AssetDatabase.LoadAssetAtPath<InflatableCurve>(AssetDatabase.GUIDToAssetPath("e18312d1b399ef44cbae03acd0a32afb"));
        var bellyBounceCurve = AssetDatabase.LoadAssetAtPath<InflatableCurve>(AssetDatabase.GUIDToAssetPath("8bb8ec1eabdcb7043a4605858f604a8a"));
        var kobold = GetComponent<Kobold>();
        var koboldSerializedObject = new SerializedObject(kobold);
        koboldSerializedObject.FindProperty("bellyInflater").FindPropertyRelative("bounce").objectReferenceValue = bellyBounceCurve;
        koboldSerializedObject.FindProperty("fatnessInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        koboldSerializedObject.FindProperty("sizeInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        koboldSerializedObject.FindProperty("boobsInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        var displayAnimatorProp = serializedObject.FindProperty("displayAnimator");
        if (displayAnimatorProp.objectReferenceValue == null) {
            displayAnimatorProp.objectReferenceValue = GetComponentInChildren<Animator>();
        }

        if (displayAnimatorProp.objectReferenceValue != null) {
            displayAnimator = displayAnimatorProp.objectReferenceValue as Animator;
            var attachPointArray = koboldSerializedObject.FindProperty("attachPoints");
            CreateOrSetAttachPoint(Equipment.AttachPoint.Chest, displayAnimator.GetBoneTransform(HumanBodyBones.Chest),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.Head, displayAnimator.GetBoneTransform(HumanBodyBones.Head),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.Neck, displayAnimator.GetBoneTransform(HumanBodyBones.Neck),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftCalf,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightCalf,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftForearm,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightForearm,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftHand,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftHand),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightHand,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightHand),
                attachPointArray);
            koboldSerializedObject.FindProperty("hip").objectReferenceValue = displayAnimator.GetBoneTransform(HumanBodyBones.Hips);
        }

        koboldSerializedObject.FindProperty("heartPrefab").FindPropertyRelative("gameObject").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("b47e824ef9dd0654bae5ca33a2d5dd4b"));
        koboldSerializedObject.FindProperty("heartHitMask").intValue = 1 << LayerMask.NameToLayer("UsablePickups");
        koboldSerializedObject.FindProperty("tummyGrumbles").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("67a1644657f256b47ab2a61a75c069d6")); 
        koboldSerializedObject.FindProperty("garglePack").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("2098de8eac6d5e0419986616fa2a8f15")); 
        koboldSerializedObject.FindProperty("milkLactator").FindPropertyRelative("milkSplatMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("3821f9133468bfa449f3dbee8d5a1aff"));
        
        if (displayAnimator != null && displayAnimator.runtimeAnimatorController == null) {
            var defaultAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath("01936098084665e4bb7c834e8c46c5cc"));
            displayAnimator.runtimeAnimatorController = defaultAnimatorController;
            displayAnimator.applyRootMotion = false;
        }

        gameObject.layer = LayerMask.NameToLayer("Player");
        serializedObject.ApplyModifiedProperties();
        photonViewSerializedObject.ApplyModifiedProperties();
        koboldSerializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameObject);
        return true;
    }

    private void CreateOrSetAttachPoint(Equipment.AttachPoint pointReference, Transform targetTransform, SerializedProperty prop) {
        if (targetTransform == null) {
            return;
        }

        for (int i = 0; i < prop.arraySize; i++) {
            var targetProp = prop.GetArrayElementAtIndex(i);
            if (targetProp.FindPropertyRelative("attachPoint").intValue != (int)pointReference) continue;
            targetProp.FindPropertyRelative("targetTransform").objectReferenceValue = targetTransform;
            return;
        }

        prop.InsertArrayElementAtIndex(0);
        var newProp = prop.GetArrayElementAtIndex(0);
        newProp.FindPropertyRelative("attachPoint").intValue = (int)pointReference;
        newProp.FindPropertyRelative("targetTransform").objectReferenceValue = targetTransform;
    }

    private void OnDrawGizmosSelected() {
        if (hideGizmos) {
            return;
        }
        DrawWireCapsule(transform.localToWorldMatrix, colliderOffset+Vector3.up * (colliderHeight-colliderRadius*2f) * 0.5f,
            colliderOffset+Vector3.down * (colliderHeight-colliderRadius*2f) * 0.5f, colliderRadius);
    }
    private static void DrawWireCapsule(Matrix4x4 space, Vector3 upper, Vector3 lower, float radius) {
        using var scope = new Handles.DrawingScope(space);
        var offsetX = new Vector3(radius, 0f, 0f);
        var offsetZ = new Vector3(0f, 0f, radius);
        Handles.DrawWireArc(upper, Vector3.back, Vector3.left, 180, radius);
        Handles.DrawLine(lower + offsetX, upper + offsetX);
        Handles.DrawLine(lower - offsetX, upper - offsetX);
        Handles.DrawWireArc(lower, Vector3.back, Vector3.left, -180, radius);
        Handles.DrawWireArc(upper, Vector3.left, Vector3.back, -180, radius);
        Handles.DrawLine(lower + offsetZ, upper + offsetZ);
        Handles.DrawLine(lower - offsetZ, upper - offsetZ);
        Handles.DrawWireArc(lower, Vector3.left, Vector3.back, 180, radius);
        Handles.DrawWireDisc(upper, Vector3.up, radius);
        Handles.DrawWireDisc(lower, Vector3.up, radius);
    }

    public void CreateBasicRagdoll() {
        hideGizmos = true;
        RagdollCreator.CreateRagdollWizard(displayAnimator).exited += OnCreateBasicRagdoll;
    }
    
    private void OnCreateBasicRagdoll(bool created, Animator animator, RagdollConstraints.HumanoidConstraints constraints, RagdollColliders.HumanoidRagdollColliders colliders) {
        hideGizmos = false;
        if (!created) return;
        var serializedRagdoller = new SerializedObject(GetComponent<Ragdoller>());
        var ragdollBodiesProp = serializedRagdoller.FindProperty("ragdollBodies");
        ragdollBodiesProp.ClearArray();
        foreach (var coll in colliders) {
            var realCollider = coll.Get(animator);
            realCollider.material = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(AssetDatabase.GUIDToAssetPath("aed15ac3b782c8c4a8403ba6c6039f0e"));
            var ragdollRigidbody = realCollider.GetComponentInParent<Rigidbody>();
            bool find = false;
            for (int i = 0; i < ragdollBodiesProp.arraySize; i++) {
                if (ragdollBodiesProp.GetArrayElementAtIndex(i).objectReferenceValue != ragdollRigidbody) continue;
                find = true;
                break;
            }

            realCollider.gameObject.layer = LayerMask.NameToLayer("Hitbox");
            if (find) continue;
            ragdollBodiesProp.InsertArrayElementAtIndex(0);
            ragdollBodiesProp.GetArrayElementAtIndex(0).objectReferenceValue = ragdollRigidbody;
        }

        var hipBody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        serializedRagdoller.FindProperty("hipBody").objectReferenceValue = hipBody;
        serializedRagdoller.ApplyModifiedProperties();
    }

#endif

    public enum ControlType {
        NetworkedPlayer,
        LocalPlayer,
        AIPlayer,
    }

    public void SetPlayerControlled(ControlType newControlType) {
        controlType = newControlType;
        if (possession != null) {
            possession.gameObject.SetActive(newControlType == ControlType.LocalPlayer);
        }

        if (koboldAIPossession != null) {
            koboldAIPossession.enabled = newControlType == ControlType.AIPlayer;
        }

        GetComponent<KoboldCharacterController>().inputDir = Vector3.zero;
        GetComponent<KoboldCharacterController>().inputJump = false;
    }

    public ControlType GetPlayerControlled() => controlType;

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        bool isPlayer = false;
        if (info.photonView.InstantiationData is { Length: > 0 } && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // Might be a shared buffer
            buffer.SetReadPosition(0);
            buffer.ReadKoboldGenes();
            isPlayer = buffer.ReadBool();
        }
        
        if (Equals(info.Sender, PhotonNetwork.LocalPlayer)) {
            SetPlayerControlled(isPlayer ? ControlType.LocalPlayer : ControlType.AIPlayer);
            if (isPlayer) {
                OrbitCamera.SetPlayerIntendedFacingDirection(eyeDir);
            }
        } else {
            SetPlayerControlled(isPlayer ? ControlType.NetworkedPlayer : ControlType.AIPlayer);
        }
        if (!isPlayer) {
            FarmSpawnEventHandler.TriggerProduceSpawn(gameObject);
        } else if (info.Sender != null) {
            info.Sender.TagObject = GetComponent<Kobold>();
        }
    }
}
