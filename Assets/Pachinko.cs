using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using Photon;
using Photon.Pun;
using Photon.Realtime;

public class Pachinko : MonoBehaviourPun{
#region Definitions
    [Header("Pachinko!")]
    public GameObject pachinkoPrefab;
    public Transform spawnPoint;
    GameObject activeBall;
    
    AudioSource audioSrc;
    
    [Header("Audio Setup"),Space(10)]
    public AudioClip wonPrize;
    public AudioClip ballReset, hitPin, gameStart;
    [Space(10)]
    [Header("Game Restrictions")]
    [Range(0,5)]
    public int ballLimit;
    [Range(0,5)]
    public int ballsInPlay;
    [Header("Prize Setup"),Space(10)]
    bool prizesSetup = false;


    public List<Transform> prizeSlots = new List<Transform>();
    public PachinkoPrizeList prizeList;
    public List<PachinkoPrizeList.PrizeEntry> activePrizes = new List<PachinkoPrizeList.PrizeEntry>();

    [Header("Particle Effects")]
    public VisualEffect SpawnVFX;

#endregion

#region Top Level Code
    void Start(){
        audioSrc = GetComponent<AudioSource>();
    }

    public void StartGame(){
        if(prizesSetup == true){
            if(activeBall == null){
                SpawnBall();
                playSound(gameStart);
            }
        }
    }

    public void ResetGame(bool respawnBall = true){
        if(activeBall != null){
            Destroy(activeBall); //One ball per customer!
            if(respawnBall == true && ballsInPlay < ballLimit)
                SpawnBall();
        }
    }

    public void ShufflePrizes(){
        if(!prizesSetup){
            DestroyPrizeSlots();
            MakePrizeList(prizeList);
            AssignPrizes();
            prizesSetup = true;
        }
    }
    #endregion

#region Worker Methods
    void DestroyBall(){
        //Debug.Log("Ball destroyed");
        Destroy(activeBall);
        activeBall = null;
        ballsInPlay--;
    }

    void SpawnBall(){
        //Debug.Log("Ball spawned");
        activeBall = PhotonNetwork.Instantiate(pachinkoPrefab.name,spawnPoint.position,Quaternion.identity);
        activeBall.transform.SetParent(transform);
        activeBall.GetComponent<BallCheat>().SetMachine(this);
        activeBall.GetComponent<Rigidbody>().velocity = spawnPoint.transform.parent.GetComponent<Rigidbody>().velocity;
        ballsInPlay++;
    }

    void DistributePrize(int listIdx){
        //Debug.Log("Distributing prize in Index: "+listIdx+" at slot "+prizeSlots[listIdx].name);
        playSound(wonPrize);
        var award = PhotonNetwork.Instantiate(activePrizes[listIdx].prize.name,prizeSlots[listIdx].transform.position,Quaternion.identity);
        var vfx = Instantiate(SpawnVFX,prizeSlots[listIdx].transform.position,Quaternion.identity);

        //TODO: Play particle system to mask/explain instantaneous spawning
        if(award.GetComponent<Rigidbody>() != null)
            award.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 10f,ForceMode.VelocityChange); //Shoot the prize out from its spawn spot
    }

    void MakePrizeList(PachinkoPrizeList src){
        //Debug.Log("Making new prize list...");
        activePrizes.Clear();
        List<PachinkoPrizeList.PrizeEntry> prizes = new List<PachinkoPrizeList.PrizeEntry>();
        var runningTotal = 0f;
        for(int i = 0; i < prizeSlots.Count; i++){ //Get six prizes from our list
            var probSum = Random.Range(0,src.GetPrizes().Sum(p => p.chance));
            runningTotal = 0;
            foreach (var item in src.prizes){
                if(probSum < (runningTotal + item.chance)){
                    prizes.Add(item);
                    //Debug.Log("Adding this item to prize pool: "+item.prize.name);
                    break; //Jump to next prize slot
                }
                else
                    runningTotal+=item.chance;
            }
        }
        //Debug.Log("Prize list complete. Updated!");
        activePrizes = prizes;
    }

    void AssignPrizes(){
        //Debug.Log("Assigning prizes...");
        for(int i = 0; i < prizeSlots.Count; i++){
            var obj = PhotonNetwork.Instantiate(activePrizes[i].prize.name,prizeSlots[i].transform.position,Quaternion.identity);
            var children = obj.transform.GetComponentsInChildren<Transform>();
            obj.layer = 10; //Set to World to prevent theft
            foreach (var child in children){
                child.gameObject.layer = 10; // Also prevent children from being stolen (very important)
            }
            var vfx = Instantiate(SpawnVFX,prizeSlots[i].transform.position,Quaternion.identity);
            obj.transform.parent = prizeSlots[i].transform;
            obj.transform.localPosition = Vector3.zero; //Ensure it arrives at 0,0,0 regardless of prefabsettings.
            obj.GetComponent<Rigidbody>().useGravity = false;
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // Anti-Theft System
            //Debug.Log(string.Format("Added {0} to Prize Slot IDX {1}", activePrizes[i].prize.name,prizeSlots[i].name));
        }
    }

    void DestroyPrizeSlots(){
        foreach (var item in prizeSlots){ //First, clear the board
            if(item.transform.childCount != 0) //Don't destroy if there's nothing to destroy; risk of destroying self.
                Destroy(item.GetChild(0).gameObject);
        }
        activePrizes.Clear();
    }

    public void ReachedBottom(PachinkoBallZone src){
        //Debug.Log("Ball reached bottom!");
        DestroyBall();
        DistributePrize(src.zoneID);
        prizesSetup = false;
        ShufflePrizes();
        ResetGame(false);
    }
#endregion

#region Utilities
    public bool GameNotStarted(){
        return activeBall == null;
    }
    void playSound(AudioClip snd){
        if(snd == wonPrize || snd == ballReset){
            audioSrc.Stop();
            audioSrc.PlayOneShot(snd);
        }
        else if(!audioSrc.isPlaying){
            audioSrc.PlayOneShot(snd);
        }
    }

    public void BallStuck(){
        playSound(ballReset);
        ResetGame();
    }

    public void HitPin(){
        playSound(hitPin);
    }
    #endregion
}