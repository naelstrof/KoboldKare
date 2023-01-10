using System;
using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class BucketWeapon : GenericWeapon {
    public delegate void FoodCreateAction(BucketWeapon bucket, ScriptableReagent food);

    public static event FoodCreateAction foodCreated;
    [SerializeField]
    private PhotonGameObjectReference bucketSplashProjectile;
    [SerializeField]
    private GenericReagentContainer container;
    [SerializeField]
    private Animator bucketAnimator;
    [SerializeField]
    private GameObject defaultBucketDisplay;

    [SerializeField]
    private Rigidbody body;

    private static readonly int Fire = Animator.StringToHash("Fire");
    private Kobold playerFired;

    [SerializeField] private AudioPack bucketSlosh;
    private AudioSource audioSource;

    private WaitForSeconds waitForSeconds;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float projectileVolume = 10f;

    private GameObject currentDisplay;

    void Start() {
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.maxDistance = 20f;
            audioSource.minDistance = 0.2f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
        }

        container.OnChange.AddListener(OnReagentsChanged);
        audioSource.enabled = false;
        waitForSeconds = new WaitForSeconds(5f);
        defaultBucketDisplay.SetActive(true);
        OnReagentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
    }

    private void OnDestroy() {
        container.OnChange.RemoveListener(OnReagentsChanged);
    }

    void OnReagentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        GameObject bestDisplay = null;
        float bestVolume = 0f;
        byte bestID = 0;
        foreach (var reagent in contents) {
            if (ReagentDatabase.GetReagent(reagent.id).GetDisplayPrefab() == null) {
                continue;
            }
            if (reagent.volume < 5f) {
                continue;
            }
            if (reagent.volume > bestVolume) {
                bestDisplay = ReagentDatabase.GetReagent(reagent.id).GetDisplayPrefab();
                bestVolume = reagent.volume;
                bestID = reagent.id;
            }
        }

        if ((bestDisplay == null && currentDisplay != null) || (currentDisplay != null && bestDisplay != null && !currentDisplay.name.Contains(bestDisplay.name))) {
            Destroy(currentDisplay);
            currentDisplay = null;
            defaultBucketDisplay.SetActive(true);
        }

        if (bestDisplay != null && currentDisplay == null) {
            foodCreated?.Invoke(this, ReagentDatabase.GetReagent(bestID));
            currentDisplay = Instantiate(bestDisplay, transform);
            defaultBucketDisplay.SetActive(false);
        }
    }

    [PunRPC]
    protected override void OnFireRPC(int viewID) {
        base.OnFireRPC(viewID);
        bucketAnimator.SetTrigger(Fire);
        playerFired = PhotonNetwork.GetPhotonView(viewID).GetComponentInParent<Kobold>();
        PhotonProfiler.LogReceive(sizeof(int));
    }

    // Called from the animator
    public void OnFireComplete() {
        if (!photonView.IsMine) {
            return;
        }

        if (container.volume < 0.1f) {
            return;
        }
        for (int i = 0; i < projectileCount; i++) {
            Vector3 velocity = GetWeaponBarrelTransform().forward * 10f;
            if (playerFired != null) {
                velocity += playerFired.body.velocity * 0.5f;
            }

            velocity += Random.insideUnitSphere * i * 2f;
            BitBuffer instantiationData = new BitBuffer(16);
            instantiationData.AddReagentContents(container.Spill(projectileVolume));
            instantiationData.AddUShort(HalfPrecision.Quantize(velocity.x));
            instantiationData.AddUShort(HalfPrecision.Quantize(velocity.y));
            instantiationData.AddUShort(HalfPrecision.Quantize(velocity.z));
            instantiationData.AddKoboldGenes(container.GetGenes());
            GameObject obj = PhotonNetwork.Instantiate(bucketSplashProjectile.photonName,
                GetWeaponBarrelTransform().position,
                GetWeaponBarrelTransform().rotation, 0, new object[] { instantiationData });
            obj.GetComponent<Projectile>().LaunchFrom(body);
        }

        audioSource.enabled = true;
        bucketSlosh.Play(audioSource);
    }

    IEnumerator WaitSomeTimeThenDisableAudio() {
        yield return waitForSeconds;
        audioSource.enabled = false;
    }

    private void OnValidate() {
        bucketSplashProjectile.OnValidate();
    }
}
