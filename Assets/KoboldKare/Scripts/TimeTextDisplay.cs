using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Text;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class TimeTextDisplay : MonoBehaviour {
    private TMPro.TextMeshProUGUI text;
    private StringBuilder sb = new StringBuilder(16,16);
    public string startingText;
    private float hour = 0;
    private float minute = 0;
    void Start() {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }
    void FixedUpdate() {
        // I know this whole thing looks fucking dumb, but it prevents C# from allocating 1kb per frame.
        if ( hour != Mathf.Floor(DayNightCycle.instance.hour) || minute != Mathf.Floor(DayNightCycle.instance.minute)) {
            hour = Mathf.Floor(DayNightCycle.instance.hour);
            minute = Mathf.Floor(DayNightCycle.instance.minute);
            // THIS THING ALLOCATES 1KB EVERYTIME YOU CALL IT
            // WHAT THE FUCK
            //string s = startingText + Mathf.Floor(time.GetHour()).ToString("00") + ":" + Mathf.Floor(time.GetMinute()).ToString("00");
            sb.Length = 0;
            sb.Append(startingText);
            if (hour<10) {
                sb.Append("0");
            }
            sb.Append(hour);
            sb.Append(":");
            if (minute<10) {
                sb.Append("0");
            }
            sb.Append(minute);
            string s = sb.ToString();
            if (text.text != s) {
                text.SetText(s);
            }
        }
    }
}
