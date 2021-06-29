using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using System.IO;
using System.Text;
using SimpleJSON;
using System;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public interface IGameEventOptionListener {
    void OnEventRaised(GraphicsOptions.OptionType e, float value);
}
[CreateAssetMenu(fileName = "GraphicsOptions", menuName = "Data/Graphics Options", order = 1)]
public class GraphicsOptions : SingletonScriptableObject<GraphicsOptions> {
    public LocalizedString resolutionReference;
    public List<ProceduralMaterialInfo> proceduralTextures = new List<ProceduralMaterialInfo>();
    public List<Option> options = new List<Option>();
    public ForwardRendererData renderer;
    public AudioMixer mixer;
    public VolumetricClouds cloudsSettings;
    [System.Serializable]
    public class ProceduralMaterialInfo {
        //[SerializeField]
        //public Substance.Game.SubstanceGraph graph;
        [SerializeField]
        public int bias = 0;
        [SerializeField]
        public int min = 7;
        [SerializeField]
        public int max = 10;
    }
    [NonSerialized]
    public float textureLoadingProgress = 0f;
    [NonSerialized]
    public string textureLoadingName = "";
    [System.Serializable]
    public class Option {
        public OptionType type;
        public OptionGroup group;
        public Option() {}
        public Option(float def, params string[] dropDownOptions ) {
            dropDownDescriptions = new List<string>(dropDownOptions);
            value = def;
        }
        public Option(float def, float min, float max) {
            value = def;
            this.min = min;
            this.max = max;
        }
        public LocalizedString name;
        public LocalizedString description;
        public List<string> dropDownDescriptions = new List<string>();
        public List<LocalizedString> localizedDropDownDescriptions = new List<LocalizedString>();
        public float value;
        public float min;
        public float max;
        public float defaultValue;
    }
    [System.Serializable]
    public enum OptionGroup : int {
        Graphics = 0,
        Audio,
        Gameplay,
        Special,
        Multiplayer,
        Controls,
    }
    public enum OptionType {
        DecalQuality = 0,
        WindowMode,
        Resolution,
        Bloom,
        MotionBlur,
        AmbientOcclusion,
        Shadows,
        Grass,
        ScreenSpaceReflections,
        SubsurfaceScattering,
        AntiAliasing,
        VolumetricLighting,
        MaterialQuality,
        FilmGrain,
        VideoMemoryUsage,
        MasterVolume,
        SoundEffectsVolume,
        MusicVolume,
        CameraFOV,
        PaniniProjection,
        DepthOfField,
        ProceduralTextureSize,
        Clouds,
        OverallQuality,
        ToggleWalk,
        InvertWalk,
        Language,
        Sex,
        TopBottom,
        Thickness,
        Hue,
        Brightness,
        Saturation,
        Contrast,
        Dick,
        DickSize,
        BoobSize,
        InOut,
        KoboldSize,
        BallsSize,
        Chubbiness,
        MirrorQuality,
        VSync,
        TargetFramerate,
        MouseSensitivity,
    }
    [System.Serializable]
    public class OptionChange {
        [SerializeField]
        public OptionType target;
        [SerializeField]
        public int value;
    }
    public IEnumerator WaitForLocalizationToBeReadyThenSet( int index ) {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[Mathf.Min(index, LocalizationSettings.AvailableLocales.Locales.Count-1)];
    }
    public IEnumerator WaitAndThenSetTextureSize(float value){
        // Substance might be totally broken and unstable, can't use for now.
        /*Raise(OptionType.ProceduralTextureSize, value);
        int i = 0;
        foreach( ProceduralMaterialInfo info in proceduralTextures) {
            int specificLevel = Mathf.RoundToInt(Mathf.Clamp(value+(float)info.bias, info.min, info.max));
            info.graph.SetInputVector2Int("$outputsize", specificLevel, specificLevel);
            info.graph.QueueForRender();
            textureLoadingName = info.graph.name;
            Substance.Game.Substance.RenderSubstancesAsync();
            yield return new WaitForEndOfFrame();
            while(Substance.Game.Substance.IsProcessing()) {
                yield return new WaitForEndOfFrame();
            }
            textureLoadingProgress = ((float)(i++)/(float)proceduralTextures.Count);
        }*/
        Raise(OptionType.ProceduralTextureSize, value);
        textureLoadingProgress = 0f;
        yield return new WaitForEndOfFrame();
        textureLoadingProgress = 1f;
        Raise(OptionType.ProceduralTextureSize, value);
    }
    public void Save() {
        //string savePath = Application.persistentDataPath + "/graphicsSettings.json";
        //FileStream file = File.Create(savePath);
        //JSONNode n = JSON.Parse("{}");
        //foreach(Option o in options) {
            //if (o.type == OptionType.Resolution) {
                //n["ScreenWidth"] = resolution.width;
                //n["ScreenHeight"] = resolution.height;
                //n["RefreshRate"] = resolution.refreshRate;
                //continue;
            //}
            //n[o.type.ToString()] = o.value;
        //}
        //file.Write(Encoding.UTF8.GetBytes(n.ToString(2)),0,n.ToString(2).Length);
        //file.Close();
        //Debug.Log("Saved graphics settings to " + savePath);
        string savePath = Application.persistentDataPath + "/playerprefs.json";
        FileStream file = File.Create(savePath);
        //string json = JsonUtility.ToJson(overrides, true);
        JSONNode n = JSON.Parse("{}");

        foreach(Option o in options) {
            if (o.type == OptionType.Resolution) {
                PlayerPrefs.SetInt ("Screenmanager Resolution Height", resolution.height);
                PlayerPrefs.SetInt ("Screenmanager Resolution Width", resolution.width);
                PlayerPrefs.SetInt ("Screenmanager Refresh Rate", resolution.refreshRate);
                continue;
            }
            if (o.type == OptionType.WindowMode) {
                PlayerPrefs.SetInt ("Screenmanager Fullscreen mode", (int)fullscreenMode);
                continue;
            }
            if (o.type == OptionType.OverallQuality) {
                PlayerPrefs.SetInt("UnityGraphicsQuality", (int)o.value);
                continue;
            }
            //PlayerPrefs.SetFloat(o.group.ToString() + " Options " + o.type.ToString(), o.value);
            n[o.group.ToString() + " Options " + o.type.ToString()] = o.value;
        }
        file.Write(Encoding.UTF8.GetBytes(n.ToString(2)),0,n.ToString(2).Length);
        file.Close();
        Debug.Log("Saved player preferences to " + savePath);
        PlayerPrefs.Save();
    }
    public bool Load() {
        try {
            string savePath = Application.persistentDataPath + "/playerprefs.json";
            FileStream file = File.Open(savePath, FileMode.Open);
            byte[] b = new byte[file.Length];
            file.Read(b,0,(int)file.Length);
            file.Close();

            string data = Encoding.UTF8.GetString(b);
            JSONNode n = JSON.Parse(data);
            foreach(Option o in options) {
                if (o.type == OptionType.Resolution && PlayerPrefs.HasKey("Screenmanager Resolution Height") && PlayerPrefs.HasKey("Screenmanager Resolution Width") && PlayerPrefs.HasKey("Screenmanager Refresh Rate")) {
                    Resolution r = new Resolution();
                    r.height = PlayerPrefs.GetInt ("Screenmanager Resolution Height");
                    r.width = PlayerPrefs.GetInt ("Screenmanager Resolution Width");
                    r.refreshRate = PlayerPrefs.GetInt ("Screenmanager Refresh Rate");
                    if (r.height <= 128 || r.width <= 128 || r.refreshRate <= 1) {
                        r = Screen.resolutions[Screen.resolutions.Length-1];
                    }
                    resolution = r;
                    for(int i=0;i<Screen.resolutions.Length;i++) {
                        Resolution res = Screen.resolutions[i];
                        if (r.width == res.width && r.height == res.height && r.refreshRate == res.refreshRate) {
                            resolution = Screen.resolutions[i];
                            o.value = i;
                            break;
                        }
                    }
                    continue;
                }
                if (o.type == OptionType.WindowMode && PlayerPrefs.HasKey("Screenmanager Fullscreen mode")) {
                    fullscreenMode = (FullScreenMode)Mathf.FloorToInt(PlayerPrefs.GetInt("Screenmanager Fullscreen mode"));
                    continue;
                }
                if (o.type == OptionType.OverallQuality) {
                    ChangeOption(o.type, PlayerPrefs.GetInt("UnityGraphicsQuality"));
                    continue;
                }
                if (n.HasKey(o.group.ToString() + " Options " + o.type.ToString())) {
                    float value = n[o.group.ToString() + " Options " + o.type.ToString()];
                    ChangeOption(o.type, Mathf.Clamp(value, o.min, o.max));
                }
                //if ( PlayerPrefs.HasKey(o.group.ToString() + " Options " + o.type.ToString())) {
                    //ChangeOption(o.type, PlayerPrefs.GetFloat(o.group.ToString() + " Options " + o.type.ToString()));
                //}
            }
            Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
            return true;
        } catch (Exception e) {
            if (e is FileNotFoundException) {
                return false;
            }
            Debug.LogException(e);
            return false;
        }
    }
    [System.NonSerialized]
    public FullScreenMode fullscreenMode;
    [System.NonSerialized]
    public Resolution resolution;
    [NonSerialized]
    private List<IGameEventOptionListener> listeners = new List<IGameEventOptionListener>();
    [NonSerialized]
    private List<IGameEventOptionListener> savedListeners = new List<IGameEventOptionListener>();
    [NonSerialized]
    private List<IGameEventOptionListener> savedRemovedListeners = new List<IGameEventOptionListener>();
    [NonSerialized]
    private bool running = false;

