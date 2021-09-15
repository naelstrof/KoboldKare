using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PenetrationTech;

public class EggSpawner : MonoBehaviour {
    public Penetrable targetPenetrable;
    [Range(0f,1f)]
    public float spawnAlongLength = 0.5f;
    [Range(-1,1f)]
    public float pushDirection = -1f;
    public PhotonGameObjectReference penetratorPrefab;
    private List<Penetrator> penetrators = new List<Penetrator>();
    public void Update() {
        for(int i=0;i<penetrators.Count;i++) {
            Penetrator d = penetrators[i];
            d.PushTowards(pushDirection*0.02f);
            if (!d.IsInside(0.25f)) {
                d.Decouple(true);
                penetrators.Remove(d);
                StartCoroutine(ReenableEggAfterSomeTime(d));
            }
        }
    }
    public void SpawnEggNoReturn() {
        SpawnEgg();
    }
    public Penetrator SpawnEgg() {
        //Penetrator d = GameObject.Instantiate(penetratorPrefab).GetComponentInChildren<Penetrator>();
        Penetrator d = SaveManager.Instantiate(penetratorPrefab.photonName,targetPenetrable.GetPoint(spawnAlongLength, false), Quaternion.identity).GetComponentInChildren<Penetrator>();
        if (d == null) {
            return null;
        }
        d.body.transform.position = targetPenetrable.GetPoint(spawnAlongLength, false);
        // Manually control penetration parameters
        d.autoPenetrate = false;
        d.canOverpenetrate = true;
        d.CoupleWith(targetPenetrable, ((spawnAlongLength*targetPenetrable.orificeLength)/d.GetLength()));
        penetrators.Add(d);
        return d;
    }
    public IEnumerator ReenableEggAfterSomeTime(Penetrator d) {
        yield return new WaitForSeconds(1f);
        d.autoPenetrate = true;
    }
    public IEnumerator SpawnEggs() {
        while(true) {
            Destroy(SpawnEgg().gameObject, 60f);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f,5f));
        }
    }
    public void OnValidate() {
#if UNITY_EDITOR
        penetratorPrefab.OnValidate();
#endif
    }
}
