using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchasableDatabase : MonoBehaviour {
    private static PurchasableDatabase instance;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }
    public static ScriptablePurchasable GetPurchasable(string name) {
        foreach(var purchasable in instance.purchasables) {
            if (purchasable.name == name) {
                return purchasable;
            }
        }
        return null;
    }
    public static ScriptablePurchasable GetPurchasable(short id) {
        return instance.purchasables[id];
    }
    public static short GetID(ScriptablePurchasable purchasable) {
        return (short)instance.purchasables.IndexOf(purchasable);
    }
    public List<ScriptablePurchasable> purchasables;
}
