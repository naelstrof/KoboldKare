using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalBleeder : MonoBehaviour
{
    //public List<GameObject> _decals;
    public Material decalMat;
    public LayerMask _hitLayers;
    private Vector3 _lastPos = Vector3.zero;
    //private float _minDistance = 0.1f;
    private void OnCollisionEnter(Collision collision) {
        if ( (1<<collision.collider.gameObject.layer & _hitLayers) == 0 ) {
            return;
        }
        Vector3 normal = Vector3.zero;
        Vector3 point = Vector3.zero;
        foreach (ContactPoint p in collision.contacts) {
            normal += p.normal;
            point += p.point;
        }
        normal /= collision.contacts.Length;
        point /= collision.contacts.Length;

        decalMat.color = Color.red;
        SkinnedMeshDecals.PaintDecal.RenderDecalForCollision(collision.collider, decalMat, point, normal, UnityEngine.Random.Range(0f,360f), Vector2.one*3f);
        //if (Vector3.Distance(_lastPos, point) > _minDistance) {
            //GameObject.Instantiate(_decals[Random.Range(0, _decals.Count - 1)], point, Quaternion.LookRotation(normal));
            //_lastPos = point;
        //}
    }
}
