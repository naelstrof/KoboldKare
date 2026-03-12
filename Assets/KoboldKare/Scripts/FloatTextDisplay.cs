using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Text;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class FloatTextDisplay : MonoBehaviour {
    private TMPro.TextMeshProUGUI text;
    [SerializeField]
    private string startingText;
    private Coroutine routine;
    private NetworkedKobold holder;

    void Start() {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        holder = GetComponentInParent<NetworkedKobold>();
        text.text = startingText + Mathf.Round(holder.money.Value);
        holder.money.OnChange += OnMoneyChanged;
    }
    void OnMoneyChanged(float prev, float next, bool asServer) {
        if (routine != null) {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(MoneyUpdateRoutine(prev, next));
    }
    IEnumerator MoneyUpdateRoutine(float from, float to) {
        float startTime = Time.time;
        float duration = 1f;
        while (Time.time<startTime+duration) {
            float t = (Time.time - startTime)/duration;
            var actual = Mathf.Lerp(from,to,t);
            text.text = startingText + Mathf.Round(actual);
            yield return null;
        }
    }
}
