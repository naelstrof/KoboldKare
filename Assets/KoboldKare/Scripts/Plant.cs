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
public class Plant : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback, ISavable {
    public ScriptablePlant plant;
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

    void Start() {
        container = GetComponent<GenericReagentContainer>();
        container.OnFilled.AddListener(OnFilled);
        midnightEvent.AddListener(OnEventRaised);
        SwitchTo(plant);
    }

    void OnDestroy() {
        container.OnFilled.RemoveListener(OnFilled);
        midnightEvent.RemoveListener(OnEventRaised);
    }

    void OnFilled(GenericReagentContainer.InjectType injectType) {
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
    }

    void SwitchTo(ScriptablePlant newPlant) {
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
                    GameObject obj =PhotonNetwork.Instantiate(produce.prefab.photonName, transform.position, Quaternion.identity);
                    if (obj.GetComponent<Kobold>() != null) {
                        obj.GetComponent<Kobold>().RandomizeKobold();
                    }
                }
            }
        }
        plant = newPlant;
    }

    public void OnEventRaised(object e) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0f) {
            PhotonNetwork.Destroy(gameObject);
            return;
        }
        if (container.isFull) {
            container.Spill(container.volume);
            SwitchTo(plant.possibleNextGenerations[UnityEngine.Random.Range(0, plant.possibleNextGenerations.Length)]);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is short) {
            SwitchTo(PlantDatabase.GetPlant((short)info.photonView.InstantiationData[0]));
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
            tgtMat.color = Color.Lerp(tgtMat.color, darkenedColor, timeSoFar/timeToFadeWatered);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsReading) {
            SwitchTo(PlantDatabase.GetPlant((short)stream.ReceiveNext()));
        } else {
            stream.SendNext(PlantDatabase.GetID(plant));
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(PlantDatabase.GetID(plant));
    }

    public void Load(BinaryReader reader, string version) {
        SwitchTo(PlantDatabase.GetPlant(reader.ReadInt16()));
    }
}
