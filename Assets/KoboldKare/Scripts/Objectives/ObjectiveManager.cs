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
    private int stars = 0;

    public static int GetStars() {
        return instance.stars;
    }
    public static void GetMail() {
        instance.SwitchToObjective(instance.objectives[instance.currentObjectiveIndex]);
    }
    public delegate void ObjectiveChangedAction(DragonMailObjective newObjective);
    private event ObjectiveChangedAction objectiveChanged;
    private event ObjectiveChangedAction objectiveUpdated;

    public static void AddObjectiveSwappedListener(ObjectiveChangedAction action) {
        instance.objectiveChanged += action;
    }
    public static void RemoveObjectiveSwappedListener(ObjectiveChangedAction action) {
        instance.objectiveChanged -= action;
    }
    public static void AddObjectiveUpdatedListener(ObjectiveChangedAction action) {
        instance.objectiveUpdated += action;
    }
    public static void RemoveObjectiveUpdatedListener(ObjectiveChangedAction action) {
        instance.objectiveUpdated -= action;
    }

    public static DragonMailObjective GetCurrentObjective() {
        return instance.currentObjective;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    void Start() {
        SwitchToObjective(null);
    }

    void OnObjectiveComplete(DragonMailObjective objective) {
        currentObjectiveIndex++;
        if (objective.autoAdvance) {
            SwitchToObjective(objectives[currentObjectiveIndex]);
        } else {
            stars++;
            SwitchToObjective(null);
        }
    }

    void OnObjectiveUpdated(DragonMailObjective objective) {
        objectiveUpdated?.Invoke(objective);
    }

    private void SwitchToObjective(DragonMailObjective newObjective) {
        if (newObjective == currentObjective) {
            return;
        }
        if (currentObjective != null) {
            currentObjective.Unregister();
            currentObjective.completed -= OnObjectiveComplete;
            currentObjective.updated -= OnObjectiveUpdated;
        }
        currentObjective = newObjective;
        if (newObjective != null) {
            currentObjective.Register();
            currentObjective.completed += OnObjectiveComplete;
            currentObjective.updated += OnObjectiveUpdated;
        }
        objectiveChanged?.Invoke(currentObjective);
    }

    public void Save(BinaryWriter writer) {
        bool hasObjective = currentObjective != null;
        writer.Write(hasObjective);
        writer.Write(currentObjectiveIndex);
        objectives[currentObjectiveIndex].Save(writer);
    }

    public void Load(BinaryReader reader) {
        bool hasObjective = reader.ReadBoolean();
        currentObjectiveIndex = reader.ReadInt32();
        if (hasObjective) {
            SwitchToObjective(objectives[currentObjectiveIndex]);
        } else {
            SwitchToObjective(null);
        }
        objectives[currentObjectiveIndex].Load(reader);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            bool hasObjective = currentObjective != null;
            stream.SendNext(stars);
            stream.SendNext(hasObjective);
            stream.SendNext(currentObjectiveIndex);
        } else {
            stars = (int)stream.ReceiveNext();
            bool hasObjective = (bool)stream.ReceiveNext();
            currentObjectiveIndex = (int)stream.ReceiveNext();
            if (hasObjective) {
                SwitchToObjective(objectives[currentObjectiveIndex]);
            } else {
                SwitchToObjective(null);
            }
        }
        objectives[currentObjectiveIndex].OnPhotonSerializeView(stream, info);
    }

    private void OnValidate() {
        foreach (var obj in objectives) {
            obj.OnValidate();
        }
    }
}
