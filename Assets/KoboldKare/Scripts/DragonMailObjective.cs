using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Localization;

[System.Serializable]
public class DragonMailObjective : ISavable, IPunObservable {
    public bool autoAdvance = false;
    [SerializeField]
    protected LocalizedString title;
    [SerializeField]
    protected PhotonGameObjectReference starExplosion;

    public virtual string GetTitle() => title.GetLocalizedString();
    public delegate void ObjectiveAction(DragonMailObjective obj);

    public event ObjectiveAction completed;
    public event ObjectiveAction updated;
    protected void TriggerComplete() {
        TriggerUpdate();
        completed.Invoke(this);
    }
    protected void TriggerUpdate() {
        updated.Invoke(this);
    }

    protected virtual void Advance(Vector3 position) {
        if (!PhotonNetwork.IsMasterClient) return;
        GameObject obj = PhotonNetwork.Instantiate(starExplosion.photonName, position, Quaternion.identity);
        obj.GetPhotonView().StartCoroutine(DestroyAfterTime(obj));
    }

    IEnumerator DestroyAfterTime(GameObject obj) {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.Destroy(obj);
    }

    public virtual void Register() {
    }

    public virtual void Unregister() {
    }

    public virtual string GetTextBody() {
        return "";
    }

    public virtual void Save(BinaryWriter writer) {
    }

    public virtual void Load(BinaryReader reader) {
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    }

    public virtual void OnValidate() {
        starExplosion.OnValidate();
    }

}