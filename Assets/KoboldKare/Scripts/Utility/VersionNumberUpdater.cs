using UnityEngine;
using TMPro;

public class VersionNumberUpdater : MonoBehaviour{
    void Start(){
        GetComponent<TextMeshProUGUI>().text = Application.version.ToString();
    }
}
