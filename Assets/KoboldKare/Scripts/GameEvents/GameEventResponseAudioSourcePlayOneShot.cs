using UnityEngine;

[System.Serializable]
public class GameEventResponseAudioSourcePlayOneShot : GameEventResponse {
    public AudioSource target;
    public AudioClip clip;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.PlayOneShot(clip);
    }
}
