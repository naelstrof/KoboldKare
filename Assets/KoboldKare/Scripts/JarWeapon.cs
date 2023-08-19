using System;
using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class JarWeapon : GenericWeapon
{   public delegate void FoodCreateAction(BucketWeapon bucket, ScriptableReagent food);

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

        audioSource.enabled = false;
        waitForSeconds = new WaitForSeconds(5f);
        defaultBucketDisplay.SetActive(true);
 
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
