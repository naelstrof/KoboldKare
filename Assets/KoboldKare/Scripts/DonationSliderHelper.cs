using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DonationSliderHelper : MonoBehaviour{
    public TextMeshProUGUI label;

    public void updateText(float val){
        label.text = val.ToString();
    }
}