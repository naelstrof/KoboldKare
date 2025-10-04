using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class GameEventResponseVideoPlayerPlay : GameEventResponse {
    public VideoPlayer target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.Play();
    }
}
