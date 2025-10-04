using UnityEngine;

[System.Serializable]
public class GameEventResponseGameObjectSetActive : GameEventResponse {
    public GameObject[] targets;
    public bool active;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.SetActive(active);
        }
    }
}
