using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class GameEventResponseInputFieldDeactivate : GameEventResponse {
    public TMPro.TMP_InputField target;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        target.DeactivateInputField();
    }
}
