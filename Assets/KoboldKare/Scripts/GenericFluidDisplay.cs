using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFluidDisplay : MonoBehaviour, IReagentContainerListener {
    public GenericReagentContainer container;
    public Renderer targetRenderer;
    public Transform targetTransform;
    public Vector3 scaleDirection = Vector3.up;
    public void Start() {
        scaleDirection = new Vector3(Mathf.Abs(scaleDirection.x), Mathf.Abs(scaleDirection.y), Mathf.Abs(scaleDirection.z));
        container.contents.AddListener(this);
        OnReagentContainerChanged(container.contents, ReagentContents.ReagentInjectType.Inject);
    }
    public void OnDestroy() {
        container.contents?.RemoveListener(this);
    }
    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType type) {
        foreach(var m in targetRenderer.materials) {
            m.color = contents.GetColor(ReagentDatabase.instance);
        }
        targetTransform.localScale = (Vector3.one - scaleDirection) + (scaleDirection * (contents.volume/contents.maxVolume));
        //foreach(var pair in contents) {
            //Debug.Log(pair.Key + ": " + pair.Value.volume + ", ");
        //}
    }
}
