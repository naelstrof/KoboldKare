using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonRegionHelper : MonoBehaviourPunCallbacks{
    public TMPro.TMP_Dropdown dropdown;
    private RegionHandler cachedHandler;
    private Region selectedRegion;

    public void ChooseRegion(int id){
        NetworkManager.instance.JoinLobby(dropdown.options[id].text);
    }

    public override void OnRegionListReceived(RegionHandler handler){
        Debug.Log("[Photon Region Handler] :: Currently connected region: "+PhotonNetwork.CloudRegion);
        dropdown.ClearOptions();
        cachedHandler = handler;
        var returnedRegions = new List<TMP_Dropdown.OptionData>();
        foreach (var item in cachedHandler.EnabledRegions){
            returnedRegions.Add(new TMPro.TMP_Dropdown.OptionData(item.Code));
        }
        dropdown.AddOptions(returnedRegions);
        dropdown.onValueChanged.RemoveListener(ChooseRegion);
        dropdown.onValueChanged.AddListener(ChooseRegion);
        if (string.IsNullOrEmpty(PhotonNetwork.CloudRegion)) {
            Debug.Log("Unknown region, forcing connection to us");
            NetworkManager.instance.JoinLobby("us");
        }
    }

    public override void OnConnectedToMaster(){
        base.OnConnectedToMaster();
        Debug.Log("[Photon Region Handler] :: Connected to master");
        foreach (var item in dropdown.options){
            if(PhotonNetwork.CloudRegion == item.text) {
                dropdown.SetValueWithoutNotify(dropdown.options.IndexOf(item));
            }
        }
    }
}
