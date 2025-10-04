using UnityEngine;

[System.Serializable]
public class GameEventResponsePlayAudioPack : GameEventResponse {
    [SerializeField] private AudioPack audioPack;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        
        AudioPack.PlayClipAtPoint(audioPack, owner.transform.position);
    }
}
