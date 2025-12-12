using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JigglePhysics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;
using Vilar.IK;

public class NetworkedKobold : NetworkBehaviour {
    private List<AsyncOperationHandle> handles = new();
    private readonly SyncVar<string> koboldAssetName = new SyncVar<string>("Kobold");
    private ControlType controlType = ControlType.AIPlayer;
    
    private void Awake() {
        koboldAssetName.OnChange += OnKoboldAssetNameChanged;
        OnKoboldAssetNameChanged("", koboldAssetName.Value, true);
    }

    [ServerRpc]
    public void SetKoboldAssetName(string newName) {
        koboldAssetName.Value = newName;
    }

    private AssetGroup.AssetLocation.AssetHandle<GameObject> koboldAssetHandle;
    private GameObject koboldInstance;

    private bool changingKobold = false;
    public event System.Action<GameObject> koboldFinishedLoading;
    
    public enum ControlType {
        NetworkedPlayer,
        LocalPlayer,
        AIPlayer,
    }

    private void OnKoboldAssetNameChanged(string prev, string next, bool asServer) {
        _ = KoboldAssetNameChangedAsync(prev, next, asServer);
    }

    private async Task KoboldAssetNameChangedAsync(string prev, string next, bool asServer) {
        if (koboldInstance) {
            Destroy(koboldInstance);
        }
        if (koboldAssetHandle != null) {
            koboldAssetHandle.Release();
        }
        
        Debug.Log("waiting for asset");
        koboldAssetHandle = await KoboldKareObjectPostProcessor.GetAssetAsync("PlayableCharacter", next, GameManager.GetErrorKobold());
        Debug.Log("instantiating!!");
        koboldInstance = Instantiate(koboldAssetHandle.asset, transform);
        try {
            Debug.Log("trying to initialize kobold...");
            await TryInitializeKobold(koboldInstance);
        } catch (Exception e) {
            Debug.LogException(e);
            Debug.LogError($"Failed to initialize kobold with name {next}, loading error kobold instead.");
            Destroy(koboldInstance);
            koboldInstance = Instantiate(GameManager.GetErrorKobold(), transform);
            await TryInitializeKobold(koboldInstance);
        }

        var equipmentTasks = new List<Task>();
        if (koboldInstance.TryGetComponent<KoboldInventory>(out var koboldInventory) && koboldInstance.TryGetComponent<CharacterDescriptor>(out var characterDescriptor)) {
            foreach (string equipName in characterDescriptor.GetEquipOnSpawn()) {
                equipmentTasks.Add(koboldInventory.PickupEquipment(equipName, null));
            }
        }
        Debug.Log("waiting for equipments");
        await Task.WhenAll(equipmentTasks);
    }

