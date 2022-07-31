using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class ObjectiveManager : MonoBehaviourPun, ISavable, IPunObservable {
    private static ObjectiveManager instance;
    [SerializeReference, SerializeReferenceButton]
    [SerializeField] private List<DragonMailObjective> objectives;
    private DragonMailObjective currentObjective;
    private int currentObjectiveIndex;

    public delegate void ObjectiveChangedAction(DragonMailObjective newObjective);
    private event ObjectiveChangedAction objectiveChanged;
    
    private event ObjectiveChangedAction objectiveUpdated;

    public static void AddObjectiveSwappedListener(ObjectiveChangedAction action) {
        instance.objectiveChanged += action;
    }
    public static void RemoveObjectiveChangeListener(ObjectiveChangedAction action) {
        instance.objectiveChanged -= action;
    }
    
    public static void AddObjectiveUpdatedListener(ObjectiveChangedAction action) {
        instance.objectiveUpdated += action;
    }
    public static void RemoveObjectiveUpdatedListener(ObjectiveChangedAction action) {
        instance.objectiveUpdated -= action;
    }

    public static DragonMailObjective GetCurrentObjective() {
        return instance.objectives[instance.currentObjectiveIndex];
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    void Start() {
        SwitchToObjective(objectives[currentObjectiveIndex]);
    }

    void OnObjectiveComplete(DragonMailObjective objective) {
        SwitchToObjective(objectives[++currentObjectiveIndex]);
    }

    void OnObjectiveUpdated(DragonMailObjective objective) {
        objectiveUpdated?.Invoke(objective);
    }

    private void SwitchToObjective(DragonMailObjective newObjective) {
        if (currentObjective != null) {
            currentObjective.Unregister();
            currentObjective.completed -= OnObjectiveComplete;
            currentObjective.updated -= OnObjectiveUpdated;
        }
        currentObjective = newObjective;
        currentObjective.Register();
        currentObjective.completed += OnObjectiveComplete;
        currentObjective.updated += OnObjectiveUpdated;
        objectiveChanged?.Invoke(currentObjective);
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(currentObjectiveIndex);
        objectives[currentObjectiveIndex].Save(writer, version);
    }

    public void Load(BinaryReader reader, string version) {
        currentObjectiveIndex = reader.ReadInt32();
        SwitchToObjective(objectives[currentObjectiveIndex]);
        objectives[currentObjectiveIndex].Load(reader, version);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(currentObjectiveIndex);
        } else {
            currentObjectiveIndex = (int)stream.ReceiveNext();
        }
        
        objectives[currentObjectiveIndex].OnPhotonSerializeView(stream, info);
    }
}
