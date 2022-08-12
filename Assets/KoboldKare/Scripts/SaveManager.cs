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
            if (File.Exists(imageLocation)) {
                image.LoadImage(File.ReadAllBytes(imageLocation));
            }
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

        if(name.Contains("(")){
            //Debug.Log(String.Format("[SaveManager] :: Convering {0} to {1}.",name,name.Split('(')[0].Trim()));
            return name.Split('(')[0].Trim();
        }
        
        return name;
    }
    public static void Save(string filename, SaveCompleteAction action = null) {
        //Debug.Log("[SaveManager] :: <Init Stage> File attempting to be saved: "+filename);
        string saveDataPath = Application.persistentDataPath + "/" + saveDataLocation;
        string savePath = saveDataPath + filename + saveExtension;
        if (!Directory.Exists(saveDataPath)) {
            Directory.CreateDirectory(saveDataPath);
        }
        using(FileStream file = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write)) {
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(saveHeader);
            writer.Write(version);
            //Debug.Log("viewCount: "+PhotonNetwork.ViewCount);
            writer.Write(PhotonNetwork.ViewCount);
            // We need to enable all our saved objects, they don't have proper viewids otherwise
            foreach(PhotonView view in GameObject.FindObjectsOfType<PhotonView>(true)) {
                if (!view.gameObject.activeInHierarchy && !(PhotonNetwork.PrefabPool as DefaultPool).ResourceCache.ContainsKey(PrefabifyGameObjectName(view.gameObject))){
                    Debug.LogError("Found a disabled static viewID" + view.ViewID + " " + view.gameObject.name + ", this is not allowed as it prevents unique id assignments!", view.gameObject);
                    return;
                }
            }
            foreach(PhotonView view in PhotonNetwork.PhotonViewCollection) {
                writer.Write(view.ViewID);
                //Debug.Log("[SaveManager] <Serialization Log> :: "+PrefabifyGameObjectName(view.gameObject));
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

    public static bool RemoveSave(string fileName){
        string saveDataPath = Application.persistentDataPath + "/" + saveDataLocation;
        string imageSavePath = fileName.Substring(0,fileName.Length-4) + imageExtension;
        string savePath = fileName;
        if(!File.Exists(savePath)){
            Debug.LogWarning("Indicated save file doesn't exist! ("+savePath+") Should remove from UI rather than disk. TODO: Callback.");
            return false;
        }
        else{
            File.Delete(savePath);
            File.Delete(imageSavePath); //Make sure to remove associated .jpg file of the same name too.
            //Debug.Log("Deleted file from disk: "+savePath);
            return true;
        }
    }
    private static void CleanUpImmediate() {
        foreach(PhotonView view in GameObject.FindObjectsOfType<PhotonView>(true)) {
            if((PhotonNetwork.PrefabPool as DefaultPool).ResourceCache.ContainsKey(PrefabifyGameObjectName(view.gameObject))){
                PhotonNetwork.Destroy(view.gameObject);
            }
        }
    }
    private static void LoadImmediate(string filename) {
        //Debug.Log("[SaveManager] :: <Init Stage> File attempting to be loaded: "+filename);
        // Don't load saves while online.
        if (NetworkManager.instance.online) {
            return;
        }
        CleanUpImmediate();
        using(FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
            BinaryReader reader = new BinaryReader(file);
            if (reader.ReadString() != saveHeader) {
                throw new UnityException("Not a save file: " + filename);
            }
            string fileVersion = reader.ReadString();
            int viewCount = reader.ReadInt32();
            //Debug.Log("viewCount: "+viewCount);
            for(int i=0;i<viewCount;i++) {
                int viewID = reader.ReadInt32();
                string prefabName = reader.ReadString();
                PhotonView view = PhotonNetwork.GetPhotonView(viewID);
                
                // Debug.Log("[SaveManager] <Deserialization Log> :: Attempting to load: "+prefabName);
                if((PhotonNetwork.PrefabPool as DefaultPool).ResourceCache.ContainsKey(prefabName)){
                    // Debug.Log("[SaveManager] <Deserialization Log> :: Found in Prefab Pool: "+prefabName);
                    GameObject obj = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
                    view = obj.GetComponent<PhotonView>();
                }
                if (view == null) {
                    Debug.Log("[SaveManager] <Deserialization Log> :: Running deep check when view returned null...");
                    foreach(PhotonView deepcheck in GameObject.FindObjectsOfType<PhotonView>(true)) {
                        if (deepcheck.ViewID == viewID) {
                            view = deepcheck;
                            Debug.Log("[SaveManager] <Deserialization Log> :: Deep check successful!");
                            break;
                        }
                    }
                }
                if (view == null) {
                    Debug.LogError( "Failed to find view id " + viewID + " and name " + prefabName);
                }
                else{
                    // Debug.Log("[Save Manager] <Deserialization Log> :: View checks were not null; load should proceed smoothly on this object.");
                }
                try {
                    //if(view.ObservedComponents.Count == 0){
                        // This is not necessarily an issue, photonviews can simply be used as a unique id for other scripts to use for rpcs or whatever.
                        //Debug.LogWarning("[SaveManager] <Deserialization Log> :: Attempting to deserialize photonview which is either not observing components or whose references to said components are broken/missing", view.gameObject);
                    //}
                    foreach(Component observable in view.ObservedComponents) {
                        if (observable is ISavable) {
                            // Debug.Log("[SaveManager] <Deserialization Log> :: Proceeding to call Load() on observed component of type "+observable+" on game object "+view.gameObject.name);
                            (observable as ISavable).Load(reader, fileVersion);
                        }
                    }
                } catch (Exception e) {
                    Debug.LogError("Failed to load observable on photonview " +viewID + ", " + prefabName, view);
                    throw e;
                }
            }
        }
    }
    private static IEnumerator MakeSureMapIsLoadedThenLoadSave(string filename) {
        //Ensure we show the player that the game is loading while we load
        if(SceneManager.GetActiveScene().name != "MainMenu"){
            GameManager.instance.Pause(false);
            GameManager.instance.loadListener.Show();
        }

        if (SceneManager.GetActiveScene().name != "MainMap") {
            yield return NetworkManager.instance.SinglePlayerRoutine();
        }
        yield return new WaitForSecondsRealtime(1f);
        LoadImmediate(filename);

        //Once loading is finished, hide loading screen
        if(SceneManager.GetActiveScene().name != "MainMenu"){
            GameManager.instance.loadListener.Hide();
        }
    }
    public static void Load(string filename) {
        //Debug.Log("[SaveManager] :: Loading in process...");
        GameManager.instance.StartCoroutine(MakeSureMapIsLoadedThenLoadSave(filename));
    }
}
