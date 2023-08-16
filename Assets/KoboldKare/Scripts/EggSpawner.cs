using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PenetrationTech;

public class EggSpawner : MonoBehaviour {
    private class PenetratorCoupler {
        public Penetrator penetrator;
        public Penetrable penetrable;
        public Rigidbody body;
        public float pushAmount;
    }

    public Penetrable targetPenetrable;
    [Range(0f,1f)]
    public float spawnAlongLength = 0.5f;
    [Range(-1,1f)]
    public float pushDirection = -1f;
    public PhotonGameObjectReference penetratorPrefab;
    private List<PenetratorCoupler> penetrators;

    private void Awake() {
        penetrators = new List<PenetratorCoupler>();
    }

    public void Update() {
        for(int i=0;i<penetrators.Count;i++) {
            var coupler = penetrators[i];
            if (coupler.pushAmount < coupler.penetrator.GetWorldLength()) {
                CatmullSpline path = coupler.penetrable.GetPath();
                Vector3 position = path.GetPositionFromT(0f);
                Vector3 tangent = path.GetVelocityFromT(0f).normalized;
                coupler.pushAmount += Time.deltaTime*0.4f;
                coupler.body.transform.position = position - tangent * coupler.pushAmount;
                continue;
            }

            coupler.body.isKinematic = false;
            penetrators.RemoveAt(i);
        }
    }
    public Penetrator SpawnEgg(float eggVolume) {
        //Penetrator d = GameObject.Instantiate(penetratorPrefab).GetComponentInChildren<Penetrator>();
        CatmullSpline path = targetPenetrable.GetPath();
        Penetrator d = Photon.Pun.PhotonNetwork.Instantiate(penetratorPrefab.photonName,path.GetPositionFromT(0f), Quaternion.LookRotation(path.GetVelocityFromT(0f).normalized,Vector3.up)).GetComponentInChildren<Penetrator>();
        if (d == null) {
            return null;
        }

        Rigidbody body = d.GetComponentInChildren<Rigidbody>();
        d.GetComponent<GenericReagentContainer>().OverrideReagent(ReagentDatabase.GetReagent("ScrambledEgg"), eggVolume);
        //d.GetComponent<GenericInflatable>().TriggerTween();
        body.isKinematic = true;
        // Manually control penetration parameters
        d.Penetrate(targetPenetrable);
        penetrators.Add(new PenetratorCoupler(){penetrable = targetPenetrable, penetrator = d, body = body, pushAmount = 0f});
        return d;
    }
    public void OnValidate() {
#if UNITY_EDITOR
        penetratorPrefab.OnValidate();
#endif
    }
}
