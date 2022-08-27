using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityScriptableSettings {
[CreateAssetMenu(fileName = "DecalQuality", menuName = "Unity Scriptable Setting/KoboldKare/Decal Quality Setting", order = 1)]
public class DecalQualitySetting : ScriptableSettingSlider {
    public override void SetValue(float val) {
        SkinnedMeshDecals.PaintDecal.SetMemoryBudgetMB(val.Remap(0f,1f,Mathf.Min(64f,SystemInfo.graphicsMemorySize*0.025f),Mathf.Min(2048f,SystemInfo.graphicsMemorySize*0.20f)));
        SkinnedMeshDecals.PaintDecal.SetTexelsPerMeter(val.Remap(0f,1f,32f,512f));
        SkinnedMeshDecals.PaintDecal.SetDilation(val > 0.5f);
        base.SetValue(val);
    }
}

}