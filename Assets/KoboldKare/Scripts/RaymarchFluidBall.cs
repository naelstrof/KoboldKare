using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RaymarchFluidBall : MonoBehaviour {
    public ReagentContents contents = new ReagentContents();
    public float vps = 0.1f;
    public Material splatterMaterial;
    public RaymarchFluid fluid;
    private AudioSource source;
    private RaymarchShape shape;
    private Rigidbody body;
    private float travelTime;
    private float lastTime;
    private float lastCloseTime;
    private float mixInterval = 0.05f;
    public Color emitColor;
    public void Start() {
        source = GetComponent<AudioSource>();
        shape = GetComponent<RaymarchShape>();
        body = GetComponent<Rigidbody>();
        travelTime = Time.timeSinceLevelLoad+1f;
        lastTime = Time.timeSinceLevelLoad;
        lastCloseTime = Time.timeSinceLevelLoad;
    }
    private void HandleCollision(Collision collision) {
        if (Time.timeSinceLevelLoad - lastTime > mixInterval) {
            ReagentContents spilled = null;
            GenericReagentContainer container = collision.GetContact(0).otherCollider.GetComponentInParent<GenericReagentContainer>();
            if (container != null && Time.timeSinceLevelLoad-travelTime > 0f) {
                spilled = contents.Spill(vps*mixInterval*3f);
                container.contents.Mix(spilled, ReagentContents.ReagentInjectType.Spray);
                if (container.contents.volume >= container.contents.maxVolume) {
                    fluid?.TriggerBadHit(collision.GetContact(0).point);
                } else {
                    fluid?.TriggerGoodHit(collision.GetContact(0).point);
                }
                lastTime = Time.timeSinceLevelLoad;
            } else {
                spilled = contents.Spill(vps*mixInterval);
                lastTime = Time.timeSinceLevelLoad;
            }
            if (spilled == null) {
                spilled = contents;
            }
            Vector3 norm = collision.contacts[0].normal;
            Color c = spilled.volume > 0.001f ? spilled.GetColor(ReagentDatabase.instance) : emitColor;
            c.a = 1f;
            if (spilled.ContainsKey(ReagentData.ID.Water) && spilled[ReagentData.ID.Water].volume > spilled.volume*0.9f) {
                GameManager.instance.SpawnDecalInWorld(splatterMaterial, collision.contacts[0].point + norm*0.25f, -norm, Vector2.one*Mathf.Max(spilled.volume*160f,0.6f), c, collision.contacts[0].otherCollider.gameObject, 0.5f, false, true, true);
            } else {
                GameManager.instance.SpawnDecalInWorld(splatterMaterial, collision.contacts[0].point + norm*0.25f, -norm, Vector2.one*Mathf.Max(spilled.volume*160f,0.6f), c, collision.contacts[0].otherCollider.gameObject, 0.5f, false, true, false);
            }
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
    public void OnTriggerStay(Collider c) {
        if (Time.timeSinceLevelLoad - lastCloseTime > mixInterval) {
            GenericReagentContainer container = c.GetComponentInParent<GenericReagentContainer>();
            if (container != null) {
                ReagentContents spilled = contents.Spill(vps*mixInterval*3f);
                container.contents.Mix(spilled, ReagentContents.ReagentInjectType.Spray);
                if (container.contents.volume >= container.contents.maxVolume) {
                    fluid?.TriggerBadHit(c.transform.position);
                } else {
                    fluid?.TriggerGoodHit(c.transform.position);
                }
                lastCloseTime = Time.timeSinceLevelLoad;
            }
        }
    }

}
