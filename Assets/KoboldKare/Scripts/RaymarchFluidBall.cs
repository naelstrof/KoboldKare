using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymarchFluidBall : MonoBehaviour {
    public ReagentContents contents = new ReagentContents();
    public float vps = 0.1f;
    public Material splatterMaterial;
    private RaymarchShape shape;
    private Rigidbody body;
    public void Start() {
        shape = GetComponent<RaymarchShape>();
        body = GetComponent<Rigidbody>();
    }
    private void HandleCollision(Collision collision) {
        ReagentContents spilled = contents.Spill(vps*Time.deltaTime);
        GenericReagentContainer container = collision.GetContact(0).otherCollider.GetComponentInParent<GenericReagentContainer>();
        if (container != null) {
            container.contents.Mix(spilled);
        }
        Vector3 norm = collision.contacts[0].normal;
        if (spilled.volume > 0f) {
            Color c = spilled.GetColor(ReagentDatabase.instance);
            c.a = 1f;
            GameManager.instance.SpawnDecalInWorld(splatterMaterial, collision.contacts[0].point + norm*0.25f, -norm, Vector2.one*2f, c, collision.contacts[0].otherCollider.gameObject, 0.5f, false, true, false);
        }
    }
    public void OnCollisionEnter(Collision c) {
        HandleCollision(c);
        body.velocity *= 0.8f;
    }

    public void OnCollisionStay(Collision c) {
        HandleCollision(c);
        body.velocity *= 0.8f;
    }
}
