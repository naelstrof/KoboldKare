using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class CateringHandler : MonoBehaviour{
    public GameEventGeneric morningEvent, middayEvent, midnightEvent;
    public float cateringChance = 0.15f;
    public GameObject cateringVan;
    public AudioClip catererArrives, catererLeaves;
    Coroutine catererHurryUp;
    public AudioSource aud;

    void Start(){
        morningEvent.AddListener(evalCatering);
        middayEvent.AddListener(AssignAwards);
        midnightEvent.AddListener(LeaveImmediately);
    }

    void OnDestroy(){
        morningEvent.RemoveListener(evalCatering);
        middayEvent.RemoveListener(AssignAwards);
        midnightEvent.RemoveListener(LeaveImmediately);
    }

    void evalCatering(object nothing){ 
        if(Random.Range(0,1) < cateringChance){
            aud.PlayOneShot(catererArrives);
            cateringVan.SetActive(true);
            Debug.Log("Caterer arrived!");
        }
    }

    void AssignAwards(object nothing){
        if(cateringVan.activeInHierarchy){ //Don't assign rewards if van 'leaves' despite not being there
            //Assign rewards
            catererHurryUp = StartCoroutine(HurryUp());
        }
    }

    void LeaveImmediately(object nothing){
        cateringVan.SetActive(false);
        if(catererHurryUp != null){
            StopCoroutine(catererHurryUp);
            catererHurryUp = null;
        }
        Debug.Log("Caterer left; nobody was home!");
    }

    IEnumerator HurryUp(){
        aud.PlayOneShot(catererArrives);
        Debug.Log("Caterer is about to leave!");
        yield return new WaitForSeconds(20f);
        //Do rewards logic here
        //Get stuff inside the van's collider, tally their values up, assign that amount of money to the Farm
        //Van poofs out of existence
        Debug.Log("Caterer headed back to the city!");
        cateringVan.SetActive(false);
        aud.PlayOneShot(catererLeaves);
    }
}
