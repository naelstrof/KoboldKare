using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class RandomizedPurchasable : GenericPurchasable {
    [SerializeField]
    private List<ScriptablePurchasable> randomizePool;
    public override void OnRestock(object nothing) {
        SwapTo(randomizePool[UnityEngine.Random.Range(0, randomizePool.Count)]);
        base.OnRestock(nothing);
    }
}
