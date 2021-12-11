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
    [System.Serializable]
    public class DayNightCycleEvent {
        [Range(0.01f,1f)]
        public float time01 = 0f;
        public GameEventGeneric ev = null;
    }
    public GameEventFloat metabolizeEvent;
    public static DayNightCycle instance = null;
    public float dayLength = 480f;
    private bool nextFrameUpdate, alarmClockUpgrade;
    private WaitForSeconds waitForTwoSeconds = new WaitForSeconds(1.6f);
    public void ForceUpdate() {
        nextFrameUpdate = true;
    }
    private float lastTime = 0f;
    public float hour {
        get {
            return time01 * 24f;
        }
    }
    public float minute {
        get {
            return (hour * 60f) % 60f;
        }
    }
    public float time {
        get {
            return (Time.timeSinceLevelLoad / dayLength)+rootTime;
        }
        set {
            if (value > lastTime) {
                RunEvents(lastTime, value);
            }
            rootTime = value - (Time.timeSinceLevelLoad / dayLength);
            lastTime = time;
        }
    }
    public float time01 {
        get {
            return time%1f;
        }
        set {
            WarpToTime(value);
        }
    }
    // Value -1 to 1 based on how much daylight there is. -1 is midnight, 1 is midday.
    public float daylight {
        get {
            return Mathf.Sin((time * 2f * Mathf.PI) + (3f * Mathf.PI / 2f));
        }
    }
    public float day01 {
        get {
            return Mathf.Clamp01(time01.Remap(0.25f, 0.75f, 0f, 1f));
        }
    }
    public float night01 {
        get {
            if (time01 <= 0.25f) {
                return Mathf.Clamp01(time01.Remap(0f, 0.25f, 0.5f, 1f));
            }
            return Mathf.Clamp01(time01.Remap(.75f, 1f, 0f, 0.5f));
        }
    }
    private float rootTime = 0f;
    public List<DayNightCycleEvent> events = new List<DayNightCycleEvent>();
    private void Awake() {
        if (instance == null) {
            instance = this;
            WarpToTime(0.35f, true);
        } else if (instance != this) {
            Destroy(gameObject);
            return;
        }
        SceneManager.activeSceneChanged += OnSceneChange;
    }
    public void OnSceneChange(Scene oldScene, Scene newScene) {
        if (PhotonNetwork.IsMasterClient) {
            WarpToTime(0.35f, false);
        }
    }
    private void RunEvents(float from, float to) {
        float firstMidnight = Mathf.Floor(from);
        float t = to;
        for (float f = firstMidnight; f < t; f += 1f) {
            foreach (DayNightCycleEvent e in events) {
                float c = f + e.time01;
                if (c <= t && c > from ) {
                    e.ev.Raise(null);
                }
            }
        }
    }
    public void Update() {
        if (photonView.IsMine) {
            RunEvents(lastTime, time);
            lastTime = time;
        }
    }
    public void WarpToTime(float time01) {
        WarpToTime(time01, false);
    }
    public void WarpToTime(float time01, bool skipEvents = false) {
        float oldTime = time;
        float nearestMidnight = Mathf.Floor(time);
        if (time01 <= this.time01) {
            nearestMidnight += 1f;
        }
        rootTime = nearestMidnight + time01 - (Time.timeSinceLevelLoad / dayLength);
        if (skipEvents) {
            lastTime = time;
        } else {
            metabolizeEvent.Raise((time - oldTime) * dayLength);
        }
    }
    public void Start() {
        StartCoroutine(MetabolizeOccassionally());
    }
    public IEnumerator MetabolizeOccassionally() {
        while (isActiveAndEnabled) {
            yield return waitForTwoSeconds;
            metabolizeEvent.Raise(2f);
        }
    }

    public void BoughtAlarmClock(){
        if(alarmClockUpgrade == false){
            dayLength = dayLength * 1.5f;
            alarmClockUpgrade = true;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // Just skip events, I don't care!!
        if (stream.IsWriting) {
            stream.SendNext(time);
            stream.SendNext(dayLength);
        } else {
            float desiredTime = (float)stream.ReceiveNext();
            time = desiredTime;
            dayLength = (float)stream.ReceiveNext();
        }
    }
    public void Save(BinaryWriter writer, string version) {
        writer.Write(time);
        writer.Write(dayLength);
    }
    public void Load(BinaryReader reader, string version) {
        time = reader.ReadSingle();
        dayLength = reader.ReadSingle();
    }
    public void Sleep() {
        WarpToTime(alarmClockUpgrade ? 0.25f : 0.35f);
    }
}
