using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JigglePhysics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;
using Vilar.IK;

public class NetworkedKobold : NetworkedEntity {
    private List<AsyncOperationHandle> handles = new();
    private static Collider[] colliders = new Collider[32];
    
    public readonly SyncVar<float> facingRotationY = new SyncVar<float>();
    public readonly SyncVar<Vector2> eyeRot = new SyncVar<Vector2>();
    public readonly SyncVar<Vector2> hipOffset = new SyncVar<Vector2>();
    public readonly SyncVar<byte[]> ragdollBitBuffer = new SyncVar<byte[]>();
    public readonly SyncVar<bool> ragdolled = new SyncVar<bool>();
    private readonly SyncVar<ControlType> controlType = new(ControlType.AIPlayer);
    public readonly SyncVar<float> energy = new SyncVar<float>();
    public readonly SyncVar<float> money = new SyncVar<float>();
    public readonly SyncVar<ReagentContents> metabolizedContents = new SyncVar<ReagentContents>();

    public Quaternion GetFacingRotation() => Quaternion.AngleAxis(facingRotationY.Value, Vector3.up);
    public Vector3 GetFacingDirection() => GetFacingRotation()*Vector3.forward;
    public Vector3 GetEyeDir() => Quaternion.Euler(-eyeRot.Value.y, eyeRot.Value.x, 0) * Vector3.forward;
    public Vector2 GetHipOffset() => hipOffset.Value;
    
    
    protected override void Awake() {
        base.Awake();
        controlType.OnChange += OnControlTypeChanged;
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetEnergy(float newEnergy) {
        energy.Value = newEnergy;
    }

    private void OnControlTypeChanged(ControlType prev, ControlType next, bool asServer) {
        if (next == ControlType.NetworkedPlayer && PlayerPossession.TryGetPlayerInstance(out var player)) {
            next = ControlType.AIPlayer;
        }
        
        if (!koboldInstance || changingKobold) return;
        
        koboldInstance.GetComponentInChildren<PlayerPossession>(true).gameObject.SetActive(next == ControlType.NetworkedPlayer && IsOwner);
        koboldInstance.GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(next == ControlType.AIPlayer);
        if (!koboldInstance.TryGetComponent<KoboldCharacterController>(out var controller)) return;
        controller.inputDir = Vector3.zero;
        controller.inputJump = false;
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void SetControlType(ControlType newControlType) {
        controlType.Value = newControlType;
    }

    public override void SetInstantiationData(KoboldEntitySpawner.NetworkedEntityInstantiationData data) {
        base.SetInstantiationData(data);
        assetPair.Value = new AssetNamePair() {
            groupName = data.groupName,
            assetName = data.assetName,
        };
        // FIXME fishnet
        controlType.Value = ControlType.NetworkedPlayer;
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void SetRagdolled(bool newRagdoll) {
        ragdolled.Value = newRagdoll;
    }
    
    [ServerRpc(RequireOwnership = true, RunLocally = true)]
    public void SetEyeRot(Vector2 newEyeRot) {
        eyeRot.Value = newEyeRot;
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void SetHipOffset(Vector2 newHipOffset) {
        hipOffset.Value = newHipOffset;
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetFacingDirection(Vector3 direction) {
        facingRotationY.Value = Vector3.SignedAngle(direction, Vector3.forward, -Vector3.up);
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void Lactate() {
        koboldInstance.GetComponent<Kobold>().Lactate();
    }

    [ServerRpc(RequireOwnership = true)]
    public void Cum() {
        if (!koboldInstance) {
            return;
        }

        if (!koboldInstance.TryGetComponent<Kobold>(out var kobold)) {
            return;
        }
        
        // FIXME FISHNET
        /*if (kobold.activeDicks.Count == 0) {
            var heartPrefab = kobold.GetHeartPrefab();
            if (!heartPrefab.TryGetAssetGroupAndKey(out var heartGroup, out var heartKey)) {
                return;
            }
            
            bool foundHeart = false;
            int hits = Physics.OverlapSphereNonAlloc(kobold.hip.position, 5f, colliders, kobold.GetHeartHitMask());
            for (int i = 0; i < hits; i++) {
                // Found a nearby heart!
                GenericReagentContainer fruitReagentContainer = colliders[i].GetComponentInParent<GenericReagentContainer>();
                if (fruitReagentContainer != null && fruitReagentContainer.name.Contains(heartKey)) {
                    BitBuffer reagentBuffer = new BitBuffer(16);
                    ReagentContents loveContents = new ReagentContents();
                    if (ReagentDatabase.TryGetAsset("Love", out var loveReagent)) {
                        loveContents.AddMix(loveReagent.GetReagent(10f));
                    }
                    reagentBuffer.AddReagentContents(loveContents);
                    
                    fruitReagentContainer.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, reagentBuffer, photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                    
                    foundHeart = true;
                    break;
                }
            }

            // No nearby hearts, spawn a new one.
            if (!foundHeart) {
                BitBuffer buffer = new BitBuffer(16);
                buffer.AddKoboldGenes(GetGenes());
                PhotonNetwork.Instantiate(heartPrefab.photonName, hip.transform.position, Quaternion.identity, 0, new object[] { buffer });
            }
        }*/
        kobold.Cum();
    }

    private AssetGroup.AssetLocation.AssetHandle<GameObject> koboldAssetHandle;
    
    private GameObject koboldInstance;

    private bool changingKobold = false;
    public event Action<GameObject> koboldFinishedLoading;
    
    public enum ControlType {
        NetworkedPlayer,
        AIPlayer,
    }


    protected override async Task AssetChangedAsync(AssetNamePair prev, AssetNamePair next, bool asServer) {
        if (next.groupName != "PlayableCharacter") {
            await base.AssetChangedAsync(prev, next, asServer);
            return;
        }
        while (changingKobold) {
            await Task.Delay(1000);
        }
        changingKobold = true;
        try {
            if (koboldInstance) {
                Destroy(koboldInstance);
            }

            if (koboldAssetHandle != null) {
                koboldAssetHandle.Release();
            }

            koboldAssetHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(next.groupName, next.assetName, GameManager.GetErrorKobold());
            koboldInstance = Instantiate(koboldAssetHandle.asset, transform);

            try {
                await TryInitializeKobold(koboldInstance);
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to initialize kobold with name {next.groupName}:{next.assetName}, loading error kobold instead.");
                Destroy(koboldInstance);
                koboldInstance = Instantiate(GameManager.GetErrorKobold(), transform);
                await TryInitializeKobold(koboldInstance);
            }

            species.Value = next.assetName;

            var equipmentTasks = new List<Task>();
            if (koboldInstance.TryGetComponent<KoboldInventory>(out var koboldInventory) &&
                koboldInstance.TryGetComponent<CharacterDescriptor>(out var characterDescriptor)) {
                foreach (string equipName in characterDescriptor.GetEquipOnSpawn()) {
                    equipmentTasks.Add(koboldInventory.PickupEquipment(equipName, null));
                }
            }

            await Task.WhenAll(equipmentTasks);
        } finally {
            changingKobold = false;
        }
    }

    private async Task TryInitializeKobold(GameObject koboldGameObject) {
        koboldGameObject.SetActive(false);
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

        foreach (JiggleSkin skin in koboldGameObject.GetComponentsInChildren<JiggleSkin>()) {
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

        if (!gameObject.TryGetComponent<Rigidbody>(out var body)) {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.mass = 25f;
        body.drag = 0f;
        body.angularDrag = 10f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (koboldGameObject.TryGetComponent<Rigidbody>(out var existingBody)) {
            Destroy(existingBody);
        }

        var classicIK = characterDescriptor.GetDisplayAnimator().gameObject.AddComponent<ClassicIK>();
        classicIK.SetAntiPopAndTPose(characterDescriptor.GetTPoseIK(), characterDescriptor.GetAntiPopCurveIK());
        classicIK.enabled = false;

        if (!gameObject.TryGetComponent<CapsuleCollider>(out var characterCollider)) {
            characterCollider = gameObject.AddComponent<CapsuleCollider>();
        }
        characterCollider.center = characterDescriptor.GetColliderOffset();
        characterCollider.height = characterDescriptor.GetColliderHeight();
        characterCollider.radius = characterDescriptor.GetColliderRadius();

        var physicsMaterialTask = Addressables.LoadAssetAsync<PhysicMaterial>( "Assets/KoboldKare/Scripts/Physics/SpaceLube.physicMaterial");
        characterCollider.material = await physicsMaterialTask.Task;
        handles.Add(physicsMaterialTask);

        var characterController = koboldGameObject.AddComponent<KoboldCharacterController>();
        characterController.collider = characterCollider;
        characterController.stepHeight = characterDescriptor.GetStepHeight();
        characterController.worldModel = characterDescriptor.GetDisplayAnimator().transform;

        var footlandsTask =
            Addressables.LoadAssetAsync<AudioPack>(
                "Assets/KoboldKare/ScriptableObjects/SoundPacks/FootLands.asset");
        characterController.footland = await footlandsTask.Task;
        handles.Add(footlandsTask);

        characterController.body = body;
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
        circlePoofEffect.visualEffectAsset = await circlePoofVFXTask.Task;
        handles.Add(circlePoofVFXTask);

        GameObject walkDustEffectGameObject = new GameObject("WalkDust", typeof(VisualEffect));
        walkDustEffectGameObject.transform.SetParent(characterDescriptor.GetDisplayAnimator().transform);
        walkDustEffectGameObject.transform.localPosition = Vector3.up * 0.1f;
        walkDustEffectGameObject.transform.localRotation = Quaternion.identity;


        VisualEffect walkDustEffect = circlePoofEffectGameObject.GetComponent<VisualEffect>();

        var walkDustVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/WalkDust.vfx");
        walkDustEffect.visualEffectAsset = await walkDustVFXTask.Task;
        handles.Add(walkDustVFXTask);

        var characterAnimator = koboldGameObject.AddComponent<CharacterControllerAnimator>();
        characterAnimator.SetPlayerModel(characterDescriptor.GetDisplayAnimator());
        characterAnimator.SetVisualEffectSources(circlePoofEffect, walkDustEffect);

        var defaultFootstepTask = Addressables.LoadAssetAsync<AudioPack>( "Assets/KoboldKare/ScriptableObjects/SoundPacks/DefaultFootsteps.asset");
        var footlands = await defaultFootstepTask.Task;
        handles.Add(defaultFootstepTask);
        characterAnimator.SetDefaultFootstepPack(footlands);
        characterAnimator.SetBody(body);
        characterController.footland = footlands;

        var precisionGrabber = koboldGameObject.AddComponent<PrecisionGrabber>();

        var freezeVFXTask = Addressables.LoadAssetAsync<VisualEffectAsset>("Assets/KoboldKare/VFX/Freeze.vfx");
        var handDisplayPrefabTask = Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/koboldhand.prefab");
        var unfreezeAudioPackTask = Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Unfreeze.asset");

        precisionGrabber.InitializeWithAssets(await handDisplayPrefabTask.Task, await freezeVFXTask.Task, await unfreezeAudioPackTask.Task);
        
        handles.Add(freezeVFXTask);
        handles.Add(handDisplayPrefabTask);
        handles.Add(unfreezeAudioPackTask);

        var playerPossessionPrefabTask =
            Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/PlayerController.prefab");
        var playerPossessionInstance = Instantiate(await playerPossessionPrefabTask.Task, koboldGameObject.transform);
        handles.Add(playerPossessionPrefabTask);
        var possession = playerPossessionInstance.GetComponent<PlayerPossession>();

        var chatter = gameObject.AddComponent<Chatter>();

        var floatingTextPrefabTask =
            Addressables.LoadAssetAsync<GameObject>("Assets/KoboldKare/Prefabs/FloatingText.prefab");
        var floatingTextPrefabInstance = Instantiate(await floatingTextPrefabTask.Task, koboldGameObject.transform);
        handles.Add(floatingTextPrefabTask);
        chatter.SetTextOutput(floatingTextPrefabInstance.GetComponent<TMPro.TMP_Text>());

        var chatYowlPackTask =
            Addressables.LoadAssetAsync<AudioPack>("Assets/KoboldKare/ScriptableObjects/SoundPacks/Yowl.asset");
        chatter.SetYowlPack(await chatYowlPackTask.Task);
        handles.Add(chatYowlPackTask);

        possession.gameObject.SetActive(controlType.Value == ControlType.NetworkedPlayer && IsOwner);
        Physics.SyncTransforms();
        
        koboldGameObject.SetActive(true);
        
        if (!characterDescriptor.GetDisplayAnimator().gameObject.activeInHierarchy) {
            throw new UnityException("DisplayAnimator must be active to find humanoid bones!");
        }
        
        characterAnimator.SetHeadTransform(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));
        precisionGrabber.SetView(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));
        
        var grabber = koboldGameObject.AddComponent<Grabber>();
        grabber.SetView(characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head));

        var koboldAIPossession = GetComponentInChildren<KoboldAIPossession>(true);
        if (koboldAIPossession == null) {
            koboldAIPossession = koboldGameObject.AddComponent<KoboldAIPossession>();
        }
        koboldAIPossession.enabled = controlType.Value == ControlType.AIPlayer;

        var neck = characterDescriptor.GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Neck);
        if (neck) {
            precisionGrabber.SetIgnoreColliders(neck.GetComponentsInChildren<Collider>());
        }

        var thirdPersonMeshDisplay = possession.GetComponent<ThirdPersonMeshDisplay>();
        thirdPersonMeshDisplay.SetDissolveTargets(bodyRenderersArray);
        classicIK.Initialize();

        if (koboldGameObject.TryGetComponent<Kobold>(out var koboldComponent)) {
            koboldComponent.body = body;
        }
        
        koboldGameObject.AddComponent<KoboldInventory>();
        koboldFinishedLoading?.Invoke(koboldGameObject);
    }
    
    public ControlType GetControlType() => controlType.Value;
    
    public void SetEyeDir(Vector3 dir) {
        if (controlType.Value == ControlType.NetworkedPlayer && IsOwner) {
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

    [ServerRpc(RequireOwnership = false)]
    public void GiveMoney(NetworkedEntity moneyPile) {
        money.Value += moneyPile.moneyPileWorth.Value;
        InstanceFinder.ServerManager.Despawn(moneyPile);
    }

    [ServerRpc]
    public void ChargeMoney(NetworkedEntity purchasable) {
        var buyable = purchasable.GetComponentInChildren<GenericPurchasable>();
        if (buyable != null && buyable.inStock) {
            money.Value = Mathf.Max(money.Value - buyable.GetPrice(),0);
        }
    }
    
    public bool TryGetKobold(out Kobold kobold) {
        if (koboldInstance && koboldInstance.TryGetComponent<Kobold>(out kobold)) {
            return true;
        }
        kobold = null;
        return false;
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void StopAnimation() {
        if (koboldInstance && koboldInstance.TryGetComponent<CharacterControllerAnimator>(out var koboldAnimator)) {
            koboldAnimator.StopAnimation();
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void BeginAnimation(NetworkObject obj, int animatorID) {
        if (koboldInstance && koboldInstance.TryGetComponent<CharacterControllerAnimator>(out var koboldAnimator)) {
            IAnimationStationSet set = obj.GetComponentInChildren<IAnimationStationSet>();
            koboldAnimator.BeginAnimation(set, set.GetAnimationStations()[animatorID]);
        }
    }

    public bool IsAnimating() {
        if (koboldInstance && koboldInstance.TryGetComponent<CharacterControllerAnimator>(out var koboldAnimator)) {
            return koboldAnimator.IsAnimating();
        }
        return false;
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