    private async Task TryInitializeKobold(GameObject koboldGameObject) {
        while (changingKobold) {
            await Task.Delay(1000);
        }
        changingKobold = true;
        try {
            ReleaseHandles();
            if (!koboldGameObject.TryGetComponent(out CharacterDescriptor characterDescriptor)) {
                throw new UnityException("Kobold asset is missing CharacterDescriptor!");
            }

            koboldGameObject.AddComponent<PhysicsAudio>();
            koboldGameObject.AddComponent<MoneyHolder>();
            if (!koboldGameObject.TryGetComponent(out LODGroup lodGroup)) {
                lodGroup = koboldGameObject.AddComponent<LODGroup>();
            }

            var bodyRenderersArray = characterDescriptor.GetBodyRenderers().ToArray();
            lodGroup.SetLODs(new[] { new LOD(0.01f, bodyRenderersArray) });

            foreach (JiggleRigBuilder builder in GetComponentsInChildren<JiggleRigBuilder>()) {
                foreach (var jiggleRig in builder.jiggleRigs) {
                    // reverse-compatiblity for old mods, force animated to true, costs a little performance, oh well!
                    jiggleRig.animated = true;
                }

                // skip tails and other things that have ragdoll properties for LODDING.
                var ragdoller = GetComponent<Ragdoller>();
                if (ragdoller != null && ragdoller.GetDisableRigs().Contains(builder)) {
                    continue;
                }

                if (builder.GetComponent<JiggleRigRendererLOD>() == null) {
                    var lod = builder.gameObject.AddComponent<JiggleRigRendererLOD>();
                    lod.SetRenderers(bodyRenderersArray);
                    lod.SetDistance(25f);
                }
            }

            foreach (JiggleSkin skin in GetComponentsInChildren<JiggleSkin>()) {
                foreach (var jiggleZone in skin.jiggleZones) {
                    // reverse-compatiblity for old mods, force animated to true, costs a little performance, oh well!
                    jiggleZone.animated = true;
                }

                if (skin.GetComponent<JiggleRigRendererLOD>() == null) {
                    var lod = skin.gameObject.AddComponent<JiggleRigRendererLOD>();
                    lod.SetRenderers(bodyRenderersArray);
                    lod.SetDistance(25f);
                }
            }

            var body = koboldGameObject.AddComponent<Rigidbody>();
            body.mass = 25f;
            body.drag = 0f;
            body.angularDrag = 10f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var classicIK = characterDescriptor.GetDisplayAnimator().gameObject.AddComponent<ClassicIK>();
            classicIK.SetAntiPopAndTPose(characterDescriptor.GetTPoseIK(), characterDescriptor.GetAntiPopCurveIK());
            classicIK.enabled = false;

            var characterCollider = koboldGameObject.AddComponent<CapsuleCollider>();
            characterCollider.center = characterDescriptor.GetColliderOffset();
            characterCollider.height = characterDescriptor.GetColliderHeight();
            characterCollider.radius = characterDescriptor.GetColliderRadius();

            var physicsMaterialTask =
                Addressables.LoadAssetAsync<PhysicMaterial>(
                    "Assets/KoboldKare/Scripts/Physics/SpaceLube.physicMaterial");
            handles.Add(physicsMaterialTask);
            characterCollider.material = await physicsMaterialTask.Task;

            var characterController = koboldGameObject.AddComponent<KoboldCharacterController>();
            characterController.stepHeight = characterDescriptor.GetStepHeight();

            var footlandsTask =
                Addressables.LoadAssetAsync<AudioPack>(
                    "Assets/KoboldKare/ScriptableObjects/SoundPacks/FootLands.asset");
            handles.Add(footlandsTask);
            characterController.footland = await footlandsTask.Task;

            characterController.worldModel = characterDescriptor.GetDisplayAnimator().transform;
            characterController.collider = characterCollider;
            characterController.crouchHeight = characterDescriptor.GetColliderHeight() * 0.5f;

            GameObject circlePoofEffectGameObject = new GameObject("CirclePoof", typeof(VisualEffect));
            circlePoofEffectGameObject.transform.SetParent(characterDescriptor.GetDisplayAnimator().transform);
            circlePoofEffectGameObject.transform.localPosition = Vector3.up * 0.1f;
            circlePoofEffectGameObject.transform.localRotation = Quaternion.identity;
            VisualEffect circlePoofEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();

            var circlePoofVFXTask =
                Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/CirclePoof.vfx");
            handles.Add(circlePoofVFXTask);
            circlePoofEffect.visualEffectAsset = await circlePoofVFXTask.Task;

            GameObject walkDustEffectGameObject = new GameObject("WalkDust", typeof(VisualEffect));
            walkDustEffectGameObject.transform.SetParent(characterDescriptor.GetDisplayAnimator().transform);
            walkDustEffectGameObject.transform.localPosition = Vector3.up * 0.1f;
            walkDustEffectGameObject.transform.localRotation = Quaternion.identity;


            VisualEffect walkDustEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();

            var walkDustVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/WalkDust.vfx");
            handles.Add(walkDustVFXTask);
            walkDustEffect.visualEffectAsset = await walkDustVFXTask.Task;

            var characterAnimator = koboldGameObject.AddComponent<CharacterControllerAnimator>();
            characterAnimator.SetPlayerModel(characterDescriptor.GetDisplayAnimator());
            characterAnimator.SetVisualEffectSources(circlePoofEffect, walkDustEffect);

            var defaultFootstepTask =
                Addressables.LoadAssetAsync<AudioPack>(
                    "Assets/KoboldKare/ScriptableObjects/SoundPacks/DefaultFootsteps.asset");
            handles.Add(defaultFootstepTask);
            characterAnimator.SetDefaultFootstepPack(await defaultFootstepTask.Task);
            characterAnimator.SetBody(body);

            var precisionGrabber = koboldGameObject.AddComponent<PrecisionGrabber>();

            var freezeVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/Freeze.vfx");
            var handDisplayPrefabTask =
                Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/koboldhand.prefab");
            var unfreezeAudioPackTask =
                Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Unfreeze.asset");

            handles.Add(freezeVFXTask);
            handles.Add(handDisplayPrefabTask);
            handles.Add(unfreezeAudioPackTask);

            precisionGrabber.InitializeWithAssets(await handDisplayPrefabTask.Task, await freezeVFXTask.Task,
                await unfreezeAudioPackTask.Task);

            var playerPossessionPrefabTask =
                Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/PlayerController.prefab");
            handles.Add(playerPossessionPrefabTask);
            var playerPossessionInstance = Instantiate(await playerPossessionPrefabTask.Task, transform);
            var possession = playerPossessionInstance.GetComponent<PlayerPossession>();

            var chatter = gameObject.AddComponent<Chatter>();

            var floatingTextPrefabTask =
                Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/FloatingText.prefab");
            handles.Add(floatingTextPrefabTask);
            var floatingTextPrefabInstance = Instantiate(await floatingTextPrefabTask.Task, transform);
            chatter.SetTextOutput(floatingTextPrefabInstance.GetComponent<TMPro.TMP_Text>());

            var chatYowlPackTask =
                Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Yowl.asset");
            handles.Add(chatYowlPackTask);
            chatter.SetYowlPack(await chatYowlPackTask.Task);

            possession.gameObject.SetActive(controlType == ControlType.LocalPlayer);
            Physics.SyncTransforms();
            
            characterAnimator.SetHeadTransform(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));
            precisionGrabber.SetView(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));
            
