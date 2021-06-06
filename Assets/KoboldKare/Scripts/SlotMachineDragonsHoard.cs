using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineDragonsHoard : MonoBehaviour{
    public event System.Action HandlePulled = delegate { };

    [SerializeField]
    private Text prizeText;

    [SerializeField]
    private SlotMachineRow[] rows;

    [SerializeField]
    private Transform handle;

    [SerializeField]
    public ScriptableFloat money;

    private int prizeValue;

    private float timeLastPlayed, continuousSessionTimeout, attractModeTimeout;

    private bool resultsChecked = false;

    public AudioClip won, bigwin, failed, started, startedShort;

    public AudioClip[] attract;

    public AudioSource gameAud, attractAud;

    public GameObject LEDLights;
    private Material LEDLightsMat;

    private void Awake(){
        LEDLightsMat = LEDLights.GetComponent<MeshRenderer>().material;
        attractModeTimeout = 20 + Random.Range(5, 12);
        continuousSessionTimeout = 8;
        StartCoroutine(AttractSubsystem());
    }

    public void CheckForUpdate(){
        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked){
            CheckResults();
        }
    }

    public void RunMachine(){
        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped){
            Started();
            StartCoroutine("PullHandle"); 
        }
    }

    private IEnumerator PullHandle(){
        /*for(int i = 0; i < 15; i += 5){
            handle.Rotate(0f, 0f, i);
            yield return new WaitForSeconds(0.1f);
        }*/

        HandlePulled();

        yield return new WaitForSeconds(0.2f);
    }

    private void CheckResults(){
        //Variables
        //0 - Ring
        //1 - Blue Stone
        //2 - Gold Bar
        //3 - Cup
        //4 - Hoard
        //5 - Silver Coin
        //6 - Scroll
        //7 - Treasure Chest

        //Strip Layout
        //Silver Coin - 2
        //Gold Bar - 2
        //Cup - 2
        //Ring - 2
        //Hoard - 1
        //Scroll - 1
        //Blue Stone - 1
        //Treasure Chest - 1

        //Design Layout
        //00 GB
        //01 CUP
        //02 SC
        //03 HOARD
        //04 GB
        //05 BS
        //06 SC
        //07 RING
        //08 CUP
        //09 SCRL
        //10 TC
        //11 RING


        //Silver Coin + Gold Bar + Hoard = All Coinage Treasure -> "ANY COIN"
        //Any Ring gives a small amount back


        //Combinations [in order of least to most payout]
        //Ring in any position -> 5
        //Paired Rings -> 25
        //Triple Silver Coin -> 30
        //Triple Gold Bar -> 40
        //Any 3 Coinage -> 50
        //Triple Cup -> 100
        //Triple Ring -> 300
        //Triple Hoard -> 500
        //Triple Blue Stone -> 1000
        //Triple Scroll -> 5000
        //Triple Treasure Chest -> 10000


        if (rows[0].stoppedSlot == "Silver Coin" 
            && rows[1].stoppedSlot == "Silver Coin"
            && rows[2].stoppedSlot == "Silver Coin"){

            prizeValue = 20;
            Win();
        }

        else if (rows[0].stoppedSlot == "Gold Bar"
            && rows[1].stoppedSlot == "Gold Bar"
            && rows[2].stoppedSlot == "Gold Bar"){

            prizeValue = 40;
            Win();
        }

        else if (rows[0].stoppedSlot == "Cup"
            && rows[1].stoppedSlot == "Cup"
            && rows[2].stoppedSlot == "Cup"){

            prizeValue = 100;
            Win();
        }

        else if (rows[0].stoppedSlot == "Ring"
            && rows[1].stoppedSlot == "Ring"
            && rows[2].stoppedSlot == "Ring"){

            prizeValue = 300;
            Win();
        }

        else if (rows[0].stoppedSlot == "Hoard"
            && rows[1].stoppedSlot == "Hoard"
            && rows[2].stoppedSlot == "Hoard"){

            BigWin();
            prizeValue = 500;
        }

        else if (rows[0].stoppedSlot == "Blue Stone"
            && rows[1].stoppedSlot == "Blue Stone"
            && rows[2].stoppedSlot == "Blue Stone"){

            BigWin();
            prizeValue = 1000;
        }

        else if (rows[0].stoppedSlot == "Scroll"
            && rows[1].stoppedSlot == "Scroll"
            && rows[2].stoppedSlot == "Scroll"){

            BigWin();
            prizeValue = 5000;
        }

        else if (rows[0].stoppedSlot == "Treasure Chest"
            && rows[1].stoppedSlot == "Treasure Chest"
            && rows[2].stoppedSlot == "Treasure Chest"){

            BigWin();
            prizeValue = 10000;
        }

        else if (rows[0].stoppedSlot == "Ring" || rows[1].stoppedSlot == "Ring" || rows[2].stoppedSlot == "Ring"){
            prizeValue += 5;
            Win();
        }

        else if (rows[0].stoppedSlot == "Ring" && rows[1].stoppedSlot == "Ring" || rows[1].stoppedSlot == "Ring" && rows[2].stoppedSlot == "Ring")
        {
            prizeValue += 25;
            Win();
        }

        else if (rows[0].stoppedSlot == "Silver Coin" || rows[0].stoppedSlot == "Gold Bar" || rows[0].stoppedSlot == "Hoard"){
            if(rows[1].stoppedSlot == "Silver Coin" || rows[1].stoppedSlot == "Gold Bar" || rows[1].stoppedSlot == "Hoard"){
                if(rows[2].stoppedSlot == "Silver Coin" || rows[2].stoppedSlot == "Gold Bar" || rows[2].stoppedSlot == "Hoard"){
                    prizeValue += 50;
                    Win();
                }
            }
        }

        else{
            Failed();
        }

        money.give(prizeValue);
        resultsChecked = true;

    }

    private void Win(){
        prizeText.enabled = true;
        prizeText.text = " You Won: $" + prizeValue;
        gameAud.PlayOneShot(won);
    }

    private void BigWin(){
        prizeText.enabled = true;
        prizeText.text = " BIG WINNER! $" + prizeValue;
        gameAud.PlayOneShot(bigwin);
    }

    private void Failed(){
        prizeText.enabled = true;
        prizeText.text = "Try your luck again!";
        gameAud.PlayOneShot(failed);
    }

    private void Started(){
        attractAud.Stop();

        if (Time.realtimeSinceStartup > continuousSessionTimeout + timeLastPlayed)
            gameAud.PlayOneShot(started);
        else
            gameAud.PlayOneShot(startedShort);

        timeLastPlayed = Time.realtimeSinceStartup;
        prizeValue = 0;
        prizeText.enabled = false;
        resultsChecked = false;
        LEDLightsMat.SetVector("_EmissionColor", Color.yellow * 5f);
    }

    private IEnumerator AttractSubsystem(){
        while (true){
            yield return new WaitForSeconds(  attractModeTimeout  );
            //Wait for long enough to actually play the attract mode
            if(timeLastPlayed+attractModeTimeout < Time.realtimeSinceStartup){
                //Choose a random attract song
                attractAud.Stop();
                attractAud.clip = attract[Random.Range(0, attract.Length - 1)];
                attractAud.Play();
                LEDLightsMat.SetVector("_EmissionColor", Color.yellow * (Random.Range(-1,1)*5f));
                prizeText.enabled = true;
                prizeText.text = "Only $1 to Play!";
            }
        }
    }

}
