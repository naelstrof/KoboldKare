using System.Collections;
using UnityEngine;

public class SlotMachineRow : MonoBehaviour{

    private float timeInterval;

    public bool rowStopped;
    public string stoppedSlot;

    public GameObject strip;

    public SlotMachineDragonsHoard myMachine;

    public AudioClip tickNoise, slotLocked;

    private AudioSource myAud;

    private float stripLength = 30.625f;
    private float lengthBetweenSlots = 2.5f;
    private float movementLength = 0.64f;
    //private float slotOffset = 0.095f;
    private float slotOffset = 0f;
    private float randomVal = 0.0f;

    private void Start(){
        rowStopped = true;
        myMachine.HandlePulled += StartRotating;
        myAud = gameObject.GetComponent<AudioSource>();
    }

    private void StartRotating(){
        stoppedSlot = "";
        StartCoroutine("Rotate");
    }

    private void Click(){
        myAud.PlayOneShot(tickNoise);
    }

    private void Clunk(){
        myAud.PlayOneShot(slotLocked);
    }

    private IEnumerator Rotate(){
        //Y Distance between Slots - 2.5
        //Divide into 4 -> 0.625f --> !! Corrected to 0.64 with a -0.1f offset for more accurate midlines !!
        //====Example Diagram demonstrating 6 rounds of Movement versus Valid Position====
        //CUP           > START < 
        //EMPTY         V
        //EMPTY         V
        //EMPTY         V
        //EMPTY         V
        //SILVER COIN   V
        //EMPTY         > STOP <  --> RETURNS EMPTY, NOT SILVER COIN

        //FROM ORIGIN POINT OF Y [0]
        // 49 STEPS UP          |   -15.3125f
        // 49 STEPS DOWN        |    15.3125f
        // TOTAL STRIP LENGTH   |   30.625f

        rowStopped = false;
        timeInterval = 0.025f;

        //!! Remember to reseed at game start for true randomness
        Random.InitState(Random.Range(0,9999));
        randomVal = movementLength * Mathf.RoundToInt(Random.Range(80, 114));
        //Debug.Log(gameObject.name + " randomVal: " + randomVal);
        //Debug.Log(Mathf.FloorToInt(strip.transform.localPosition.y / 2.5f) % 12 + 6);


        for(int i = 0; i < randomVal; i++){
            if (strip.transform.localPosition.y <= -(stripLength / 2)) // When we are on the last possible position, loop back to the top
                strip.transform.localPosition = new Vector3(0f, (stripLength / 2), 0f);

            strip.transform.localPosition = new Vector3(0f, slotOffset + (strip.transform.localPosition.y - movementLength), 0f);

            if (i > Mathf.RoundToInt(randomVal * 0.5f))
                timeInterval = 0.05f;
            if (i > Mathf.RoundToInt(randomVal * 0.7f))
                timeInterval = 0.08f;
            if (i > Mathf.RoundToInt(randomVal * 0.95f))
                timeInterval = 0.16f;
            if (i > Mathf.RoundToInt(randomVal * 0.98f))
                timeInterval = 0.24f;

            Click();

            yield return new WaitForSeconds(timeInterval);
        }

        //ACTUAL LAYOUT (4 BETWEEN EACH POSITION [2.56])
        //VALUE     | Y POSITION
        //===========================
        //00 GB     | -15.3125 ** -15.40751
        //01 CUP    | -12.7525 ** -12.8475
        //02 SC     | -10.1925 ** -10.2875
        //03 HOARD  | -7.6325 ** -7.727501
        //04 GB     | -5.0725 ** -5.167502
        //05 BS     | -2.5125 ?? -2.6075
        //06 SC     | 0.0475 ** 0.04750
        //07 RING   | 2.6075 ** 2.512497
        //08 CUP    | 5.1675 ?? 5.0725
        //09 SCRL   | 7.7275 ** 7.632496 == -0.095 diff
        //10 TC     | 10.2875 ** 10.1925 == -0.095 diff
        //11 RING   | 12.8475 ** 12.7525 == 0.095 diff
        //12 GB     | 15.4075 ?? 15.3125

        var currentYPos = strip.transform.localPosition.y;
        stoppedSlot = "Empty";

        

        if (FloatCompare(currentYPos,-15.40751f))
            stoppedSlot = "Gold Bar";

        if (FloatCompare(currentYPos,-12.8475f))
            stoppedSlot = "Cup";

        if (FloatCompare(currentYPos,-10.2875f))
            stoppedSlot = "Silver Coin";

        if (FloatCompare(currentYPos,-7.727501f))
            stoppedSlot = "Hoard";

        if (FloatCompare(currentYPos,-5.167502f))
            stoppedSlot = "Gold Bar";

        if (FloatCompare(currentYPos,-2.607502f))
            stoppedSlot = "Blue Stone";

        if (FloatCompare(currentYPos,0.04750f))
            stoppedSlot = "Silver Coin";

        if (FloatCompare(currentYPos,2.512497f))
            stoppedSlot = "Ring";

        if (FloatCompare(currentYPos,5.0725f))
            stoppedSlot = "Cup";

        if (FloatCompare(currentYPos,7.632496f))
            stoppedSlot = "Scroll";

        if (FloatCompare(currentYPos,10.1925f))
            stoppedSlot = "Treasure Chest";

        if (FloatCompare(currentYPos,12.7525f))
            stoppedSlot = "Ring";

        if (FloatCompare(currentYPos,15.3125f))
            stoppedSlot = "Gold Bar";

        rowStopped = true;
        Clunk();

        myMachine.CheckForUpdate();

        yield return new WaitForSeconds(1f);
    }

    private void OnDestroy(){
        myMachine.HandlePulled -= StartRotating;

    }

    private bool FloatCompare(float a, float b){
        if (Mathf.Approximately(a, b))
            return true;
        else
            return false;
    }
}
