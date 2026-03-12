using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class MoneyPile : MonoBehaviour {
    private float internalWorth;
    private Kobold tryingToEquip;
    [SerializeField]
    private GameObject[] displays;
    [SerializeField]
    private AnimationCurve moneyMap;
    [SerializeField]
    private Sprite useSprite;

    private NetworkedEntity networkedEntity;

    void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(useSprite);
            networkedEntity.moneyPileWorth.OnChange += OnMoneyPileWorthChanged;
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private void OnUse(NetworkedKobold by) {
        by.GiveMoney(networkedEntity);
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }
    
    private void OnMoneyPileWorthChanged(float prev, float next, bool asServer) {
        int targetIndex = (int)Mathf.Log(next, 5f);
        targetIndex = Mathf.Clamp(targetIndex, 0, displays.Length-1);
        for (int i=0;i<displays.Length;i++) {
            displays[i].SetActive(i==targetIndex);
        }
    }
}
