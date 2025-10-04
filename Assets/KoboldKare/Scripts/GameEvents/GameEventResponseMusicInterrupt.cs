using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseMusicInterrupt : GameEventResponse {
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        MusicManager.InterruptStatic();
    }
}
