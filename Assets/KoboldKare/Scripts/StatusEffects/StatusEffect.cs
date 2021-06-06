using Photon.Pun;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Data/StatusEffect", order = 1)]
public class StatusEffect : ScriptableObject, ITooltipDisplayable {
    public GameObject statDisplayPrefab;
    [HideInInspector]
    public int guid;
    private static Dictionary<int,StatusEffect> availableStatuses = new Dictionary<int, StatusEffect>();

    // Since these aren't referenced anywhere directly, this is the only way to get them to load properly.
    [RuntimeInitializeOnLoadMethod]
    public void OnInitialize() {
        if (!availableStatuses.ContainsKey(guid)) {
            availableStatuses.Add(guid, this);
        }
    }
    public void OnEnable() {
        OnInitialize();
    }

    public int GetID() {
        return guid;
    }
    public static StatusEffect GetFromID(int id) {
        if (availableStatuses.ContainsKey(id)) {
            return availableStatuses[id];
        }
        return null;
    }

    public void OnTooltipDisplay(RectTransform panel) {
        GameObject someText = new GameObject("StatusName", new Type[] { typeof(TMPro.TextMeshProUGUI) });
        someText.transform.SetParent(panel, false);
        var async = localizedName.GetLocalizedString();
        async.Completed += (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<string> result) => { someText.GetComponent<TextMeshProUGUI>().text = result.Result; };
        foreach(Statistic s in statistics) {
            GameObject display = GameObject.Instantiate(statDisplayPrefab, panel);
            string displayString = "";
            if (s.addAmount > 0) {
                displayString += (" +" + s.addAmount);
            } else if (s.addAmount < 0) {
                displayString += s.addAmount.ToString();
            }
            if (s.multiplier != 1) {
                displayString += " x" + s.multiplier;
            }
            display.GetComponentInChildren<TextMeshProUGUI>().text = displayString;
            display.transform.Find("Image").GetComponentInChildren<Image>().sprite = s.statType.sprite;
        }
    }

    public Sprite sprite;
    public LocalizedString localizedName;
    public LocalizedString localizedDescription;
    public List<Statistic> statistics;
    public bool stacks = false;
    public float duration = float.PositiveInfinity;

    [System.Serializable]
    public class Statistic {
        public Stat statType;
        public float multiplier = 1f;
        public float addAmount = 0f;
    }
    public void OnValidate() {
        OnInitialize();
        while ( GetFromID(guid) != this) {
            guid++;
            OnInitialize();
        }
    }
}
