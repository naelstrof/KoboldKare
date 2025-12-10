using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleJSON;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Prefab Database", menuName = "Data/Prefab Database", order = 1)]
public class PrefabDatabase : ScriptableObject {
    public bool TryGetGroupName(out string groupName) {
        switch (name) {
            case "CosmeticDatabase":
                groupName = "Cosmetic";
                return true;
            case "EquipmentStoreDatabase":
                groupName = "EquipmentStoreItem";
                return true;
            case "FruitDatabase":
                groupName = "Fruit";
                return true;
            case "GeneralDatabase":
                groupName = "NetworkedPrefab";
                return true;
            case "PenisDatabase":
                groupName = "Penis";
                return true;
            case "PlayablePlayerDatabase":
                groupName = "PlayableCharacter";
                return true;
            case "SeedDatabase":
                groupName = "Seed";
                return true;
        }
        groupName = "NetworkedPrefab";
        return false;
    }
}
