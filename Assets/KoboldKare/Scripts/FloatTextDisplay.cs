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
    private MoneyHolder holder;
    private float oldMoney;
    private Coroutine routine;

    private void OnEnable() {
        holder = GetComponentInParent<MoneyHolder>();
    }

    void Start() {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        text.text = startingText + Mathf.Round(holder.GetMoney());
        oldMoney = holder.GetMoney();
        holder.moneyChanged += OnMoneyChanged;
    }
    void OnMoneyChanged(float newMoney) {
        if (routine != null) {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(MoneyUpdateRoutine(oldMoney, newMoney));
    }
    IEnumerator MoneyUpdateRoutine(float from, float to) {
        float startTime = Time.time;
        float duration = 1f;
        while (Time.time<startTime+duration) {
            float t = (Time.time - startTime)/duration;
            oldMoney = Mathf.Lerp(from,to,t);
            text.text = startingText + Mathf.Round(oldMoney).ToString();
            yield return null;
        }
    }
}
