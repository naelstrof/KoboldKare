using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using SimpleJSON;

[RequireComponent(typeof(AudioSource))]
public class Pachinko : GenericUsable {
#region Definitions
    [System.Serializable]
    public class Prize {
        public Transform location;
        public PrefabDatabase prizeSpawn;
        public VisualEffect spawnVFX;
        public Shader displayShader;
        
        private GameObject display;
        private PrefabDatabase.PrefabReferenceInfo prefabReference;
        private VisualEffect spawnVFXInstance;
        public void Spawn() {
            if (!prizeSpawn.TryGetRandom(out var info)) {
                return;
            }
            prefabReference = info;
            if (display != null) {
                Destroy(display);
            }
            
            if (prefabReference != null && prefabReference.GetPrefab() != null) {
                display = GenericPurchasable.GenerateDisplay(prefabReference.GetPrefab(), displayShader, location);
            }

            //ScriptablePurchasable.DisableAllButGraphics(gobj);
            if (spawnVFXInstance == null) {
                spawnVFXInstance = Instantiate(spawnVFX, location);
            }
        }
        public void Claim() {
            // FIXME FISHNET
            throw new UnityException();
            /*
            GameObject award = PhotonNetwork.Instantiate(prefabReference.GetKey(), location.position, location.rotation);
            //TODO: Play particle system to mask/explain instantaneous spawning
            if(award.GetComponent<Rigidbody>() != null) {
                award.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 10f, ForceMode.VelocityChange); //Shoot the prize out from its spawn spot
            }
            spawnVFXInstance.Play();
            Spawn();*/
        }
    }
    [Header("Pachinko!")]
    [SerializeField]
    private MoneyFloater floater;
    [SerializeField]
    private Sprite displaySprite;
    public float playCost = 50f;
    public PhotonGameObjectReference pachinkoBallPrefab;
    public Transform ballSpawnPoint;
    GameObject activeBall;
    
    [SerializeField]
    public new ConstantForce constantForce;
    
    AudioSource audioSrc;
    [Header("Audio Setup"),Space(10)]
    public AudioClip wonPrize;
    public AudioClip ballReset, hitPin, gameStart;
    [Space(10)]
    [Header("Prize Setup"),Space(10)]
    public List<Prize> prizes = new List<Prize>();
#endregion

#region Top Level Code
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    void Start(){
        floater.SetBounds(GetComponent<Renderer>().bounds);
        floater.SetText(playCost.ToString());
        audioSrc = GetComponent<AudioSource>();
        foreach(var prize in prizes) {
            prize.Spawn();
        }
    }

    public override void LocalUse(Kobold k) {
        k.GetComponent<MoneyHolder>().ChargeMoney(playCost);
        // FIXME FISHNET
        // photonView.RPC("RPCUse", RpcTarget.All, new object[]{});
    }

    public override bool CanUse(Kobold k) {
        return (k==null || k.GetComponent<MoneyHolder>().HasMoney(playCost)) && activeBall == null;
    }

    public override void Use() {
        StartGame();
    }

    public void StartGame(){
        // FIXME FISHNET
        throw new NotImplementedException();
        /*if (!photonView.IsMine) {
            return;
        }*/
        SpawnBall();
        audioSrc.clip = gameStart;
        audioSrc.Play();
    }

    public void ResetGame(){
        // FIXME FISHNET
        throw new NotImplementedException();
        /*if(photonView.IsMine && activeBall != null){
            PhotonNetwork.Destroy(activeBall); //One ball per customer!
        }*/
    }
    #endregion

#region Worker Methods
    void DestroyBall(){
        // FIXME FISHNET
        throw new NotImplementedException();
        /*
        if (!photonView.IsMine) {
            return;
        }
        PhotonNetwork.Destroy(activeBall);
        */
    }

    void SpawnBall(){
        // FIXME FISHNET
        /*
        if (!photonView.IsMine) {
            return;
        }
        //Debug.Log("Ball spawned");
        activeBall = PhotonNetwork.Instantiate(pachinkoBallPrefab.photonName,ballSpawnPoint.position,Quaternion.identity, 0, new object[]{photonView.ViewID});
        // We set the machine via instantiation data, since other clients would just have the ball "appear".
        //activeBall.GetComponent<BallCheat>().SetMachine(this);
        //activeBall.GetComponent<Rigidbody>().velocity = ballSpawnPoint.transform.parent.GetComponent<Rigidbody>().velocity;
        activeBall.GetComponent<Rigidbody>().velocity = constantForce.force;
        */
    }

    void DistributePrize(int listIdx){
        // FIXME FISHNET
        /*
        if (!photonView.IsMine) {
            return;
        }*/
        audioSrc.clip = wonPrize;
        audioSrc.Play();
        prizes[listIdx].Claim();
    }

    public void ReachedBottom(PachinkoBallZone src){
        //Debug.Log("Ball reached bottom!");
        DestroyBall();
        DistributePrize(src.zoneID);
    }
#endregion

#region Utilities
    public void BallStuck(){
        audioSrc.PlayOneShot(ballReset);
        ResetGame();
    }

    public void HitPin(){
        audioSrc.PlayOneShot(hitPin);
    }
    #endregion
    void OnValidate() {
        pachinkoBallPrefab.OnValidate();
    }
    public override void Save(JSONNode node) { }
    public override void Load(JSONNode node) { }
}