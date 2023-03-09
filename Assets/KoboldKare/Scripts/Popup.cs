using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Popup : MonoBehaviour {
    public Image icon;
    public TMP_Text description;
    public TMP_Text title;
    public Button cancel;
    public Button okay;
    public void Clear() {
        PopupHandler.instance.ClearPopup(this);
    }
}
