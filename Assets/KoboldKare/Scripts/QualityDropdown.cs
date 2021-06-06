using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;
using TMPro;
/*
public class QualityDropdown : MonoBehaviour {
    public enum GraphicsType {
        Anistropic,
        LitShaderMode,
    }
    public GraphicsType type;
    public TMP_Dropdown dropdown;
    private void GetData <T>(List<TMP_Dropdown.OptionData> data) {
        foreach (T t in (T[])Enum.GetValues(typeof(T))) {
            data.Add(new TMP_Dropdown.OptionData(t.ToString()));
        }
    }
    // Start is called before the first frame update
    private void Start() {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
        switch (type) {
            case GraphicsType.Anistropic: {
                GetData<AnisotropicFiltering>(data);
                dropdown.AddOptions(data);
                dropdown.value = (int)QualitySettings.anisotropicFiltering;
                break;
            }
            case GraphicsType.LitShaderMode: {
                GetData<LitShaderMode>(data);
                dropdown.AddOptions(data);
                dropdown.value = (int)GameManager.instance.frameSettings.litShaderMode;
                break;
            }
        }
    }

    public void change(System.Int32 i) {
        switch (type) {
            case GraphicsType.Anistropic: QualitySettings.anisotropicFiltering = (AnisotropicFiltering)i; break;
            case GraphicsType.LitShaderMode: {
                GameManager.instance.frameSettingsMask.mask[(uint)FrameSettingsField.LitShaderMode] = true;
                GameManager.instance.frameSettings.litShaderMode = (LitShaderMode)i;
                GameManager.instance.updateFrameSettings();
                break;
            }
        }
    }
}*/
