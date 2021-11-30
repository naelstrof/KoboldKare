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

[RequireComponent(typeof(GenericReagentContainer))]
public class Plant : MonoBehaviourPun, IGameEventListener, IPunObservable, IPunInstantiateMagicCallback {
    public ScriptablePlant plant;
    private GenericReagentContainer container;
    [SerializeField]
    private GameEvent midnightEvent;
    [SerializeField]
    private VisualEffect effect;
    [SerializeField]
    private GameObject display;

    void Start() {
        container = GetComponent<GenericReagentContainer>();
        container.OnFilled.AddListener(OnFilled);
        midnightEvent.RegisterListener(this);
        SwitchTo(plant);
    }

    void OnDestroy() {
        container.OnFilled.RemoveListener(OnFilled);
        midnightEvent.UnregisterListener(this);
    }

    void OnFilled(GenericReagentContainer.InjectType injectType) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0) {
            return;
        }
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            renderer.material.SetFloat("_BounceAmount", 1f);
        }
        effect.gameObject.SetActive(false);
        effect.gameObject.SetActive(true);
    }

    void SwitchTo(ScriptablePlant newPlant) {
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

    public void OnEventRaised(GameEvent e) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0f) {
            PhotonNetwork.Destroy(gameObject);
            return;
        }
        if (container.isFull) {
            container.Spill(container.volume);
            SwitchTo(plant.possibleNextGenerations[UnityEngine.Random.Range(0, plant.possibleNextGenerations.Length)]);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsReading) {
            SwitchTo(PlantDatabase.GetPlant((short)stream.ReceiveNext()));
        } else {
            stream.SendNext(PlantDatabase.GetID(plant));
        }
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is short) {
            SwitchTo(PlantDatabase.GetPlant((short)info.photonView.InstantiationData[0]));
        }
    }
}
