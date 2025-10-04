using UnityEngine;

[System.Serializable]
public class GameEventResponseSpawnObject : GameEventResponse {
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        GameObject.Instantiate(prefab, spawnPoint.position, Quaternion.identity);

    }
}
