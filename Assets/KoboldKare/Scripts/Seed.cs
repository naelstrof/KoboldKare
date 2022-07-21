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
            SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetPlantable()) {
                return true;
            }
        }
        return false;
    }

    [PunRPC]
    public override void Use() {
        if (!photonView.IsMine || !CanUse(null)) {
            return;
        }
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<hitCount;i++) {
            SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetPlantable()) {
                PhotonNetwork.Instantiate(plantPrefab.photonName, tile.GetPlantPosition(), Quaternion.LookRotation(Vector3.forward, Vector3.up), 0, new object[] {PlantDatabase.GetID(plant)} );
                PhotonNetwork.Destroy(gameObject);
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
