using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class CateringHandler : MonoBehaviour{
    public GameEventGeneric morningEvent, middayEvent, midnightEvent;
    public float cateringChance = 0.15f;
    public bool vanArrived = false;
    public List<MeshRenderer> cateringVan = new List<MeshRenderer>();
    public List<MeshRenderer> cateringVanCity = new List<MeshRenderer>();
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
        var rnd = Random.Range(0f,1f);
        //Debug.Log(string.Format("{0} vs {1}",rnd,cateringChance));
        if(rnd < cateringChance){
            aud.PlayOneShot(catererArrives);
            foreach (var item in cateringVan){
                item.enabled = true;
                item.GetComponent<MeshCollider>().enabled = true;
            }
            foreach (var item in cateringVanCity){
                item.enabled = false;
                item.GetComponent<MeshCollider>().enabled = false;
            }
            vanArrived = true;
            Debug.Log("Caterer arrived!");
        }
    }

    void AssignAwards(object nothing){
        if(vanArrived){ //Don't assign rewards if van 'leaves' despite not being there
            //Assign rewards
            catererHurryUp = StartCoroutine(HurryUp());
        }
    }

    void LeaveImmediately(object nothing){
        foreach (var item in cateringVan){
            item.enabled = false;
            item.GetComponent<MeshCollider>().enabled = false;
        }
        foreach (var item in cateringVanCity){
            item.enabled = true;
            item.GetComponent<MeshCollider>().enabled = true;
        }
        if(catererHurryUp != null){
            StopCoroutine(catererHurryUp);
            catererHurryUp = null;
        }
        Debug.Log("Caterer left; nobody was home!");
    }

    IEnumerator HurryUp(){
        if(!aud.isPlaying)
            aud.PlayOneShot(catererArrives);
        Debug.Log("Caterer is about to leave!");
        yield return new WaitForSeconds(20f);
        //Do rewards logic here
        //Get stuff inside the van's collider, tally their values up, assign that amount of money to the Farm
        //Van poofs out of existence
        Debug.Log("Caterer headed back to the city!");
        foreach (var item in cateringVan){
            item.enabled = false;
            item.GetComponent<MeshCollider>().enabled = false;
        }
        foreach (var item in cateringVanCity){
            item.enabled = true;
            item.GetComponent<MeshCollider>().enabled = true;
        }
        aud.PlayOneShot(catererLeaves);
    }
}
