using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityScriptableSettings {
[CreateAssetMenu(fileName = "DecalQuality", menuName = "Unity Scriptable Setting/KoboldKare/Decal Quality Setting", order = 1)]
public class DecalQualitySetting : ScriptableSettingSlider {
    public override void SetValue(float value) {
        SkinnedMeshDecals.PaintDecal.SetMemoryBudgetMB(value.Remap(0f,1f,512f,2048f));
        //SkinnedMeshDecals.PaintDecal.texelsPerMeter = Mathf.RoundToInt(value.Remap(0f,1f,64f,2048f));
        base.SetValue(value);
    }
}

}