            var grabber = koboldGameObject.AddComponent<Grabber>();
            grabber.SetView(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));

            var koboldAIPossession = GetComponentInChildren<KoboldAIPossession>(true);
            if (koboldAIPossession == null) {
                koboldAIPossession = gameObject.AddComponent<KoboldAIPossession>();
            }
            koboldAIPossession.enabled = controlType == ControlType.AIPlayer;
        
            precisionGrabber.SetIgnoreColliders(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Neck).GetComponentsInChildren<Collider>());
        
            var thirdPersonMeshDisplay = possession.GetComponent<ThirdPersonMeshDisplay>();
            thirdPersonMeshDisplay.SetDissolveTargets(bodyRenderersArray);
            classicIK.Initialize();

            if (koboldGameObject.TryGetComponent<Kobold>(out var koboldComponent)) {
                koboldComponent.SetGenes(koboldComponent.GetGenes());
            }
            
            koboldGameObject.AddComponent<KoboldInventory>();
            koboldFinishedLoading?.Invoke(koboldGameObject);
        } finally {
            changingKobold = false;
        }
    }
    
    public void SetPlayerControlled(ControlType newControlType) {
        // Don't allow multiple players to be set to LocalPlayer
        if (newControlType == ControlType.LocalPlayer && PlayerPossession.TryGetPlayerInstance(out var player)) {
            newControlType = ControlType.AIPlayer;
        }
        
        controlType = newControlType;
        if (!koboldInstance) return;
        GetComponentInChildren<PlayerPossession>(true).gameObject.SetActive(newControlType == ControlType.LocalPlayer);
        GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(newControlType == ControlType.AIPlayer);
        if (!koboldInstance.TryGetComponent<KoboldCharacterController>(out var controller)) return;
        controller.inputDir = Vector3.zero;
        controller.inputJump = false;
    }
    public ControlType GetPlayerControlled() => controlType;
    
    public void SetEyeDir(Vector3 dir) {
        if (controlType == ControlType.LocalPlayer) {
            OrbitCamera.SetPlayerIntendedFacingDirection(dir);
        }
    }

    private void ReleaseHandles() {
        foreach (var handle in handles) {
            Addressables.Release(handle);
        }
        handles.Clear();
    }
    
    private void OnDestroy() {
        ReleaseHandles();
    }


    // FIXME FISHNET
    /*public void OnPhotonInstantiate(PhotonMessageInfo info) {
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
    }*/
}
