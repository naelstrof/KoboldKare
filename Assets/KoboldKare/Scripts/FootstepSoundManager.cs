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
    private static readonly int Grounded = Animator.StringToHash("Grounded");

    public void SetFootstepPack(AudioPack pack) {
        footstepPack = pack;
    }

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void DoFootstep(AnimationEvent evt) {
        bool groundedAnimation = evt.animatorStateInfo.IsName("Base Layer.Movement");
        if (groundedAnimation && !animator.GetBool(Grounded)) {
            return;
        }

        if (!groundedAnimation && animator.GetBool(Grounded)) {
            return;
        }
        
        if (evt.animatorClipInfo.weight < 0.5f) {
            return;
        }
        
        Transform f = evt.intParameter == 0 ? animator.GetBoneTransform(HumanBodyBones.LeftFoot) : animator.GetBoneTransform(HumanBodyBones.RightFoot);
        AudioClip clip = footstepPack.GetClip();
        if (Physics.Raycast(f.position, Vector3.down, out var hit, 1f, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore)) {
            TerrainAudio a = hit.collider.GetComponent<TerrainAudio>();
            PhysicMaterial mat = hit.collider.sharedMaterial;
            if (a != null) {
                mat = a.GetMaterialAtPoint(hit.point);
            }
            var group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(mat);
            if (group != null) {
                clip = group.GetImpactClip(PhysicsAudioGroup.SurfaceImpactType.Footstep, 1f);
            }
            GameManager.instance.SpawnAudioClipInWorld(clip, f.position, 0.8f, GameManager.instance.soundEffectGroup);
        }
    }
}
