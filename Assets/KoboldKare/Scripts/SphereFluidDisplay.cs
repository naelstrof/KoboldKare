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
    void Start() {
        vel = Vector3.zero;
        pos = Vector3.up;
        container.OnChange.AddListener(OnChanged);
        OnChanged(GenericReagentContainer.InjectType.Inject);
    }
    void OnDestroy() {
        container.OnChange.RemoveListener(OnChanged);
    }
    void FixedUpdate() {
        Vector3 normal = body.velocity - Physics.gravity;
        Vector3 wantedNormal = fluidRenderer.transform.InverseTransformDirection(Vector3.Normalize(normal));
        vel += ((wantedNormal - pos) * spring - (vel * damping)) * Time.fixedDeltaTime;
        pos = Vector3.Normalize(pos + vel * Time.fixedDeltaTime);
        fluidRenderer.material.SetVector("_PlaneNormal", pos);
    }
    void OnChanged(GenericReagentContainer.InjectType injectType) {
        fluidRenderer.material.SetColor("_Color", container.GetColor());
        fluidRenderer.material.SetFloat("_Position", container.volume / container.maxVolume);
    }
}
