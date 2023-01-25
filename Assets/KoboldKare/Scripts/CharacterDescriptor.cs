using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;
using NetStack.Serialization;
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CharacterDescriptor))]
public class CharacterDescriptorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var characterDescriptor = (CharacterDescriptor)target;
        if (characterDescriptor.InitializeIfNeeded()) {
            EditorUtility.SetDirty(characterDescriptor);
        }
        if (characterDescriptor.GetDisplayAnimator() != null && GUILayout.Button("Create Ragdoll")) {
            throw new NotImplementedException("Haven't implemented ragdoll automation yet. Sorry!!");
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
    
    [Header("Main settings")]
    [SerializeField] private Animator displayAnimator;

    [SerializeField] private Vector3 colliderOffset;
    [SerializeField] private float colliderHeight = 1.2f;
    [SerializeField] private float colliderRadius = 0.2f;
    
    [SerializeField] private Transform rightKneeHint;
    [SerializeField] private Transform leftKneeHint;
    [SerializeField] private List<SkinnedMeshRenderer> bodyRenderers;
    
    [Header("Special settings")]
    [SerializeField] private AudioPack footLand;
    [SerializeField] private AudioPack footstepPack;
    [SerializeField] private PhysicMaterial spaceLubeMaterial;
    [SerializeField] private VisualEffectAsset circlePoof;
    [SerializeField] private VisualEffectAsset walkDust;
    [SerializeField] private PlayerPossession playerPossessionPrefab;
    [SerializeField] private GameObject handDisplayPrefab;
    [SerializeField] private VisualEffectAsset freezeVFXAsset;
    [SerializeField] private AudioPack unfreezeAudioPack;
    [SerializeField] private TMPro.TMP_Text floatingTextPrefab;
    [SerializeField] private AudioPack chatYowlPack;

    public Animator GetDisplayAnimator() {
        return displayAnimator;
    }

    void Awake() {
        body = gameObject.AddComponent<Rigidbody>();
        body.mass = 10f;
        body.drag = 0f;
        body.angularDrag = 10f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
        gameObject.AddComponent<PhysicsAudio>();
        characterCollider = gameObject.AddComponent<CapsuleCollider>();
        characterCollider.center = colliderOffset;
        characterCollider.height = colliderHeight;
        characterCollider.radius = colliderRadius;
        characterCollider.material = spaceLubeMaterial;
        
        characterController = gameObject.AddComponent<KoboldCharacterController>();
        characterController.footland = footLand;
        characterController.worldModel = displayAnimator.transform;
        characterController.collider = characterCollider;
        characterController.crouchHeight = colliderHeight * 0.5f;

        GameObject circlePoofEffectGameObject = new GameObject("CirclePoof", typeof(VisualEffect));
        circlePoofEffectGameObject.transform.SetParent(displayAnimator.transform);
        circlePoofEffectGameObject.transform.localPosition = Vector3.zero;
        circlePoofEffectGameObject.transform.localRotation = Quaternion.identity;
        VisualEffect circlePoofEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();
        
        GameObject walkDustEffectGameObject = new GameObject("WalkDust", typeof(VisualEffect));
        walkDustEffectGameObject.transform.SetParent(displayAnimator.transform);
        walkDustEffectGameObject.transform.localPosition = Vector3.zero;
        walkDustEffectGameObject.transform.localRotation = Quaternion.identity;
        VisualEffect walkDustEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();
        
        characterAnimator = gameObject.AddComponent<CharacterControllerAnimator>();
        characterAnimator.SetKneeHints(leftKneeHint, rightKneeHint);
        characterAnimator.SetVisualEffectSources(circlePoofEffect, walkDustEffect);
        characterAnimator.SetDefaultFootstepPack(footstepPack);
        characterAnimator.SetPlayerModel(displayAnimator);
        characterAnimator.SetBody(body);
        characterAnimator.SetHeadTransform(displayAnimator.GetBoneTransform(HumanBodyBones.Head));

        grabber = gameObject.AddComponent<Grabber>();
        
        precisionGrabber = gameObject.AddComponent<PrecisionGrabber>();
        precisionGrabber.InitializeWithAssets(handDisplayPrefab, freezeVFXAsset, unfreezeAudioPack);

        List<Collider> ignoreColliders = new List<Collider>();
        ignoreColliders.AddRange(GetDepthOneColliders(displayAnimator, displayAnimator.GetBoneTransform(HumanBodyBones.Chest)));
        ignoreColliders.AddRange(GetDepthOneColliders(displayAnimator, displayAnimator.GetBoneTransform(HumanBodyBones.Neck)));
        ignoreColliders.AddRange(GetDepthOneColliders(displayAnimator, displayAnimator.GetBoneTransform(HumanBodyBones.Head)));
        precisionGrabber.SetIgnoreColliders(ignoreColliders.ToArray());

        koboldInventory = gameObject.AddComponent<KoboldInventory>();
        moneyHolder = gameObject.AddComponent<MoneyHolder>();
    }

    private void Start() {
        var playerPossessionInstance = Instantiate(playerPossessionPrefab, transform);
        possession = playerPossessionInstance.GetComponent<PlayerPossession>();
        thirdPersonMeshDisplay = playerPossessionInstance.GetComponent<ThirdPersonMeshDisplay>();
        thirdPersonMeshDisplay.SetDissolveTargets(bodyRenderers.ToArray());
        
        precisionGrabber.SetView(possession.eyes.transform);
        grabber.SetView(possession.eyes.transform);

        koboldInventory = gameObject.AddComponent<KoboldInventory>();

        chatter = gameObject.AddComponent<Chatter>();

        var floatingTextPrefabInstance = Instantiate(floatingTextPrefab.gameObject, transform);
        chatter.SetTextOutput(floatingTextPrefabInstance.GetComponent<TMPro.TMP_Text>());
        chatter.SetYowlPack(chatYowlPack);

        smoothCharacterPhoton = gameObject.AddComponent<SmoothCharacterPhoton>();
        photonView = GetComponent<PhotonView>();
        photonView.FindObservables();
        possession.gameObject.SetActive(controlType == ControlType.LocalPlayer);
    }

    private List<Collider> GetDepthOneColliders(Animator animator, Transform target) {
        List<Collider> colliders = new List<Collider>();
        foreach (Transform t in target) {
            bool found = false;
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                if (t == target || t != animator.GetBoneTransform((HumanBodyBones)i)) continue;
                found = true;
                break;
            }
            if (found) {
                continue;
            }

            if (t.TryGetComponent(out Collider outCollider)) {
                colliders.Add(outCollider);
            }
        }
        return colliders;
    }

