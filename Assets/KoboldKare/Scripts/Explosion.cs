using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviourPun {
    [SerializeField]
    private LayerMask playerMask;
    [SerializeField]
    private Material scorchDecal;
    void Start() {
        SkinnedMeshDecals.PaintDecal.RenderDecalInBox(Vector3.one*4f, transform.position, scorchDecal, Quaternion.FromToRotation(Vector3.forward, Vector3.down), GameManager.instance.decalHitMask);
        if (!photonView.IsMine) {
            return;
        }

        List<Kobold> kobolds = new List<Kobold>();
        SoilTile bestTile = null;
        float bestTileDistance = float.MaxValue;
        foreach( Collider c in Physics.OverlapSphere(transform.position, 5f, playerMask, QueryTriggerInteraction.Ignore)) {
            scorchDecal.color = Color.black;
            Kobold k = c.GetComponentInParent<Kobold>();
            if (k != null && !kobolds.Contains(k)) {
                kobolds.Add(k);
                foreach (Rigidbody r in k.GetRagdoller().GetRagdollBodies()) {
                    r.AddExplosionForce(3000f, transform.position, 5f);
                }
                k.body.AddExplosionForce(3000f, transform.position, 5f);
                k.StartCoroutine(k.ThrowRoutine());
            } else {
                Rigidbody r = c.GetComponentInParent<Rigidbody>();
                r?.AddExplosionForce(3000f, transform.position, 5f);
            }

            SoilTile tile = c.GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetDebris()) {
                float distance = Vector3.Distance(transform.position, tile.transform.position);
                if (distance < bestTileDistance) {
                    bestTile = tile;
                    bestTileDistance = distance;
                }
            }

            IDamagable damagable = c.GetComponentInParent<IDamagable>();
            // Bombs hurt!!
            if (damagable != null) {
                float dist = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
                float damage = Mathf.Clamp01((5f - dist) / 5f) * 250f;
                //linear falloff because :shrug:
                damagable.photonView.RPC(nameof(IDamagable.Damage), RpcTarget.All, damage);
            }
        }

        if (bestTile != null) {
            bestTile.photonView.RPC(nameof(SoilTile.SetDebris),RpcTarget.All,false);
        }
    }
}
