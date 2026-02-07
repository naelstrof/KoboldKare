using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFluidDisplay : MonoBehaviour {
    public GenericReagentContainer container;
    public Renderer targetRenderer;
    public Transform targetTransform;
    public Vector3 scaleDirection = Vector3.up;
    private NetworkedEntity networkedEntity;
    public void Start() {
        scaleDirection = new Vector3(Mathf.Abs(scaleDirection.x), Mathf.Abs(scaleDirection.y), Mathf.Abs(scaleDirection.z));
        
        networkedEntity = container.GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange += OnChanged;
            OnChanged(networkedEntity.GetContents(), networkedEntity.GetContents(), false);
        }
    }
    public void OnDestroy() {
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange -= OnChanged;
        }
    }
    private void OnChanged(ReagentContents prev, ReagentContents next, bool asServer) {
        foreach(var m in targetRenderer.materials) {
            m.color = next.GetColor();
        }
        targetTransform.localScale = (Vector3.one - scaleDirection) + (scaleDirection * (next.volume/next.GetMaxVolume()));
    }
}
