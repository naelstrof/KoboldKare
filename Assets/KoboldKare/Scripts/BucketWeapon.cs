using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BucketWeapon : GenericWeapon {
    [SerializeField]
    private PhotonGameObjectReference bucketSplashProjectile;
    [SerializeField]
    private GenericReagentContainer container;
    [SerializeField]
    private Animator bucketAnimator;

    [SerializeField]
    private Rigidbody body;

    private static readonly int Fire = Animator.StringToHash("Fire");
    private Kobold playerFired;

    [SerializeField] private AudioPack bucketSlosh;
    private AudioSource audioSource;

    private WaitForSeconds waitForSeconds;

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
    }

    public override void OnFire(GameObject player) {
        base.OnFire(player);
        bucketAnimator.SetTrigger(Fire);
        playerFired = player.GetComponentInParent<Kobold>();
    }

    public void OnFireComplete() {
        if (container.volume > 0.1f) {

            Vector3 velocity = GetWeaponBarrelTransform().forward * 10f;
            if (playerFired != null) {
                velocity += playerFired.body.velocity * 0.5f;
            }
            GameObject obj = PhotonNetwork.Instantiate(bucketSplashProjectile.photonName,
                GetWeaponBarrelTransform().position,
                GetWeaponBarrelTransform().rotation, 0, new object[] { container.Spill(10f), velocity });
            obj.GetComponent<Projectile>().LaunchFrom(body);
            audioSource.enabled = true;
            bucketSlosh.Play(audioSource);
        }
    }

    IEnumerator WaitSomeTimeThenDisableAudio() {
        yield return waitForSeconds;
        audioSource.enabled = false;
    }

    private void OnValidate() {
        bucketSplashProjectile.OnValidate();
    }
}
