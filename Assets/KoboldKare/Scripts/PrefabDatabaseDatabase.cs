using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabDatabaseDatabase : MonoBehaviour {
    private static PrefabDatabaseDatabase instance;
    
    [SerializeField]
    private List<PrefabDatabase> databases;

    private void Awake() {
        if (instance != null) {
            Destroy(this);
        }

        instance = this;
        databases = new List<PrefabDatabase>();
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
