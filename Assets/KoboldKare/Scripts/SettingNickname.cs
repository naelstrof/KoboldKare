using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "DecalQuality", menuName = "Unity Scriptable Setting/KoboldKare/Nickname", order = 1)]
public class SettingNickname : SettingString {
    
    // FIXME FISHNET
    public override void SetValue(string value) {
        //PhotonNetwork.NickName = value;
        base.SetValue(value);
    }
}