    public void RegisterListener(IGameEventOptionListener listener) {
        if (running) {
            savedListeners.Add(listener);
        } else {
            listeners.Add(listener);
        }
    }
    public void UnregisterListener(IGameEventOptionListener listener) {
        if (running) {
            savedRemovedListeners.Add(listener);
        } else {
            listeners.Remove(listener);
        }
    }

    public void OnDestroy() {
        if (renderer.rendererFeatures.Contains(cloudsSettings)) {
            renderer.rendererFeatures.Remove(cloudsSettings);
        }
    }
    public void ResetToDefaults() {
        foreach(Option o in options) {
            if (o.type == OptionType.Resolution) {
                continue;
            }
            if (o.type == OptionType.WindowMode) {
                continue;
            }
            ChangeOption(o.type, o.defaultValue);
        }
    }

    public void ResetToDefaults( OptionGroup group ) {
        foreach(Option o in options) {
            if (o.group != group) {
                continue;
            }
            if (o.type == OptionType.Resolution) {
                continue;
            }
            if (o.type == OptionType.WindowMode) {
                continue;
            }
            ChangeOption(o.type, o.defaultValue);
        }
    }
    public void ResetToDefaults( int group ) {
        ResetToDefaults((OptionGroup)group);
    }
    public void Raise(OptionType target, float value) {
        if (running) {
            return;
        }
        listeners.AddRange(savedListeners);
        savedListeners.Clear();

        running = true;
        int size = listeners.Count;
        foreach (IGameEventOptionListener l in listeners) {
            l.OnEventRaised(target, value);
        }
        running = false;

        foreach (IGameEventOptionListener l in savedRemovedListeners) {
            listeners.Remove(l);
        }
        savedRemovedListeners.Clear();
    }
    /*public void Apply(Camera cam) {
        if (cam == null || cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null) {
            return;
        }
        cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>().customRenderingSettings = true;
        cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>().renderingPathCustomFrameSettings = frameSettings;
        cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>().renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        switch(antiAliasing){ 
            case 0:
                cam.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                break;
            case 1:
                cam.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                break;
            case 2:
                cam.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
            case 3:
                cam.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                break;
            default:
                cam.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                break;
        }
    }*/
    public void OnEnable() {
        fullscreenMode = Screen.fullScreenMode;
        resolution = Screen.currentResolution;
        textureLoadingProgress = 0f;

        for(int i=options.Count-1;i>=0;i--) {
            if (options[i].type == OptionType.Resolution) {
                options.RemoveAt(i);
            }
        }
        for(int i=0;i<options.Count;i++) {
            if (options[i].type == GraphicsOptions.OptionType.WindowMode) {
                fullscreenMode = Screen.fullScreenMode;
                options[i].value = (float)Screen.fullScreenMode;
                continue;
            }
            options[i].value = options[i].defaultValue;
            ChangeOption(options[i].type, options[i].defaultValue);
        }
        Option resolutionOptions = new Option();
        resolutionOptions.dropDownDescriptions = new List<string>();
        for(int i=0;i<Screen.resolutions.Length;i++) {
            Resolution r = Screen.resolutions[i];
            resolutionOptions.dropDownDescriptions.Add(r.width + "x" + r.height + " [" + r.refreshRate + "]");
        }
        resolutionOptions.min = 0;
        resolutionOptions.max = Screen.resolutions.Length-1;
        resolutionOptions.type = OptionType.Resolution;
        resolutionOptions.value = resolutionOptions.max;
        resolutionOptions.name = resolutionReference;
        options.Add(resolutionOptions);

        listeners = new List<IGameEventOptionListener>();
        Load();
    }
    public void OnDisable() {
        // Disable the cloud renderer on our way out.
        if (renderer.rendererFeatures.Contains(cloudsSettings)) {
            renderer.rendererFeatures.Remove(cloudsSettings);
        }
    }
    public List<UnityEngine.Rendering.VolumeProfile> volumes;
    public void ChangeOption(OptionChange c) {
        ChangeOption(c.target, c.value);
    }
    public Option GetOption(OptionType type) {
        foreach(Option o in options) {
            if (o.type == type) {
                return o;
            }
        }
        return null;
    }
    public void ChangeOption(OptionType target, float value) {
        foreach(UnityEngine.Rendering.VolumeProfile volume in volumes) {
            switch(target) {
                //case OptionType.ScreenSpaceReflections:
                    //ScreenSpaceReflection reflect;
                    //if (!volume.TryGet<ScreenSpaceReflection>(out reflect)) { break; }
                    //reflect.active = (value!=0);
                    //reflect.enabled.Override(value!=0);
                    //reflect.quality.Override((int)(value-1));
                //break;
                case OptionType.AmbientOcclusion:
                    //AmbientOcclusion ao;
                    //if (!volume.TryGet<AmbientOcclusion>(out ao)) { break; }
                    //ao.active = (value!=0);
                    //ao.quality.Override((int)(value-1));
                    foreach( var feature in renderer.rendererFeatures) {
                        if (feature.name == "Screen Space Ambient Occlusion") {
                            feature.SetActive(value != 0);
                        }
                    }
                break;
                //case OptionType.AntiAliasing:
                    //universalRenderPipelineAsset.msaaSampleCount = (int)Mathf.Pow(2,value);
                    //break;
                //case OptionType.Shadows:
                    //universalRenderPipelineAsset.shadowCascadeOption = (ShadowCascadesOption)(Mathf.FloorToInt(Mathf.Max(value-1,0)));
                    //universalRenderPipelineAsset.shadowDistance = value * 50f;
                    //HDShadowSettings shadows;
                    //if (!volume.TryGet<HDShadowSettings>(out shadows)) { break; }
                    //shadows.active = (value!=0);

                    // Only looks good with temporal anti-aliasing, but TAA is broken with the cloud pass so we either choose clouds or TAA and contact shadows.
                    // ... so fuck TAA
                    //ContactShadows cshadows;
                    //if (!volume.TryGet<ContactShadows>(out cshadows)) { break; }
                    //cshadows.active = (value>=2);
                    //cshadows.quality.Override((int)(value-1));

                    //shadows.cascadeShadowSplitCount = new UnityEngine.Rendering.NoInterpClampedIntParameter(Mathf.Max(3,Mathf.FloorToInt(value)), 0, 4, true);
                    //shadows.maxShadowDistance = new UnityEngine.Rendering.NoInterpMinFloatParameter(value*100f, 0f, true);
                //break;
                case OptionType.OverallQuality:
                    QualitySettings.SetQualityLevel((int)value);
                    if (value == 0) {
                        Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
                    } else {
                        Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
                    }
                    break;
                case OptionType.Resolution:
                    Resolution[] resolutions = Screen.resolutions;
                    if (value < 0 || value >= resolutions.Length) { break; }
                    resolution = resolutions[Mathf.FloorToInt(value)];
                    Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
                break;
                case OptionType.WindowMode:
                    //if (fullscreenMode == (FullScreenMode)Mathf.FloorToInt(value)) {
                        //return;
                    //}
                    fullscreenMode = (FullScreenMode)Mathf.FloorToInt(value);
                    Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
                break;
                //case OptionType.DecalQuality:
                    //if (GameManager.instance != null) {
                        //GameManager.instance.decalPrefabPoolCount = (int)value;
                    //}
                //break;
                case OptionType.Clouds:
                    // OpenGL can't do clouds, sorry!
                    if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore) {
                        value = 0;
                    }
                    if (renderer == null) { break; }
                    if (cloudsSettings!=null) {
                        cloudsSettings.SetActive(value!=0);
                        cloudsSettings.settings.cloudShadows = (value>1);
                        if (cloudsSettings.settings.blitMaterial != null) {
                            cloudsSettings.settings.blitMaterial.SetFloat("numStepsLight", value == 2 ? 12 : 8);
                        }
                    }
                    if (!renderer.rendererFeatures.Contains(cloudsSettings) && value!=0) {
                        renderer.rendererFeatures.Add(cloudsSettings);
                    }
                    if (renderer.rendererFeatures.Contains(cloudsSettings) && value==0) {
                        renderer.rendererFeatures.Remove(cloudsSettings);
                    }
                    break;
                //case OptionType.VideoMemoryUsage:
                    //for(int i=0;i<QualitySettings.names.Length;i++) {
                        //string name = QualitySettings.names[i];
                        //if (name == "Potato" && value == 0) {
                            //QualitySettings.SetQualityLevel(i);
                        //}
                        //if (name == "Recommended" && value == 1) {
                            //QualitySettings.SetQualityLevel(i);
                        //}
                    //}
                    //break;
                case OptionType.MasterVolume:
                case OptionType.SoundEffectsVolume:
                case OptionType.MusicVolume:
                    mixer.SetFloat(target.ToString(), Mathf.Log(Mathf.Max(value,0.01f))*20f);
                    break;
                case OptionType.Language:
                    // -1 is to just use the system locale
                    if (Mathf.Approximately(value,-1)) {
                        break;
                    }
                    if (GameManager.instance == null) {
                        break;
                    }
                    GameManager.instance.StartCoroutine(WaitForLocalizationToBeReadyThenSet((int)value));
                    break;
                case OptionType.VSync:
                    QualitySettings.vSyncCount = Mathf.RoundToInt(Mathf.Clamp(value,0,4));
                    break;
                case OptionType.TargetFramerate:
                    if (Mathf.RoundToInt(value) >= 300) {
                        Application.targetFrameRate = -1;
                    } else {
                        Application.targetFrameRate = Mathf.RoundToInt(value);
                    }
                    break;
                    //case OptionType.ProceduralTextureSize:
                    //if (Application.isEditor || GameManager.instance == null) {
                    //return;
                    //}
                    //// Generate the quality directly below first, because this task can be so slow that it becomes annoying to wait for the final quality level.
                    //GameManager.instance.StopAllCoroutines();
                    //GameManager.instance.StartCoroutine(WaitAndThenSetTextureSize(value));
                    //break;
                default: break;
            }
        }
        foreach(Option o in options) {
            if (o.type == target) {
                o.value = value;
                Raise(o.type, value);
                break;
            }
        }
    }
}
