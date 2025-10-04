using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class GameEventResponseVideoPlayerPause : GameEventResponse {
    public VideoPlayer target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.Pause();
    }
}
