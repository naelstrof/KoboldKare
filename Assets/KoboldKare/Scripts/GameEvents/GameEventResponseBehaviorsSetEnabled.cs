using UnityEngine;

[System.Serializable]
public class GameEventResponseBehaviorsSetEnabled : GameEventResponse {
    public MonoBehaviour[] targets;
    public bool enabled;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.enabled = enabled;
        }
    }
}
