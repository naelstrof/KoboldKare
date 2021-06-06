using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKoboldLoader : MonoBehaviour, IGameEventOptionListener {
    public Kobold targetKobold;
    public UnityEvent onLoad;
    public static int defaultDickID = 0;
    private ExitGames.Client.Photon.Hashtable koboldSave = new ExitGames.Client.Photon.Hashtable();
    void Start() {
        GameManager.instance.options.RegisterListener(this);
        foreach(var option in GameManager.instance.options.options) {
            ProcessOption(koboldSave, option.type, option.value);
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
        GameManager.instance.options.UnregisterListener(this);
    }
    public static void ProcessOption(ExitGames.Client.Photon.Hashtable t, GraphicsOptions.OptionType e, float value) {
        switch (e) {
            case GraphicsOptions.OptionType.Sex: t["sex"] = value; break;
            case GraphicsOptions.OptionType.Hue: t["hue"] = value; break;
            case GraphicsOptions.OptionType.Brightness: t["brightness"] = value; break;
            case GraphicsOptions.OptionType.Saturation: t["saturation"] = value; break;
            case GraphicsOptions.OptionType.Contrast: t["contrast"] = value; break;
            case GraphicsOptions.OptionType.Dick: {
                if (value != 0f) {
                    int[] equipmentList = new int[] { defaultDickID };
                    t["equippedItems"] = equipmentList;
                } else {
                    int[] equipmentList = new int[] {};
                    t["equippedItems"] = equipmentList;
                }
                break;
            }
            case GraphicsOptions.OptionType.TopBottom: t["topBottom"] = value; break;
            //case GraphicsOptions.OptionType.InOut: t["inout"] = value; break;
            case GraphicsOptions.OptionType.Thickness: t["thickness"] = value; break;
            //case GraphicsOptions.OptionType.Chubbiness: {
                //s.reagents["subcutaneous0"][ReagentData.ID.Fat].volume = value * 10f;
                //s.reagents["subcutaneous0"].TriggerChange();
                //break;
            //}
            //case GraphicsOptions.OptionType.BallsSize: {
                //s.reagents["balls0"][ReagentData.ID.Fat].volume = value * 10f;
                //s.reagents["balls0"][ReagentData.ID.Cum].volume = value * 10f * 0.33f;
                //s.reagents["balls0"].TriggerChange();
                //break;
            //}
            case GraphicsOptions.OptionType.KoboldSize: {
                t["size"] = value;
                //s.reagents["subcutaneous0"][ReagentData.ID.GrowthSerum].volume = value * 10f;
                //s.reagents["subcutaneous0"].TriggerChange();
                break;
            }
            //case GraphicsOptions.OptionType.DickSize: {
                //s.reagents["dick0"][ReagentData.ID.Fat].volume = value * 7f;
                //s.reagents["dick0"].TriggerChange();
                //break;
            //}
            case GraphicsOptions.OptionType.BoobSize: {
                t["boobSize"] = value;
                //s.reagents["boob0"][ReagentData.ID.Milk].volume = value*30f*0.33f;
                //s.reagents["boob0"][ReagentData.ID.Fat].volume = value*30f;
                //s.reagents["boob0"].TriggerChange();
                //s.reagents["boob1"][ReagentData.ID.Milk].volume = value*30f*0.33f;
                //s.reagents["boob1"][ReagentData.ID.Fat].volume = value*30f;
                //s.reagents["boob1"].TriggerChange();
                break;
            }
        }
    }
    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        ProcessOption(koboldSave, e, value);
        ExitGames.Client.Photon.Hashtable obj = new ExitGames.Client.Photon.Hashtable();
        ProcessOption(obj, e, value);
        if (PhotonNetwork.InRoom) {
            SaveManager.RPC(targetKobold.photonView, "Load", RpcTarget.AllBuffered, new object[] { obj });
        } else {
            targetKobold.Load(obj);
        }
    }

    public static ExitGames.Client.Photon.Hashtable GetSaveObject() {
        ExitGames.Client.Photon.Hashtable obj = new ExitGames.Client.Photon.Hashtable();
        foreach(var option in GameManager.instance.options.options) {
            ProcessOption(obj, option.type, option.value);
        }
        return obj;
    }
    public static ExitGames.Client.Photon.Hashtable GetRandomKobold() {
        ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
        t["sex"] = Random.Range(0f,1f);
        t["hue"] = Random.Range(0f,1f);
        t["brightness"] = Random.Range(0f,1f);
        t["saturation"] = Random.Range(0f,1f);
        t["contrast"] = Random.Range(0f,1f);
        if (Random.Range(0f,1f) > 0.5f) {
            int[] equipmentList = new int[] { defaultDickID };
            t["equippedItems"] = equipmentList;
            t["boobSize"] = Random.Range(0f,0.1f);
        } else {
            int[] equipmentList = new int[] {};
            t["equippedItems"] = equipmentList;
            t["boobSize"] = Random.Range(0.2f,1f);
        }
        t["topBottom"] = Random.Range(-1f,1f);
        t["thickness"] = Random.Range(-1f, 1f);
        t["size"] = Random.Range(0.7f,1.2f);
        return t;
    }
}
