using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class KoboldSeeker : MonoBehaviour{
    public List<Kobold> nearbyKobolds = new List<Kobold>();
    public Kobold curTarget;
    public NavMeshAgent agent;
    public Vector3 home;
    [Range(0.05f,0.5f)]
    public float sensorCheck;
    [Range(5f,400f)]
    public float sensorRange;
    
    [Range(0,30)]
    public int sensorFailedChecks, maxSensorFailedChecks;
    public LayerMask physMask;

    private SphereCollider sphereCollider;

    void Start(){
        home = transform.position;
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = sensorRange;
        StartCoroutine(DoubleCheck());
    }

    void OnTriggerEnter(Collider other){
        if(!nearbyKobolds.Contains(other.GetComponent<Kobold>())){
            //Debug.Log("Kobold entering: "+other.name);
            nearbyKobolds.Add(other.GetComponent<Kobold>());
            if(curTarget == null) //Only seek if we have nobody we're looking for
                SeekNewTarget();
        }
    }

    void OnTriggerExit(Collider other){
        if(nearbyKobolds.Contains(other.GetComponent<Kobold>())){
            //Debug.Log("Kobold exiting: "+other.name);
            nearbyKobolds.Remove(other.GetComponent<Kobold>());
            if(curTarget == other.GetComponent<Kobold>()){ //If we're looking for someone and they leave our trigger, stop looking for them
                curTarget = null;
                SeekNewTarget();
            }
            
        }
    }

    void Update(){
        if(curTarget != null){
            Color drawColor = Color.white;
            RaycastHit hit;
            if(Physics.Raycast(transform.position,curTarget.transform.position - transform.position, out hit, sensorRange, physMask)){
                if(hit.collider.gameObject.tag == "Player"){
                    drawColor = Color.red;
                    agent.SetDestination(curTarget.transform.position);
                }
                else{
                    if(curTarget != null){
                        //Debug.Log("In can't see state");
                        if(sensorFailedChecks >= maxSensorFailedChecks){
                            //Debug.Log("Lost attention!");
                            curTarget = null;
                            sensorFailedChecks = 0;
                            agent.SetDestination(home);
                        }
                        if(sensorFailedChecks < maxSensorFailedChecks){
                            //Debug.Log("Losing attention...");
                            sensorFailedChecks++;
                            //Debug.DrawLine(transform.position,curTarget.transform.position,Color.green,10f);
                        }
                    }
                }
            }
            else{
                //if(curTarget != null)
                //Debug.DrawLine(transform.position,curTarget.transform.position,drawColor,0.2f);
            }
        }
        else
            agent.SetDestination(home);
    }

    void SeekNewTarget(int listPos = 0){
        Debug.Log("Seeking new target");
        //Extremely Dumb AI
        if(nearbyKobolds.Count != 0){
            sensorFailedChecks = 0;
            curTarget = nearbyKobolds[listPos];
            agent.SetDestination(curTarget.transform.position);
        }
        else
            curTarget = null;
    }

    IEnumerator DoubleCheck(){
        while(true){
            yield return new WaitForSeconds(sensorCheck); //Run infrequently to save on perf
            if(curTarget == null){ //We only care about this when we aren't looking for someone
                //Debug.Log("Looking for new candidate");
                RaycastHit hit;
                for(int i = 0; i < nearbyKobolds.Count; i++){
                    if(nearbyKobolds[i] == null) {nearbyKobolds.Remove(nearbyKobolds[i]); break;} //If null, remove that item and break out of loop for safety
                    if(Physics.Raycast(transform.position,(nearbyKobolds[i].transform.position - transform.position), out hit, sensorRange, physMask)){
                        //Debug.DrawRay(transform.position, item.transform.position - transform.position);
                        if(hit.collider.gameObject.tag == "Player"){
                            //Debug.DrawLine(transform.position,item.transform.position,Color.black,5f);
                            SeekNewTarget(nearbyKobolds.FindIndex(x => x == nearbyKobolds[i]));
                            break;            
                        }
                        else{
                        //Debug.Log("Attempting to find Kobold, found: "+hit.collider.gameObject.tag+" with name "+hit.collider.gameObject.name);
                        //Debug.DrawLine(transform.position, item.transform.position,Color.green,0.2f);
                        }
                    }
                }
            }
        }    
    }
}
