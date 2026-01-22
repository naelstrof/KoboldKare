using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using SimpleJSON;

[RequireComponent(typeof(AudioSource))]
public class Pachinko : MonoBehaviour {
#region Definitions
    [System.Serializable]
    public class Prize {
        public Transform location;
        public PrefabDatabase prizeSpawn;
        public VisualEffect spawnVFX;
        public Shader displayShader;
        
        private GameObject display;
        private PrefabReferenceInfo prefabReference;
        private AssetGroup.AssetLocation.AssetHandle<GameObject> prefabHandle;
        
        private VisualEffect spawnVFXInstance;
        
        public async Task Spawn() {

            if (!prizeSpawn.TryGetGroupName(out var groupName)) {
                return;
            }
            
            if (!KoboldKareObjectPostProcessor.TryGetRandomAssetKey(groupName, out var assetKey)) {
                return;
            }
            
            prefabReference = new PrefabReferenceInfo(groupName, assetKey);
            prefabHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(prefabReference.GetGroupName(), prefabReference.GetKey(), GameManager.GetErrorGeneric());
            
            if (display != null) {
                Destroy(display);
            }
            
            if (prefabReference != null && prefabHandle.asset != null) {
                display = GenericPurchasable.GenerateDisplay(prefabHandle.asset, displayShader, location);
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
    private NetworkedEntity networkedEntity;
    
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
    void Start(){
        floater.SetBounds(GetComponent<Renderer>().bounds);
        floater.SetText(playCost.ToString());
        audioSrc = GetComponent<AudioSource>();
        foreach(var prize in prizes) {
            prize.Spawn();
        }

        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private void OnUse(NetworkedKobold k) {
        if (k.TryGetKobold(out var kobold)) {
            kobold.GetComponent<MoneyHolder>().ChargeMoney(playCost);
        }
        StartGame();
        // FIXME FISHNET
        // photonView.RPC("RPCUse", RpcTarget.All, new object[]{});
    }

    private bool OnUseRequested(NetworkedKobold k) {
        return (k && k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().HasMoney(playCost)) && activeBall == null;
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
}