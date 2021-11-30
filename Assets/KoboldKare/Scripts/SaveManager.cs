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
    public static bool isSaving = false;
    public static bool isLoading = false;
    public static bool Save(string filename) {
        // Write it out!
        FileStream dataStream = null;
        try {
            string filePath = Application.persistentDataPath + "/" + filename;
            dataStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(dataStream);
            foreach(PhotonView view in PhotonNetwork.PhotonViewCollection) {
                writer.Write(view.ViewID);
            }
            //BinaryFormatter converter = new BinaryFormatter();
            //converter.Serialize(dataStream, save);
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

        // LOAD

        // Before we just reuse the save data, there's lots of temporary data we can't/shouldn't pull over.
        Debug.Log("Done loading " + filePath);
        // Done! Let the player spawn on the next frame.
        isLoading = false;
    }
}
