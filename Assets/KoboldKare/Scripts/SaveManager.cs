using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager {
    public static string saveDataName = "save.dat";
    [System.Serializable]
    public class SaveList {
        public List<string> fileNames = new List<string>();
    }

    private static SaveList data = new SaveList();
    private static bool WriteSaveList() {
        FileStream dataStream = null;
        try {
            string filePath = Application.persistentDataPath + "/" + saveDataName;
            dataStream = new FileStream(filePath, FileMode.Create);
            BinaryFormatter converter = new BinaryFormatter();
            converter.Serialize(dataStream, data);
            dataStream.Close();
            return true;
        } catch( Exception e ) {
            dataStream?.Close();
            Debug.LogException(e);
            return false;
        }
    }
    public static bool RemoveSave(string filename) {
        data.fileNames.Remove(filename);
        File.Delete(Application.persistentDataPath + "/" + filename);
        data.fileNames.Sort((a, b) => { return (int.Parse(b)).CompareTo(int.Parse(a)); });
        return WriteSaveList();
    }
    private static bool AddSave(string filename) {
        data.fileNames.Add(filename);
        data.fileNames.Sort((a, b) => { return (int.Parse(b)).CompareTo(int.Parse(a)); });
        return WriteSaveList();
    }

    public static SaveList GetSaveList(bool reloadFromDisk) {
        if (reloadFromDisk) {
            string filePath = Application.persistentDataPath + "/" + saveDataName;
            if (System.IO.File.Exists(filePath)) {
                FileStream dataStream = null;
                try {
                    dataStream = new FileStream(filePath, FileMode.Open);
                    BinaryFormatter converter = new BinaryFormatter();
                    data = converter.Deserialize(dataStream) as SaveList;
                    dataStream.Close();
                } catch (Exception e) {
                    Debug.LogException(e);
                    Debug.LogError("Failed to load save database, saves still exist! Though they will no longer be displayed... Regenerating the save database.");
                    data = new SaveList();
                    data.fileNames = new List<string>();
                    dataStream?.Close();
                    WriteSaveList();
                    return data;
                }
            }
            if (data == null || data.fileNames == null) {
                data = new SaveList();
                data.fileNames = new List<string>();
            }
        }
        data.fileNames.Sort((a, b) => { return (int.Parse(b)).CompareTo(int.Parse(a)); });
        return data;
    }

    public static void ClearData() {
        networkedEvents.Clear();
    }
    private static List<NetworkedEvent> networkedEvents = new List<NetworkedEvent>();
    public static bool isSaving = false;
    public static bool isLoading = false;
    [Serializable]
    class NetworkedEvent : ISerializable {
        public int photonID;
        public virtual void Invoke() {
            Debug.LogError("This should never get called...");
        }
        public NetworkedEvent() { }
        public NetworkedEvent(SerializationInfo info, StreamingContext context) {
            photonID = (int)info.GetValue("photonID", typeof(int));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
        }
    }
    [Serializable]
    class SceneObjectDestroyEvent : NetworkedEvent, ISerializable {
        public override void Invoke() {
            PhotonNetwork.Destroy(PhotonView.Find(photonID).gameObject);
        }
        public SceneObjectDestroyEvent() { }
        public SceneObjectDestroyEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
        }
    }
    [Serializable]
    class InstantiateEvent : NetworkedEvent, ISerializable {
        public string prefabName;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public byte group;
        public object[] data;
        public override void Invoke() {
            //PhotonView view = PhotonView.Find(photonID);
            //if (view != null) {
            //Debug.LogWarning("Deleting conflicting view id " + view, view.gameObject);
            //PhotonNetwork.Destroy(view);
            //}
            PhotonView newView = PhotonNetwork.Instantiate(prefabName, position, rotation, group, data).GetComponent<PhotonView>();
            PhotonNetwork.LocalCleanPhotonView(newView);
            newView.ViewID = photonID;
            PhotonNetwork.RegisterPhotonView(newView);
        }
        public InstantiateEvent() { }
        public InstantiateEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            prefabName = (string)info.GetValue("prefabName", typeof(string));
            position = (SerializableVector3)info.GetValue("position", typeof(SerializableVector3));
            rotation = (SerializableQuaternion)info.GetValue("rotation", typeof(SerializableQuaternion));
            group = (byte)info.GetValue("group", typeof(byte));
            List<byte[]> things = (List<byte[]>)info.GetValue("data", typeof(List<byte[]>));
            List<object> convertedThings = new List<object>();
            foreach(byte[] b in things) {
                convertedThings.Add(Protocol.Deserialize(b));
            }
            data = convertedThings.ToArray();
            if (data.Length == 0) {
                data = null;
            }
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
            info.AddValue("prefabName", prefabName, typeof(string));
            info.AddValue("position", position, typeof(SerializableVector3));
            info.AddValue("rotation", rotation, typeof(SerializableQuaternion));
            info.AddValue("group", group, typeof(byte));
            List<byte[]> things = new List<byte[]>();
            if (data != null) {
                foreach (object obj in data) {
                    things.Add(Protocol.Serialize(obj));
                }
            }
            info.AddValue("data", things, typeof(List<byte[]>));
        }
    }
    [Serializable]
    class RPCEvent : NetworkedEvent, ISerializable {
        public string functionName;
        public RpcTarget targetPlayer;
        public object[] parameters;
        public override void Invoke() {
            PhotonView view = PhotonView.Find(photonID);
            if (view != null) {
                view.RPC(functionName, targetPlayer, parameters);
            }
        }
        public RPCEvent() { }
        public RPCEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            functionName = (string)info.GetValue("functionName", typeof(string));
            targetPlayer = (RpcTarget)info.GetValue("targetPlayer", typeof(RpcTarget));
            List<byte[]> things = (List<byte[]>)info.GetValue("parameters", typeof(List<byte[]>));
            List<object> convertedThings = new List<object>();
            foreach(byte[] b in things) {
                convertedThings.Add(Protocol.Deserialize(b));
            }
            parameters = convertedThings.ToArray();
            if (parameters.Length == 0) {
                parameters = null;
            }
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
            info.AddValue("functionName", functionName, typeof(string));
            info.AddValue("targetPlayer", targetPlayer, typeof(RpcTarget));
            List<byte[]> things = new List<byte[]>();
            if (parameters != null) {
                foreach (object obj in parameters) {
                    things.Add(Protocol.Serialize(obj));
                }
            }
            info.AddValue("parameters", things, typeof(List<byte[]>));
        }
    }
    [Serializable]
    class TransformEvent : NetworkedEvent, ISerializable {
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public TransformEvent(PhotonView view) {
            photonID = view.ViewID;
            position = view.transform.position;
            rotation = view.transform.rotation;
        }
        public override void Invoke() {
            PhotonView view = PhotonView.Find(photonID);
            if (view != null) {
                view.transform.position = position;
                view.transform.rotation = rotation;
            }
        }
        public TransformEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            position = (SerializableVector3)info.GetValue("position", typeof(SerializableVector3));
            rotation = (SerializableQuaternion)info.GetValue("rotation", typeof(SerializableQuaternion));
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
            info.AddValue("position", position, typeof(SerializableVector3));
            info.AddValue("rotation", rotation, typeof(SerializableQuaternion));
        }
    }

    [Serializable]
    class PlayerPossessionEvent : NetworkedEvent, ISerializable {
        public PlayerPossessionEvent() {}
        public override void Invoke() {
            PhotonView view = PhotonView.Find(photonID);
            if (view != null) {
                view.GetComponentInChildren<PlayerPossession>(true).gameObject.SetActive(true);
                NetworkManager.instance.localPlayerInstance = view;
                NetworkManager.instance.RPCPlayerSpawn(view.ViewID);
                PopupHandler.instance.ClearAllPopups();
                GameManager.instance.UnpauseEvent.Raise();
            }
        }
        public PlayerPossessionEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
        }
    }
    
    [Serializable]
    class ObservableEvent : NetworkedEvent, ISerializable {
        public object[] data;
        public ObservableEvent(PhotonView view) {
            photonID = view.ViewID;
            PhotonStream stream = new PhotonStream(true, null);
            PhotonMessageInfo info = new PhotonMessageInfo(PhotonNetwork.NetworkingClient.LocalPlayer, PhotonNetwork.ServerTimestamp, view);
            foreach(var observable in view.ObservedComponents) {
                // Had to edit PhotonStream and make it start with a default List<object>()
                ((IPunObservable)observable).OnPhotonSerializeView(stream, info);
            }
            data = stream.ToArray();
        }
        public override void Invoke() {
            PhotonView view = PhotonView.Find(photonID);
            PhotonStream stream = new PhotonStream(false, data);
            PhotonMessageInfo info = new PhotonMessageInfo(PhotonNetwork.NetworkingClient.LocalPlayer, PhotonNetwork.ServerTimestamp, view);
            try {
                view?.SerializeView(stream, info);
            } catch (InvalidCastException e) {
                Debug.LogException(e);
                Debug.Log("Object " + view + " has mismatched observables from the save!", view.gameObject);
            }
        }
        public ObservableEvent(SerializationInfo info, StreamingContext context) : base(info, context) {
            List<byte[]> things = (List<byte[]>)info.GetValue("data", typeof(List<byte[]>));
            List<object> convertedThings = new List<object>();
            foreach(byte[] b in things) {
                convertedThings.Add(Protocol.Deserialize(b));
            }
            data = convertedThings.ToArray();
            if (data.Length == 0) {
                data = null;
            }
        }
        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("photonID", photonID, typeof(int));
            List<byte[]> things = new List<byte[]>();
            if (data != null) {
                foreach (object obj in data) {
                    things.Add(Protocol.Serialize(obj));
                }
            }
            info.AddValue("data", things, typeof(List<byte[]>));
        }
    }
    public static void Destroy(GameObject obj) {
        PhotonView view = obj.GetComponent<PhotonView>();
        if (view) {
            bool found = false;
            for (int i = 0; i < networkedEvents.Count; i++) {
                if (networkedEvents[i].photonID == view.ViewID) {
                    if (networkedEvents[i] is InstantiateEvent) {
                        found = true;
                    }
                    networkedEvents.RemoveAt(i);
                }
            }
            // If we didn't instantiate this object, that means that it's a scene object that needs to be removed during our save...
            if (!found && view.IsRoomView) {
                SceneObjectDestroyEvent sode = new SceneObjectDestroyEvent();
                sode.photonID = view.ViewID;
                networkedEvents.Add(sode);
            }
        }
        PhotonNetwork.Destroy(obj);
    }

    public static void AddRPC(PhotonView view, string name, RpcTarget targetPlayer, object[] parameters = null) {
        RPCEvent rev = new RPCEvent();
        rev.functionName = name;
        rev.parameters = parameters;
        rev.targetPlayer = RpcTarget.AllBuffered;
        rev.photonID = view.ViewID;
        networkedEvents.Add(rev);
    }

    public static void AddInstantiate(int photonID, string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null ) {
        InstantiateEvent iev = new InstantiateEvent();
        iev.prefabName = prefabName;
        iev.position = position;
        iev.rotation = rotation;
        iev.group = group;
        iev.data = data;
        iev.photonID = photonID;
        networkedEvents.Add(iev);
    }

    public static void ClearAllEventsWithID(int instantiationId) {
        bool found = false;
        for (int i = 0; i < networkedEvents.Count; i++) {
            if (networkedEvents[i].photonID == instantiationId) {
                if (networkedEvents[i] is InstantiateEvent) {
                    found = true;
                }
                networkedEvents.RemoveAt(i);
            }
        }
        // If we didn't instantiate this object, that means that it's a scene object that needs to be removed during our save...
        if (!found) {
            SceneObjectDestroyEvent sode = new SceneObjectDestroyEvent();
            sode.photonID = instantiationId;
            networkedEvents.Add(sode);
        }
    }

    public static void RPC(PhotonView view, string name, RpcTarget targetPlayer, object[] parameters = null) {
        if (targetPlayer == RpcTarget.AllBuffered || targetPlayer == RpcTarget.OthersBuffered || targetPlayer == RpcTarget.AllBufferedViaServer) {
            RPCEvent rev = new RPCEvent();
            rev.functionName = name;
            rev.parameters = parameters;
            // We probably need to repeat these commands on the client, since usually OthersBuffered means that the client already handled it, but catching up to the game state would need to run it again.
            rev.targetPlayer = RpcTarget.AllBuffered;
            rev.photonID = view.ViewID;
            networkedEvents.Add(rev);
        }
        view.RPC(name, targetPlayer, parameters);
    }
    public static bool Save(string filename) {
        isSaving = true;
        // Save all photon transforms, since they don't get saved otherwise.
        List<NetworkedEvent> save = new List<NetworkedEvent>(networkedEvents);
        // Save their positions.
        foreach(var pair in PhotonNetwork.PhotonViewCollection) {
            if (pair != null) {
                save.Add(new TransformEvent(pair));
                save.Add(new ObservableEvent(pair));
            }
        }
        if (NetworkManager.instance.localPlayerInstance != null) {
            PlayerPossessionEvent p = new PlayerPossessionEvent();
            p.photonID = NetworkManager.instance.localPlayerInstance.ViewID;
            save.Add(p);
        }

        // Write it out!
        FileStream dataStream = null;
        try {
            string filePath = Application.persistentDataPath + "/" + filename;
            dataStream = new FileStream(filePath, FileMode.Create);
            BinaryFormatter converter = new BinaryFormatter();
            converter.Serialize(dataStream, save);
            dataStream.Close();
            Debug.Log("Saved to " + filePath);
        } catch( Exception e) {
            Debug.LogException(e);
            dataStream?.Close();
            isSaving = false;
            return false;
        }
        AddSave(filename);
        isSaving = false;
        return true;
    }
    public static void Load(string filename, bool online = false, int maxPlayers=2, string roomName="", bool visible=false) {
        GameManager.instance.StartCoroutine(LoadRoutine(filename, online, maxPlayers, roomName, visible));
    }
    public static IEnumerator LoadRoutine(string filename, bool online, int maxPlayers, string roomName, bool visible) {
        string filePath = Application.persistentDataPath + "/" + filename;
        FileStream dataStream = null;
        List<NetworkedEvent> saveData = null;
        try {
            dataStream = new FileStream(filePath, FileMode.Open);
            BinaryFormatter converter = new BinaryFormatter();
            saveData = converter.Deserialize(dataStream) as List<NetworkedEvent>;
            dataStream.Close();
        } catch (Exception e) {
            Debug.Log(e);
            dataStream?.Close();
        }
        if (saveData == null) {
            Debug.LogError("Failed to load save " + filePath + ". It doesn't appear to be a save file!");
        } else {
            // This bool basically only prevents the player from spawning while we fast-forward.
            isLoading = true;
            if (online) {
                yield return GameManager.instance.StartCoroutine(NetworkManager.instance.EnsureOnlineAndReadyToLoad());
                if (string.IsNullOrEmpty(roomName)) {
                    roomName = Guid.NewGuid().ToString();
                }
                PhotonNetwork.CreateRoom(roomName, new RoomOptions{MaxPlayers = (byte)maxPlayers, IsVisible=visible});
            } else {
                yield return GameManager.instance.StartCoroutine(NetworkManager.instance.SinglePlayerRoutine());
            }
            yield return new WaitUntil(() => SceneManager.GetSceneByName("MainMap").isLoaded);
            yield return new WaitForSecondsRealtime(1f);
            foreach (var nev in saveData) {
                try {
                    nev.Invoke();
                } catch (Exception e) {
                    Debug.LogException(e);
                    Debug.Log("The save failed to load properly, one invalid command was pruned.");
                    saveData.Remove(nev);
                }
            }
            // Before we just reuse the save data, there's lots of temporary data we can't/shouldn't pull over.
            for (int i = 0; i < saveData.Count; i++) {
                if (saveData[i] is TransformEvent || saveData[i] is ObservableEvent || saveData[i] is PlayerPossessionEvent) {
                    saveData.RemoveAt(i);
                }
            }
            networkedEvents = saveData;
            Debug.Log("Done loading " + filePath);
            // Done! Let the player spawn on the next frame.
            isLoading = false;
        }
    }
    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null ) {
        if (isLoading) {
            return null;
        }
        GameObject spawned = PhotonNetwork.Instantiate(prefabName, position, rotation, group, data);
        InstantiateEvent iev = new InstantiateEvent();
        iev.prefabName = prefabName;
        iev.position = position;
        iev.rotation = rotation;
        iev.group = group;
        iev.data = data;
        iev.photonID = spawned.GetComponent<PhotonView>().ViewID;
        networkedEvents.Add(iev);
        return spawned;
    }
}
