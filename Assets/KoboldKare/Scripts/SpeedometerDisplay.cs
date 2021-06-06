using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class SpeedometerDisplay : MonoBehaviour {
    public TMP_Text textTarget;
    public TMP_Text otherTextTarget;
    public Rigidbody body;
    public Gradient colorGradient;
    string[] cachedText;
    int maxSize = 32;
    void Start() {
        cachedText = new string[maxSize];
        for (int i=0;i<maxSize;i++) {
            string cachedString = "";
            for (int o=0;o<i;o++) {
                cachedString += "|";
            }
            cachedString += "]";
            cachedText[i] = cachedString;
        }
    }

    // Update is called once per frame
    void Update() {
        float v = body.velocity.With(y: 0).magnitude;
        textTarget.color = colorGradient.Evaluate(v / maxSize);
        textTarget.text = cachedText[Mathf.RoundToInt(Mathf.Clamp(v, 0, maxSize-1))];
        otherTextTarget.text = v.ToString("0.00");
        otherTextTarget.color = textTarget.color;
    }
}
