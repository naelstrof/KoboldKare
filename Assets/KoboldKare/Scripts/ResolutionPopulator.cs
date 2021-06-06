using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionPopulator : MonoBehaviour {
    void Start() {
        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();
        foreach(Resolution r in Screen.resolutions) {
            optionData.Add(new TMP_Dropdown.OptionData(r.width + "x"+r.height + " ["+r.refreshRate+"]"));
        }
        GetComponent<TMP_Dropdown>().AddOptions(optionData);
    }
}
