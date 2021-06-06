using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUIDisplay : MonoBehaviour {
    public Transform displayTarget;
    public GameObject statusEffectPrefab;
    public Kobold targetKobold;
    private List<GameObject> spawnedStatusInfos = new List<GameObject>();
    public void Start() {
        targetKobold.statblock.StatusEffectsChangedEvent += OnStatusEffectsChanged;
        OnStatusEffectsChanged(targetKobold.statblock, StatBlock.StatChangeSource.Network);
    }
    public void OnDestroy() {
        targetKobold.statblock.StatusEffectsChangedEvent -= OnStatusEffectsChanged;
    }
    public void OnStatusEffectsChanged(StatBlock block, StatBlock.StatChangeSource source) {
        foreach(GameObject obj in spawnedStatusInfos) {
            Destroy(obj);
        }
        foreach(var stat in block.activeEffects) {
            GameObject pre = GameObject.Instantiate(statusEffectPrefab, displayTarget);
            pre.GetComponentInChildren<TooltipDisplay>().thingToDisplay = stat.effect;
            pre.transform.Find("Image").GetComponent<Image>().sprite = stat.effect.sprite;
            spawnedStatusInfos.Add(pre);
        }
    }
}
