using UnityEngine;

[System.Serializable]
public class GameEventResponsePlayAudioSource : GameEventResponse {
    public AudioSource target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.Play();
    }
}
