using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Text;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class FloatTextDisplay : MonoBehaviour {
    private TMPro.TextMeshProUGUI text;
    public ScriptableFloat num;
    public string startingText;
    private float disp;
    private float currentFloat = 0;
    private StringBuilder sb = new StringBuilder(16,16);
    void Start() {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        disp = num.value;
    }
    void FixedUpdate() {
        disp = Mathf.MoveTowards(disp, num.value, Time.deltaTime * 5f + (Mathf.Abs(num.value - disp)) * Time.deltaTime * 2f);
        if (currentFloat != disp) {
            currentFloat = disp;
            sb.Length = 0;
            sb.Append(startingText);
            sb.Append(Mathf.Round(disp));
            if (text.text != sb.ToString()) {
                text.SetText(sb.ToString());
            }
        }
    }
}
