using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class SaveManager {
    public const string saveDataLocation = "saves/";
    public const string saveExtension = ".sav";
    public const string imageExtension = ".jpg";
    public const string saveHeader = "KKSAVE";
    public const string version = "0";
    public const int textureSize = 256;
    public delegate void SaveCompleteAction();
    public class SaveData {
        public SaveData(string fileName, DateTime time) {
            this.fileName = fileName;
            imageLocation = fileName.Substring(0, fileName.Length-4)+imageExtension;
            this.time = time;
            image = new Texture2D(16,16);
            image.LoadImage(File.ReadAllBytes(imageLocation));
        }
        public readonly string imageLocation;
        public readonly Texture2D image;
        public readonly string fileName;
        public readonly DateTime time;
    }
    private static List<SaveData> saveDatas = new List<SaveData>();
    public static void Init() {
        string saveDataPath = Application.persistentDataPath + "/" + saveDataLocation;
        if (!Directory.Exists(saveDataPath)) {
            Directory.CreateDirectory(saveDataPath);
        }
        saveDatas.Clear();
        foreach(string fileName in Directory.EnumerateFiles(saveDataPath)) {
            if (fileName.EndsWith(saveExtension)) {
                saveDatas.Add(new SaveData(fileName, File.GetCreationTime(fileName)));
            }
        }
    }
    // Give a copy, we don't want anyone manipulating it manually.
    public static List<SaveData> GetSaveDatas() {
        return new List<SaveData>(saveDatas);
    }
    private static string PrefabifyGameObjectName(GameObject obj) {
        string name = obj.name;
        // Remove (Clone) at the end of our prefab name.
        if (name.EndsWith("(Clone)")) {
            name = name.Substring(0,name.Length-7);
        }
        return name;
    }
    public static void Save(string filename, SaveCompleteAction action = null) {
        string saveDataPath = Application.persistentDataPath + "/" + saveDataLocation;
        string savePath = saveDataPath + filename + saveExtension;
        if (!Directory.Exists(saveDataPath)) {
            Directory.CreateDirectory(saveDataPath);
        }
        using(FileStream file = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write)) {
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(saveHeader);
            writer.Write(version);
            writer.Write(PhotonNetwork.ViewCount);
            foreach(PhotonView view in PhotonNetwork.PhotonViewCollection) {
                writer.Write(view.ViewID);
                writer.Write(PrefabifyGameObjectName(view.gameObject));
                foreach(var observable in view.ObservedComponents) {
                    if (observable is ISavable) {
                        (observable as ISavable).Save(writer, version);
                    }
                }
            }
        }
        // Save a screenshot of what's going on.
        string imageSavePath = saveDataPath + filename + imageExtension;
        Screenshotter.GetScreenshot((texture)=>{
            using(FileStream file = new FileStream(imageSavePath, FileMode.CreateNew, FileAccess.Write)) {
                byte[] jpg = texture.EncodeToJPG();
                file.Write(jpg, 0, jpg.Length);
            }
            saveDatas.Add(new SaveData(savePath, File.GetCreationTime(savePath)));
            action?.Invoke();
        });
    }
    private static void LoadImmediate(string filename) {
        using(FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
            BinaryReader reader = new BinaryReader(file);
            if (reader.ReadString() != saveHeader) {
                throw new UnityException("Not a save file: " + filename);
            }
            string fileVersion = reader.ReadString();
            int viewCount = reader.ReadInt32();
            for(int i=0;i<viewCount;i++) {
                int viewID = reader.ReadInt32();
                string prefabName = reader.ReadString();
                PhotonView view = PhotonNetwork.GetPhotonView(viewID);
                
                if (view != null && PrefabifyGameObjectName(view.gameObject) != prefabName) {
                    PhotonNetwork.Destroy(view.gameObject);
                    view = null;
                }
                if (view == null) {
                    GameObject obj = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
                    view = obj.GetComponent<PhotonView>();
                    PhotonNetwork.LocalCleanPhotonView(view);
                    view.ViewID = viewID;
                    PhotonNetwork.RegisterPhotonView(view);
                }
                foreach(var observable in view.ObservedComponents) {
                    if (observable is ISavable) {
                        (observable as ISavable).Load(reader, fileVersion);
                    }
                }
            }
        }
    }
    public static void Load(string filename, bool online = false, int maxPlayers = 2, string roomName = "", bool visible = false) {
        GameManager.instance.StartCoroutine(LoadRoutine(filename, online, maxPlayers, roomName, visible));
    }
    public static IEnumerator LoadRoutine(string filename, bool online, int maxPlayers, string roomName, bool visible) {
        // This bool basically only prevents the player from spawning while we fast-forward.
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
        LoadImmediate(filename);
    }
}
