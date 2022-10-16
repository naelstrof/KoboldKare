using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ObjectiveManager : MonoBehaviourPun, ISavable, IPunObservable, IOnPhotonViewOwnerChange {
    private static ObjectiveManager instance;
    [SerializeReference, SerializeReferenceButton]
    [SerializeField] private List<DragonMailObjective> objectives;
    private DragonMailObjective currentObjective;
    private int currentObjectiveIndex;
    private int stars = 0;

    public static int GetStars() {
        return instance.stars;
    }

    public static void GiveStars(int count) {
        instance.stars += count;
        instance.objectiveChanged?.Invoke(null);
    }

    public static bool HasMail() {
        return instance.currentObjective == null && instance.currentObjectiveIndex < instance.objectives.Count;
    }
    public static void GetMail() {
        instance.SwitchToObjective(instance.currentObjectiveIndex >= instance.objectives.Count ? null : instance.objectives[instance.currentObjectiveIndex]);
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

    public static void SkipObjective() {
        instance.OnObjectiveComplete(GetCurrentObjective());
        instance.SwitchToObjective(instance.currentObjectiveIndex >= instance.objectives.Count ? null : instance.objectives[instance.currentObjectiveIndex]);
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
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void OnLocaleChanged(Locale locale) {
        objectiveUpdated?.Invoke(currentObjective);
    }

    void OnObjectiveComplete(DragonMailObjective objective) {
        currentObjectiveIndex++;
        if (objective is { autoAdvance: true }) {
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
            if (photonView.IsMine) {
                currentObjective.Register();
            }
            currentObjective.completed += OnObjectiveComplete;
            currentObjective.updated += OnObjectiveUpdated;
        }
        objectiveChanged?.Invoke(currentObjective);
    }

    public void Save(BinaryWriter writer) {
        writer.Write(stars);
        bool hasObjective = currentObjective != null;
        writer.Write(hasObjective);
        writer.Write(currentObjectiveIndex);
        if (hasObjective) {
            objectives[currentObjectiveIndex].Save(writer);
        }
    }

    public void Load(BinaryReader reader) {
        stars = reader.ReadInt32();
        bool hasObjective = reader.ReadBoolean();
        currentObjectiveIndex = reader.ReadInt32();
        SwitchToObjective(hasObjective ? objectives[currentObjectiveIndex] : null);
        if (hasObjective) {
            objectives[currentObjectiveIndex].Load(reader);
        }
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
            SwitchToObjective(hasObjective ? GetCurrentObjective() : null);
        }
        objectives[currentObjectiveIndex].OnPhotonSerializeView(stream, info);
    }

    private void OnValidate() {
        foreach (var obj in objectives) {
            obj?.OnValidate();
        }
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (currentObjective != null) {
            SwitchToObjective(currentObjective);
        }
    }
}