#if UNITY_EDITOR
    public bool InitializeIfNeeded() {
        if (footLand != null || footstepPack != null || spaceLubeMaterial != null || circlePoof != null ||
            walkDust != null || playerPossessionPrefab != null || handDisplayPrefab != null || freezeVFXAsset != null ||
            unfreezeAudioPack != null) return false;
        footLand = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("112a2b2f14f04c1458dded07c0b00fe9"));
        footstepPack = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("23b69436bc3d7a944a598da4b1206fc9"));
        spaceLubeMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(AssetDatabase.GUIDToAssetPath("efd1f3995e4a5bf4d8d9c8ce1d941afb"));
        circlePoof = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(AssetDatabase.GUIDToAssetPath("f20d228138364844893451ae57934590"));
        walkDust = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(AssetDatabase.GUIDToAssetPath("b96b53155ee08af498a985d2b06db2f2"));
        var playerPossessionGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("118737100278bba4f87f35b3e4e0c086"));
        playerPossessionPrefab = playerPossessionGameObject.GetComponent<PlayerPossession>();
        handDisplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("3100fdf2173d9c744a5c465ae3b19715"));
        freezeVFXAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(AssetDatabase.GUIDToAssetPath("3a30a12aee1b7d64e957ed14355d7461"));
        unfreezeAudioPack = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("cd0a89d93b29b8a49942e072d4ff62df"));
        var floatingTextGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("74149ab5c35e0da45a56910118a6dd59"));
        floatingTextPrefab = floatingTextGameObject.GetComponent<TMPro.TMP_Text>();
        chatYowlPack = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("165a7a3804fbb684cba72c68e0264320"));
        photonView = GetComponent<PhotonView>();
        photonView.OwnershipTransfer = OwnershipOption.Request;
        photonView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
        EditorUtility.SetDirty(photonView);
        return true;
    }
    private void OnDrawGizmosSelected() {
        DrawWireCapsule(transform.localToWorldMatrix, colliderOffset+Vector3.up * (colliderHeight-colliderRadius*2f) * 0.5f,
            colliderOffset+Vector3.down * (colliderHeight-colliderRadius*2f) * 0.5f, colliderRadius);
    }
    private static void DrawWireCapsule(Matrix4x4 space, Vector3 upper, Vector3 lower, float radius) {
        using (new Handles.DrawingScope(space)) {
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
        GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(newControlType == ControlType.AIPlayer);
    }

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
