using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

[System.Serializable]
public class GameEventResponseVideoPlayerStop : GameEventResponse {
    public VideoPlayer target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.Stop();
    }
}
