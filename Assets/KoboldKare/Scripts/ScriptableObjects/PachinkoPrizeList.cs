using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu()]
public class PachinkoPrizeList : ScriptableObject{
    [SerializeField]
    private List<PrizeEntry> prizes = new List<PrizeEntry>();

    [System.Serializable]
    public class PrizeEntry    {
        public ScriptablePurchasable prize;
        public float chance;
    }

    public List<PrizeEntry> GetPrizes(){
        return prizes;
    }
}
