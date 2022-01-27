using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class IrrigationZone : MonoBehaviour{
    public GenericReagentContainer grc;

    private HashSet<GenericReagentContainer> members = new HashSet<GenericReagentContainer>();

    public HashSet<GenericReagentContainer> GetContainers(){return members;}
    public int amountToInject;
    LayerMask waterMask;
    public List<VisualEffect> vfx = new List<VisualEffect>();
    public AudioSource aud;

    void Start(){
        waterMask = GameManager.instance.waterSprayHitMask;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position,transform.localScale);
    }

    public void InjectReagent(){
        //Debug.Log("[Irrigation Zone] :: Running Reagent Injection Sequence with LayerMask : "+waterMask);
        //Physics.SyncTransforms(); //Enforce correct positions before attempting injections? Unsure if safe with Photon
        
        /*Debug.Log("==DUMPING ALL POSSIBLE CANDIDATES==");
        var allCandidates = Physics.OverlapBox(transform.position,transform.localScale*0.5f,Quaternion.identity,waterMask,QueryTriggerInteraction.Collide);
        foreach (var item in allCandidates){
            Debug.Log(item.gameObject.name);
        }
        Debug.Log("==DUMP COMPLETE==");*/

        //Get candidates
        var candidates = Physics.OverlapBox(transform.position,transform.localScale/2,Quaternion.identity,waterMask,QueryTriggerInteraction.UseGlobal);
        if(candidates.Length == 0) {
            Debug.Log("[Irrigation Zone] :: Found no candidates!");
        }

        foreach (var item in candidates){
            if(item.GetComponentInParent<GenericReagentContainer>() != null){
                //foreach(var grc in item.GetComponentInParent<GenericReagentContainer>()){
                    //Debug.Log("[Irrigation Zone] :: Adding new component to the irrigation zone candidate list: "+item.gameObject.name);
                    var tgt = item.GetComponentInParent<GenericReagentContainer>();
                    if(tgt.volume < tgt.maxVolume){ //Don't bother attempting to add to containers that are full
                        members.Add(tgt);
                    }
                    
                //}
            }
        }

        //Divide amountToInject equally into the member objects
        foreach (var item in members){
            if(item != null){
                //Debug.Log("[Irrigation Zone] :: Injecting proportional fluids into object: "+item.name);
                var fluidContents = grc.Spill(amountToInject);
                item.AddMix(fluidContents,GenericReagentContainer.InjectType.Spray);
            }
            else{
                Debug.Log("[Irrigation Zone] :: Somehow GRC became invalid between setup and this frame!");
            }
        }


        //Play Visual Effects
        foreach (var item in vfx){
            item.Stop();
            item.Reinit();
            item.SendEvent("Fire");
        }

        //Play audio
        if(!aud.isPlaying){
            aud.Play();
        }
        //Debug.Log("[Irrigation Zone] :: Reagent Injection Sequence Complete!");
    }
}
