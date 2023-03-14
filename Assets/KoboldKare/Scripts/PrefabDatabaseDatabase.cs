using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabDatabaseDatabase : MonoBehaviour {
    private static PrefabDatabaseDatabase instance;
    
    [SerializeField]
    private List<PrefabDatabase> databases;

    private void Start() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
        LoadPlayerConfig();
    }

    public static void SavePlayerConfig() {
        foreach(var database in instance.databases) {
            database.SavePlayerConfiguration();
        }
    }

    public static void LoadPlayerConfig() {
        foreach(var database in instance.databases) {
            database.LoadPlayerConfiguration();
        }
    }

    public static void LoadPlayerConfig(string json) {
        foreach(var database in instance.databases) {
            database.LoadPlayerConfiguration(json);
        }
    }
}
