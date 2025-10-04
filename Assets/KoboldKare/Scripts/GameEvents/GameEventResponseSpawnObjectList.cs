using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameEventResponseSpawnObjectList : GameEventResponse {
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private bool singleUse;

    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (Transform t in spawnPoints) {
            GameObject.Instantiate(prefab, t.position, Quaternion.identity);
        }
        if(singleUse) {
            spawnPoints.Clear();
        }
    }
}
