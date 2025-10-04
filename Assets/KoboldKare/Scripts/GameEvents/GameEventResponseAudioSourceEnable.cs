using UnityEngine;

[System.Serializable]
public class GameEventResponseAudioSourceEnable : GameEventResponse {
    public AudioSource target;
    public bool enabled;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.enabled = enabled;
    }
}
