using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviourPun, IPunObservable, ISavable, IPunInstantiateMagicCallback {
    private Vector3 velocity;
    [SerializeField]
    private GameObject splash;
    private GenericReagentContainer splashContainer;
    private static Collider[] colliders = new Collider[32];
    private static HashSet<GenericReagentContainer> hitContainers = new HashSet<GenericReagentContainer>();
    void Update() {
        velocity += Physics.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = quaternion.LookRotation(velocity, Vector3.up);
    }
    private void OnTriggerEnter(Collider other) {
        hitContainers.Clear();
        GameObject.Instantiate(splash, transform.position, Quaternion.identity);
        int hits = Physics.OverlapSphereNonAlloc(transform.position, 1f, colliders, GameManager.instance.waterSprayHitMask);
        for (int i = 0; i < hits; i++) {
            hitContainers.Add(colliders[i].GetComponentInParent<GenericReagentContainer>());
        }

        float perVolume = splashContainer.volume / hitContainers.Count;
        foreach (GenericReagentContainer container in hitContainers) {
            container.TransferMix(splashContainer, perVolume, GenericReagentContainer.InjectType.Spray);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(velocity);
        } else {
            velocity = (Vector3)stream.ReceiveNext();
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(velocity.x);
        writer.Write(velocity.y);
        writer.Write(velocity.z);
    }

    public void Load(BinaryReader reader, string version) {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        velocity = new Vector3(x, y, z);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        splashContainer.AddMix((ReagentContents)info.photonView.InstantiationData[0], GenericReagentContainer.InjectType.Inject);
        velocity = (Vector3)info.photonView.InstantiationData[1];
    }
}
