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
    }

    public static PrefabDatabase GetDatabase(string name) {
        foreach(var database in instance.databases) {
            if (database.name == name) {
                return database;
            }
        }
        return null;
    }
}
