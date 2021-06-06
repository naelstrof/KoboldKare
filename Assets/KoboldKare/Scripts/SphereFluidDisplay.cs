using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereFluidDisplay : MonoBehaviour, IReagentContainerListener {
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
        container.contents.AddListener(this);
        OnChanged();
    }
    void OnDestroy() {
        container.contents.RemoveListener(this);
    }
    void FixedUpdate() {
        Vector3 normal = body.velocity - Physics.gravity;
        Vector3 wantedNormal = fluidRenderer.transform.InverseTransformDirection(Vector3.Normalize(normal));
        vel += ((wantedNormal - pos) * spring - (vel * damping)) * Time.fixedDeltaTime;
        pos = Vector3.Normalize(pos + vel * Time.fixedDeltaTime);
        fluidRenderer.material.SetVector("_PlaneNormal", pos);
    }
    void OnChanged() {
        if ( container.contents.volume <= 0 ) {
            fluidRenderer.material.SetColor("_Color", new Color(0,0,0,0));
            fluidRenderer.material.SetFloat("_Position", 0);
            return;
        }
        fluidRenderer.material.SetColor("_Color", container.contents.GetColor(GameManager.instance.reagentDatabase));
        fluidRenderer.material.SetFloat("_Position", container.contents.volume / container.contents.maxVolume);
    }

    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType injectType) {
        OnChanged();
    }
}
