using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereFluidDisplay : MonoBehaviour {
    public GenericReagentContainer container;
    public Rigidbody body;
    [Range(0f,100f)]
    public float spring = 50f;
    [Range(0f, 10f)]
    public float damping = 0f;
    public Renderer fluidRenderer;
    private Vector3 vel;
    private Vector3 pos;
    private NetworkedEntity networkedEntity;
    void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        vel = Vector3.zero;
        pos = Vector3.up;
        if (networkedEntity) {
            networkedEntity.reagentContents.OnChange += OnChanged;
            OnChanged(networkedEntity.GetContents(), networkedEntity.GetContents(), true);
        }
    }
    void OnDestroy() {
        if (networkedEntity) {
            networkedEntity.reagentContents.OnChange -= OnChanged;
        }
    }
    void FixedUpdate() {
        Vector3 normal = body.velocity - Physics.gravity;
        Vector3 wantedNormal = fluidRenderer.transform.InverseTransformDirection(Vector3.Normalize(normal));
        vel += ((wantedNormal - pos) * spring - (vel * damping)) * Time.fixedDeltaTime;
        pos = Vector3.Normalize(pos + vel * Time.fixedDeltaTime);
        fluidRenderer.material.SetVector("_PlaneNormal", pos);
    }
    void OnChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        fluidRenderer.material.SetColor("_Color", contents.GetColor());
        fluidRenderer.material.SetFloat("_Position", contents.volume / contents.GetMaxVolume());
    }
}
