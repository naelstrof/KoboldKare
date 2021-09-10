using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKoboldLoader : MonoBehaviour {
    public static string[] settingNames = {"Sex", "Hue", "Brightness", "Saturation", "Contrast", "Dick", "TopBottom", "Thickness", "BoobSize", "KoboldSize"};
    public Kobold targetKobold;
    public UnityEvent onLoad;
    public static int defaultDickID = 0;
    private ExitGames.Client.Photon.Hashtable koboldSave = new ExitGames.Client.Photon.Hashtable();
    void Start() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
            option.onValueChange += OnValueChange;
            ProcessOption(koboldSave, option);
        }
        if (!targetKobold.isLoaded) {
            if (PhotonNetwork.InRoom) {
                SaveManager.RPC(targetKobold.photonView, "Load", RpcTarget.AllBuffered, new object[] { koboldSave });
            } else {
                targetKobold.Load(koboldSave);
            }
        }
    }

    //public IEnumerator ReloadKoboldAfterDelay() {
        //yield return new WaitForSeconds(0.5f);
        //if (PhotonNetwork.InRoom && targetKobold.photonView.IsMine) {
            //PhotonNetwork.CleanRpcBufferIfMine(targetKobold.photonView);
            //targetKobold.photonView.RPC("RPCLoad", RpcTarget.OthersBuffered, koboldSave.ToPhoton());
        //}
        //targetKobold.Load(koboldSave, 0);
        //onLoad.Invoke();
    //}

    void OnDestroy() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
        }
    }
    public static void ProcessOption(ExitGames.Client.Photon.Hashtable t, UnityScriptableSettings.ScriptableSetting setting) {
        if (setting == UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Dick")) {
            if (setting.value != 0f) {
                int[] equipmentList = new int[] { defaultDickID };
                t["equippedItems"] = equipmentList;
            } else {
                int[] equipmentList = new int[] {};
                t["equippedItems"] = equipmentList;
            }
        } else {
            t[setting.name] = setting.value;
        }
    }
    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        ProcessOption(koboldSave, setting);
        ExitGames.Client.Photon.Hashtable obj = new ExitGames.Client.Photon.Hashtable();
        ProcessOption(obj, setting);
        if (PhotonNetwork.InRoom) {
            SaveManager.RPC(targetKobold.photonView, "Load", RpcTarget.AllBuffered, new object[] { obj });
        } else {
            targetKobold.Load(obj);
        }
    }

    public static ExitGames.Client.Photon.Hashtable GetSaveObject() {
        ExitGames.Client.Photon.Hashtable obj = new ExitGames.Client.Photon.Hashtable();
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            ProcessOption(obj, option);
        }
        return obj;
    }
    public static ExitGames.Client.Photon.Hashtable GetRandomKobold() {
        ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
        t["Sex"] = Random.Range(0f,1f);
        t["Hue"] = Random.Range(0f,1f);
        t["Brightness"] = Random.Range(0f,1f);
        t["Saturation"] = Random.Range(0f,1f);
        t["Contrast"] = Random.Range(0f,1f);
        if (Random.Range(0f,1f) > 0.5f) {
            int[] equipmentList = new int[] { defaultDickID };
            t["EquippedItems"] = equipmentList;
            t["BoobSize"] = Random.Range(0f,0.1f);
        } else {
            int[] equipmentList = new int[] {};
            t["EquippedItems"] = equipmentList;
            t["BoobSize"] = Random.Range(0.2f,1f);
        }
        t["TopBottom"] = Random.Range(-1f,1f);
        t["Thickness"] = Random.Range(-1f, 1f);
        t["KoboldSize"] = Random.Range(0.7f,1.2f);
        return t;
    }
}
