using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootstepSoundManager : MonoBehaviour {
    //public List<AudioClip> footstepClips = new List<AudioClip>();
    [SerializeField]
    private AudioPack footstepPack;
    private Animator animator;

    public void SetFootstepPack(AudioPack pack) {
        footstepPack = pack;
    }

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void DoFootstep(AnimationEvent evt) {
        bool groundedAnimation = evt.stringParameter == "Grounded";
        if (groundedAnimation && !animator.GetBool("Grounded")) {
            return;
        }
        if (evt.animatorClipInfo.weight < 0.5f) {
            return;
        }

        Transform f = evt.intParameter == 0 ? animator.GetBoneTransform(HumanBodyBones.LeftFoot) : animator.GetBoneTransform(HumanBodyBones.RightFoot);
        RaycastHit hit;
        AudioClip clip = footstepPack.GetClip();
        bool rayHit = false;
        if (Physics.Raycast(f.position, Vector3.down, out hit, groundedAnimation? 0.8f: 5f, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore)) {
            rayHit = true;
            if (hit.collider != null) {
                TerrainAudio a = hit.collider.GetComponent<TerrainAudio>();
                PhysicMaterial mat = hit.collider.sharedMaterial;
                if (a != null) {
                    mat = a.GetMaterialAtPoint(hit.point);
                }
                var group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(mat);
                if (group != null) {
                    clip = group.GetImpactClip(PhysicsAudioGroup.SurfaceImpactType.Footstep, 1f);
                }
            }
        }
        if (evt.stringParameter == "Airborne" || rayHit) {
            GameManager.instance.SpawnAudioClipInWorld(clip, f.position, evt.floatParameter*0.5f, GameManager.instance.soundEffectGroup);
        }
    }
}
