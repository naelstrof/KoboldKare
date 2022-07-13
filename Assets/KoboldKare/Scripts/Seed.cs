using KoboldKare;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class Seed : GenericUsable, IValuedGood {
    //public List<GameObject> _plantPrefabs;
    [SerializeField]
    private float worth = 5f;
    [SerializeField]
    private Sprite displaySprite;
    public PhotonGameObjectReference plantPrefab;
    public int type = 0;
    public float _spacing = 1f;
    public UnityEvent OnFailPlant;
    public ScriptablePlant plant;
    private Collider[] hitColliders = new Collider[16];
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override bool CanUse(Kobold k) {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<hitCount;i++) {
            if (!hitColliders[i].CompareTag("PlantableTerrain") && !hitColliders[i].CompareTag("NoBlockPlant")) {
                return false;
            }
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f, GameManager.instance.plantHitMask, QueryTriggerInteraction.Collide)) {
            if (hit.collider.CompareTag("PlantableTerrain")) {
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore)) {
                    return true;
                }
            }
        }
        return false;
    }
    [PunRPC]
    public override void Use() {
        if (!photonView.IsMine || !CanUse(null)) {
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f, GameManager.instance.plantHitMask, QueryTriggerInteraction.Collide)) {
            if (hit.collider.CompareTag("PlantableTerrain")) {
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore)) {
                    PhotonNetwork.Instantiate(plantPrefab.photonName, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal), 0, new object[] {PlantDatabase.GetID(plant)} );
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
    public float GetWorth() {
        return worth;
    }
    public void OnValidate() {
        plantPrefab.OnValidate();
    }
}
