using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.IO;

public class DayNightCycle : MonoBehaviourPun, IPunObservable, ISavable {
    [SerializeField]
    private GameEventGeneric midnightEvent;
    [SerializeField]
    private GameEventFloat metabolizeEvent;
    private static DayNightCycle instance = null;
    private WaitForSeconds waitForTwoSeconds;
    private int daysPast = 0;

    public static void StaticSleep() {
        instance.Sleep();
    }

    private void Awake() {
        waitForTwoSeconds = new WaitForSeconds(2f);
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
            return;
        }
    }
    private void Start() {
        StartCoroutine(MetabolizeOccassionally());
    }
    private IEnumerator MetabolizeOccassionally() {
        while (isActiveAndEnabled) {
            yield return waitForTwoSeconds;
            metabolizeEvent.Raise(2f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // Just skip events, I don't care!!
        if (stream.IsWriting) {
            stream.SendNext(daysPast);
        } else {
            daysPast = (int)stream.ReceiveNext();
        }
    }
    public void Save(BinaryWriter writer) {
        writer.Write(daysPast);
    }
    public void Load(BinaryReader reader) {
        daysPast = reader.ReadInt32();
    }
    private void Sleep() {
        midnightEvent.Raise(null);
        daysPast++;
    }
}
