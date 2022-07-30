using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;
using KoboldKare;
using System.IO;

[RequireComponent(typeof(GenericReagentContainer))]
public class Plant : GeneHolder, IPunObservable, IPunInstantiateMagicCallback, ISavable {
    public ScriptablePlant plant;
    [SerializeField]
    private GenericReagentContainer container;
    [SerializeField]
    private GameEventGeneric midnightEvent;

    [SerializeField]
    public float timeToFadeWatered;

    [SerializeField]
    public Color darkenedColor;

    [SerializeField]
    private VisualEffect effect, wateredEffect;
    [SerializeField]
    private GameObject display;

    [SerializeField]
    public AudioSource audioSource;
    public bool spawnedFromLoad = false;

    public delegate void SwitchAction();
    public SwitchAction switched;

    void Start() {
        if(GetComponent<GenericReagentContainer>() != null){
            container = GetComponent<GenericReagentContainer>();
        }
        else{
            Debug.LogWarning("[Plant] :: Attempted to refresh link to container but failed as container was not on same-level as Plant or does not exist");
        }
        container.OnFilled.AddListener(OnFilled);
        midnightEvent.AddListener(OnEventRaised);
        if(!spawnedFromLoad){
            SwitchTo(plant);
        }
        else{
            SwitchTo(plant,true);
        }
    }

    void OnDestroy() {
        container.OnFilled.RemoveListener(OnFilled);
        midnightEvent.RemoveListener(OnEventRaised);
    }

    void OnFilled(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0) {
            return;
        }
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            renderer.material.SetFloat("_BounceAmount", 1f);
            StartCoroutine(DarkenMaterial(renderer.material));
        }
        wateredEffect.SendEvent("Play");
        audioSource.Play();
        effect.gameObject.SetActive(false);
        effect.gameObject.SetActive(true);
        //Debug.Log(gameObject.name+"'s container filled state is: "+container.isFull);
        //Debug.Log(gameObject.name+"'s contents: "+container.ToString());
    }

    void SwitchTo(ScriptablePlant newPlant, bool loaded = false) {
        if(!loaded){ // Don't allow this to run if we're running this from save to prevent generation skip
            UndarkenMaterials();
            wateredEffect.Stop();
            if (plant == newPlant) {
                return;
            }
            if (display != null) {
                Destroy(display);
            }
            if (newPlant.display != null) {
                display = GameObject.Instantiate(newPlant.display, transform);
            }
            if (photonView.IsMine) {
                foreach(var produce in newPlant.produces) {
                    int max = UnityEngine.Random.Range(produce.minProduce, produce.maxProduce);
                    for(int i=0;i<max;i++) {
                        GameObject obj = PhotonNetwork.Instantiate(produce.prefab.photonName, transform.position, Quaternion.identity, 0, new object[]{GetGenes()});
                    }
                }
            }
            plant = newPlant;
        }
        else{ // Behavior for when the plant is being spawned as part of deserialization and not spawning
            // Plant == newPlant should always return true for deserialization, skip that step and assert
            if(display == null){
                Destroy(display);
            }
            if(newPlant.display != null){
                display = GameObject.Instantiate(newPlant.display,transform);
            }
                    //Don't fully replicate injection; no need to play hearts and poofs as if it were just watered.
            // Debug.Log("[Plant] :: <Deserialization> Running container check for isFull");
            if(container != null){
                if(container.isFull){
                    // Debug.Log("[Plant] :: <Deserialization> Container was full, running effects");
                    wateredEffect.SendEvent("Play");
                    foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
                        renderer.material.SetFloat("_BounceAmount", 1f);
                        StartCoroutine(DarkenMaterial(renderer.material));
                    }

                    // Debug.Log("[Plant] :: <Deserialization> Effects ran and/or deserialization complete");
                }
                else{
                    // Debug.Log("[Plant] :: <Deserialization> Container was not full");
                }
            }
            else{
                // Debug.LogError("[Plant] :: <Deserialization> Plant with name "+gameObject.name+" failed to deserialize. Reason: Container reference was null");
            }
        }
        switched?.Invoke();
    }

    public void OnEventRaised(object e) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0f) {
            PhotonNetwork.Destroy(gameObject);
            return;
        }
        if (container.isFull) {
            // Debug.Log("[Plant] Container was full, running SwitchTo() for next random generation");
            container.Spill(container.volume);
            SwitchTo(plant.possibleNextGenerations[UnityEngine.Random.Range(0, plant.possibleNextGenerations.Length)]);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is short) {
            SwitchTo(PlantDatabase.GetPlant((short)info.photonView.InstantiationData[0]));
            SetGenes((KoboldGenes)info.photonView.InstantiationData[1]);
            Debug.Log(GetGenes());
        }
    }

    void UndarkenMaterials(){
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            renderer.material.color = Color.white;
        }
    }

    IEnumerator DarkenMaterial(Material tgtMat){
        var timeSoFar = 0f;
        while(timeSoFar < timeToFadeWatered){
            yield return new WaitForSeconds(0.10f); //Updates 10 times per second; doesn't need to be RT.
            timeSoFar += 0.10f;
            if(tgtMat.color != null){
                tgtMat.color = Color.Lerp(tgtMat.color, darkenedColor, timeSoFar/timeToFadeWatered);
            } else {
                Debug.LogWarning("[Plant] :: Can not set _color/color of materials whose shaders do not have a color property");
            }
            
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsReading) {
            SwitchTo(PlantDatabase.GetPlant((short)stream.ReceiveNext()));
            //SetGenes((KoboldGenes)stream.ReceiveNext());
        } else {
            stream.SendNext(PlantDatabase.GetID(plant));
            //stream.SendNext(GetGenes());
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(PlantDatabase.GetID(plant));
        writer.Write(transform.position.x);
        writer.Write(transform.position.y);
        writer.Write(transform.position.z);
        GetGenes().Serialize(writer);
    }

    public void Load(BinaryReader reader, string version) {
        SwitchTo(PlantDatabase.GetPlant(reader.ReadInt16()));
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        transform.position = new Vector3(x,y,z);
        SetGenes(GetGenes().Deserialize(reader));
    }
}
