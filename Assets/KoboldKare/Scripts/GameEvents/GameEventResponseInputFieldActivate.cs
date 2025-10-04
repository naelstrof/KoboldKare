using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class GameEventResponseInputFieldActivate : GameEventResponse {
    public TMPro.TMP_InputField target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.ActivateInputField();
    }
}
