using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ReagentCallbacks", menuName = "Data/Reagent Callbacks", order = 1)]
public class ReagentSerializableCallbacks : ScriptableObject {
    //private class ReagentProcessPair {
    [NonSerialized]
    private Dictionary<GameObject, List<Task>> reagentProcesses = new Dictionary<GameObject, List<Task>>();
    //[NonSerialized]
    //private HashSet
    public GameObject explosionPrefab;
    public AudioClip sizzleSound;
    public Material scorchDecal;
    public LayerMask playerMask;
    public AudioClip reagentReactionSound;
    private void AddProcess(GameObject obj, Task t, string name) {
        if (!reagentProcesses.ContainsKey(obj)) {
            reagentProcesses.Add(obj, new List<Task>());
        }
        t.name = name;
        reagentProcesses[obj].Add(t);
        // Make sure we clean ourselves up.
        t.Finished += (manual) => ClearProcess(obj, t);
    }
    private bool HasProcess(GameObject obj, string name) {
        if (!reagentProcesses.ContainsKey(obj)) {
            return false;
        }
        for (int i = 0; i < reagentProcesses[obj].Count; i++) {
            if (reagentProcesses[obj][i].name == name && reagentProcesses[obj][i].Running) {
                return true;
            }
        }
        return false;
    }
    private void ClearProcess(GameObject obj, Task t) {
        if (!reagentProcesses.ContainsKey(obj)) {
            return;
        }
        reagentProcesses[obj].Remove(t);
        if (reagentProcesses[obj].Count == 0) {
            reagentProcesses.Remove(obj);
        }
    }
    private void ClearProcesses(GameObject obj, string name) {
        if (!reagentProcesses.ContainsKey(obj)) {
            return;
        }
        for (int i=0;i<reagentProcesses[obj].Count;i++) {
            if (reagentProcesses[obj][i].name == name) {
                reagentProcesses[obj][i].Stop();
                reagentProcesses[obj].RemoveAt(i);
            }
        }
    }

    public IEnumerator ReagentReactionSound(GameObject obj) {
        if (obj == null) {
            yield break;
        }
        Transform targetTransform = obj.transform;
        IGrabbable grabbable = obj.GetComponent<IGrabbable>();
        if (grabbable != null) {
            targetTransform = grabbable.GrabTransform(grabbable.GetRigidBodies()[0]);
        }
        GameManager.instance.SpawnAudioClipInWorld(reagentReactionSound, targetTransform.position, 1f);
        yield return new WaitForSeconds(1f);
        ClearProcesses(obj, "Bubbles");
    }

    public IEnumerator SizzleThenExplode(float delay, GenericReagentContainer container) {
        Transform targetTransform = container.transform;
        IGrabbable grabbable = container.GetComponent<IGrabbable>();
        if (grabbable != null) {
            targetTransform = grabbable.GrabTransform(grabbable.GetRigidBodies()[0]);
        }
        GameManager.instance.SpawnAudioClipInWorld(sizzleSound, targetTransform.position, 1.1f, GameManager.instance.soundEffectLoudGroup);
        Vector3 backupPosition = targetTransform.position;
        // We periodically grab a backup spot, just in case the prop gets removed over the network right before the explosion.
        for (int i = 0; i < 4; i++) {
            yield return new WaitForSeconds(delay / 4);
            if (targetTransform != null) {
                backupPosition = targetTransform.position;
            }
        }
        GameObject.Instantiate(explosionPrefab, backupPosition, Quaternion.identity);
        HashSet<Kobold> foundKobolds = new HashSet<Kobold>();
        SkinnedMeshDecals.PaintDecal.RenderDecalInBox(Vector3.one*4f, backupPosition, scorchDecal, Quaternion.FromToRotation(Vector3.forward, Vector3.down), GameManager.instance.decalHitMask);
        
        SoilTile bestTile = null;
        float bestTileDistance = float.MaxValue;
        
        foreach( Collider c in Physics.OverlapSphere(backupPosition, 5f, playerMask, QueryTriggerInteraction.Ignore)) {
            scorchDecal.color = Color.black;
            Kobold k = c.GetComponentInParent<Kobold>();
            if (k != null && !foundKobolds.Contains(k)) {
                foundKobolds.Add(k);
                foreach (Rigidbody r in k.ragdoller.GetRagdollBodies()) {
                    r.AddExplosionForce(3000f, backupPosition, 5f);
                }
                k.body.AddExplosionForce(3000f, backupPosition, 5f);
                k.StartCoroutine(k.ThrowRoutine());
            } else {
                Rigidbody r = c.GetComponentInParent<Rigidbody>();
                r?.AddExplosionForce(3000f, backupPosition, 5f);
            }

            SoilTile tile = c.GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetDebris()) {
                float distance = Vector3.Distance(backupPosition, tile.transform.position);
                if (distance < bestTileDistance) {
                    bestTile = tile;
                    bestTileDistance = distance;
                }
            }

            IDamagable damagable = c.GetComponentInParent<IDamagable>();
            // Bombs hurt!!
            if (damagable != null) {
                float dist = Vector3.Distance(backupPosition, c.ClosestPoint(backupPosition));
                float damage = Mathf.Clamp01((5f - dist) / 5f) * 250f;
                //linear falloff because :shrug:
                damagable.Damage(damage);
            }
        }

        if (bestTile != null) {
            bestTile.photonView.RPC(nameof(SoilTile.SetDebris),RpcTarget.All,false);
        }

        // Remove all explosium
        if (targetTransform != null) {
            container.Spill(container.volume);
        }

        ClearProcesses(container.gameObject, "Explosion");
    }

    // To prevent rapid execution, we use coroutines to wait until the user is done mixing things before it decides to blow.
    public void SpawnExplosion(GenericReagentContainer container) {
        if (!HasProcess(container.gameObject, "Explosion")) {
            AddProcess(container.gameObject, new Task(SizzleThenExplode(4f, container)), "Explosion");
        }
    }
    public void BubbleSound(GenericReagentContainer container) {
        GameObject obj = container.gameObject;
        if (obj == null) {
            return;
        }
        if (!HasProcess(obj, "Bubbles")) {
            AddProcess(obj, new Task(ReagentReactionSound(obj)), "Bubbles");
        }
    }
    public void PrintSomething(string thing) {
        Debug.Log("thing");
    }
    public void DestroyThing(UnityEngine.Object g) {
        if (g is GameObject) {
            PhotonView other = ((GameObject)g).GetComponentInParent<PhotonView>();
            if (other != null && other.IsMine) {
                PhotonNetwork.Destroy(other.gameObject);
                return;
            }
            if (other == null) {
                Destroy(g);
            }
        } else {
            Destroy(g);
        }
    }
}
