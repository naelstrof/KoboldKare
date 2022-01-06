using UnityEngine;
using TMPro;

public class VersionNumberUpdater : MonoBehaviour{
    void Start(){
        var txt = string.Format("Version {0}",Application.version.ToString());
        if(Debug.isDebugBuild) txt += "_BETA";
        GetComponent<TextMeshProUGUI>().text = txt;
    }
}
