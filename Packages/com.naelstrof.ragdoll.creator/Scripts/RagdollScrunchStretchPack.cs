using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


[CreateAssetMenu(fileName = "New Ragdoll Scrunch Stretch Pack", menuName = "Data/Ragdoll Scrunch Stretch Pack")]
public class RagdollScrunchStretchPack : ScriptableObject {
    [SerializeField] private AnimationClip neutralClip;
    [SerializeField] private List<AnimationClip> scrunchClips;
    [SerializeField] private List<AnimationClip> stretchClips;
    public ReadOnlyCollection<AnimationClip> GetScrunchClips() => scrunchClips.AsReadOnly();
    public ReadOnlyCollection<AnimationClip> GetStretchClips() => stretchClips.AsReadOnly();
    public AnimationClip GetNeutralClip() => neutralClip;
}
