using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricClouds : ScriptableRendererFeature {
    public class VolumetricCloudsPass : ScriptableRenderPass {
        public enum RenderTarget {
            Color,
            RenderTexture,
        }
 
        public Material blitMaterial = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }
 
        //private RenderTargetIdentifier source { get; set; }
        private ScriptableRenderer rendererSource {get;set;}
        private RenderTargetHandle destination { get; set; }
        public RenderTexture shapeTexture;
        public RenderTexture detailTexture;
 
        RenderTargetHandle m_TemporaryColorTexture;
        string m_ProfilerTag;
        public VolumetricCloudsPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag, RenderTexture shapeTexture, RenderTexture detailTexture) {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            this.shapeTexture = shapeTexture;
            this.detailTexture = detailTexture;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }
         
        public void Setup(ScriptableRenderer source, RenderTargetHandle destination) {
            this.rendererSource = source;
            this.destination = destination;
        }
         
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (!renderingData.cameraData.postProcessEnabled) {
                return;
            }
            blitMaterial.SetTexture ("NoiseTex", shapeTexture);
            blitMaterial.SetTexture ("DetailNoiseTex", detailTexture);
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
 
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            // Can't read and write to same color target, use a TemporaryRT
            if (destination == RenderTargetHandle.CameraTarget) {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                Blit(cmd, rendererSource.cameraColorTarget, m_TemporaryColorTexture.Identifier(), blitMaterial, blitShaderPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), rendererSource.cameraColorTarget);
            } else {
                Blit(cmd, rendererSource.cameraColorTarget, destination.Identifier(), blitMaterial, blitShaderPassIndex);
            }
 
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
         
        public override void FrameCleanup(CommandBuffer cmd) {
            if (destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }
 
    [System.Serializable]
    public class VolumetricCloudsSettings {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
 
        public Material blitMaterial = null;
        public int blitMaterialPassIndex = -1;
        public Target destination = Target.Color;
        public bool cloudShadows = true;
        public int shapeNoiseResolution = 128;
        public int detailNoiseResolution = 64;
        public string textureId = "_VolumetricCloudsPassTexture";
    }

 
    public enum Target {
        Color,
        Texture
    }
 
    public VolumetricCloudsSettings settings = new VolumetricCloudsSettings();
    RenderTargetHandle m_RenderTextureHandle;
    VolumetricCloudsPass blitPass;
    ComputeShader noiseCompute;
    List<WorleyNoiseSettings> shapeSettings = new List<WorleyNoiseSettings>();
    List<WorleyNoiseSettings> detailSettings = new List<WorleyNoiseSettings>();
    bool valid;

    RenderTexture shapeTexture;
    RenderTexture detailTexture;

    enum CloudNoiseType { Shape, Detail }
    enum TextureChannel { R, G, B, A }

    WorleyNoiseSettings GetActiveSettings(CloudNoiseType type, TextureChannel channel) {
        var settings = (type == CloudNoiseType.Shape) ? shapeSettings : detailSettings;
        int activeChannelIndex = (int) channel;
        if (activeChannelIndex >= settings.Count) {
            return null;
        }
        return settings[activeChannelIndex];
    }

    RenderTexture GetNoiseTexture(CloudNoiseType type) {
        switch(type) {
            case CloudNoiseType.Shape: return shapeTexture;
            case CloudNoiseType.Detail: return detailTexture;
        }
        return null;
    }
    Vector4 GetChannelMask(TextureChannel channel) {
        Vector4 channelWeight = new Vector4 (
            (channel == TextureChannel.R) ? 1 : 0,
            (channel == TextureChannel.G) ? 1 : 0,
            (channel == TextureChannel.B) ? 1 : 0,
            (channel == TextureChannel.A) ? 1 : 0
        );
        return channelWeight;
    }

    List<ComputeBuffer> UpdateWorley (WorleyNoiseSettings settings) {
        List<ComputeBuffer> buffers = new List<ComputeBuffer>();
        var prng = new System.Random (settings.seed);
        buffers.Add(CreateWorleyPointsBuffer (prng, settings.numDivisionsA, "pointsA"));
        buffers.Add(CreateWorleyPointsBuffer (prng, settings.numDivisionsB, "pointsB"));
        buffers.Add(CreateWorleyPointsBuffer (prng, settings.numDivisionsC, "pointsC"));

        noiseCompute.SetInt ("numCellsA", settings.numDivisionsA);
        noiseCompute.SetInt ("numCellsB", settings.numDivisionsB);
        noiseCompute.SetInt ("numCellsC", settings.numDivisionsC);
        noiseCompute.SetBool ("invertNoise", settings.invert);
        noiseCompute.SetInt ("tile", settings.tile);
        return buffers;
    }

    ComputeBuffer CreateWorleyPointsBuffer (System.Random prng, int numCellsPerAxis, string bufferName) {
        var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
        float cellSize = 1f / numCellsPerAxis;

        for (int x = 0; x < numCellsPerAxis; x++) {
            for (int y = 0; y < numCellsPerAxis; y++) {
                for (int z = 0; z < numCellsPerAxis; z++) {
                    float randomX = (float) prng.NextDouble ();
                    float randomY = (float) prng.NextDouble ();
                    float randomZ = (float) prng.NextDouble ();
                    Vector3 randomOffset = new Vector3 (randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = new Vector3 (x, y, z) * cellSize;

                    int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                    points[index] = cellCorner + randomOffset;
                }
            }
        }

        return CreateBuffer (points, sizeof (float) * 3, bufferName);
    }
    
    ComputeBuffer CreateBuffer (System.Array data, int stride, string bufferName, int kernel = 0) {
        var buffer = new ComputeBuffer (data.Length, stride, ComputeBufferType.Structured);
        buffer.SetData (data);
        noiseCompute.SetBuffer (kernel, bufferName, buffer);
        return buffer;
    }


    void RenderNoise( CloudNoiseType t ) {
        List<ComputeBuffer> buffersToRelease = new List<ComputeBuffer> ();

        RenderTexture ActiveTexture = GetNoiseTexture(t);
        foreach (TextureChannel channel in (TextureChannel[]) Enum.GetValues(typeof(TextureChannel))) {
            WorleyNoiseSettings ActiveSettings = GetActiveSettings(t, channel);
            if (ActiveSettings == null) {
                continue;
            }
            // Set values:
            noiseCompute.SetFloat ("persistence", ActiveSettings.persistence);
            noiseCompute.SetInt ("resolution", ActiveTexture.width);
            noiseCompute.SetVector ("channelMask", GetChannelMask(channel));
            // Set noise gen kernel data:
            noiseCompute.SetTexture (0, "Result", ActiveTexture);
            var minMaxBuffer = CreateBuffer (new int[] { int.MaxValue, 0 }, sizeof (int), "minMax", 0);
            buffersToRelease.Add(minMaxBuffer);
            buffersToRelease.AddRange(UpdateWorley (ActiveSettings));
            noiseCompute.SetTexture (0, "Result", ActiveTexture);
            //var noiseValuesBuffer = CreateBuffer (activeNoiseValues, sizeof (float) * 4, "values");

            // Dispatch noise gen kernel
            int numThreadGroups = Mathf.CeilToInt ((float)ActiveTexture.width / 8f);
            noiseCompute.Dispatch (0, numThreadGroups, numThreadGroups, numThreadGroups);

            // Set normalization kernel data:
            noiseCompute.SetBuffer (1, "minMax", minMaxBuffer);
            noiseCompute.SetTexture (1, "Result", ActiveTexture);
            // Dispatch normalization kernel
            noiseCompute.Dispatch (1, numThreadGroups, numThreadGroups, numThreadGroups);
        }

        // Release buffers
        foreach (var buffer in buffersToRelease) {
            buffer.Release ();
        }
    }

    void CreateTexture (ref RenderTexture texture, int resolution, string name) {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated () || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format) {
            //Debug.Log ("Create tex: update noise: " + updateNoise);
            if (texture != null) {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.name = name;

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
    }

    public bool Validate() {
        foreach(WorleyNoiseSettings s in shapeSettings) {
            if (s == null) {
                Debug.LogError("Failed to load shape settings, make sure they're in the Assets/Resources/Clouds/ as Shape_1, Shape_2, etc");
                return false;
            }
        }
        foreach(WorleyNoiseSettings s in detailSettings) {
            if (s == null) {
                Debug.LogError("Failed to load detail settings, make sure they're in the Assets/Resources/Clouds/ as Detail_1, Detail_2, etc");
                return false;
            }
        }
        if (shapeTexture == null) {
            return false;
        }
        if (detailTexture == null) {
            return false;
        }
        return true;
    }
         
 
    public override void Create() {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);

        noiseCompute = Resources.Load<ComputeShader>("Clouds/NoiseGenCompute");
        shapeSettings.Clear();
        for (int i=1;i<5;i++) {
            shapeSettings.Add(Resources.Load<WorleyNoiseSettings>("Clouds/Shape_"+i));
        }
        detailSettings.Clear();
        for (int i=1;i<4;i++) {
            detailSettings.Add(Resources.Load<WorleyNoiseSettings>("Clouds/Detail_"+i));
        }
        CreateTexture (ref shapeTexture, settings.shapeNoiseResolution, "CloudShapeNoise");
        CreateTexture (ref detailTexture, settings.detailNoiseResolution, "CloudDetailNoise");
        RenderNoise(CloudNoiseType.Shape);
        RenderNoise(CloudNoiseType.Detail);
        valid = Validate();
        if (settings.blitMaterial != null) {
            if (settings.cloudShadows) {
                settings.blitMaterial.EnableKeyword("CLOUD_SHADOWS_ON");
                settings.blitMaterial.DisableKeyword("CLOUD_SHADOWS_OFF");
            } else {
                settings.blitMaterial.DisableKeyword("CLOUD_SHADOWS_ON");
                settings.blitMaterial.EnableKeyword("CLOUD_SHADOWS_OFF");
            }
        }
        blitPass = new VolumetricCloudsPass(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name, GetNoiseTexture(CloudNoiseType.Shape), GetNoiseTexture(CloudNoiseType.Detail));
    }
 
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        var dest = (settings.destination == Target.Color) ? RenderTargetHandle.CameraTarget : m_RenderTextureHandle;
 
        if (settings.blitMaterial == null) {
            Debug.LogWarningFormat("Missing Blit Material. {0} Volumetric Clouds pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
 
        blitPass.Setup(renderer, dest);
        renderer.EnqueuePass(blitPass);
    }
}

    /*public Vector3Parameter cloudTestParams = new Vector3Parameter{value = new Vector3(0,0,0)};
    public IntParameter numStepsLight = new IntParameter{ value = 8 };
    public FloatParameter rayOffsetStrength = new FloatParameter{ value = 10 };
    public TextureParameter blueNoise = new TextureParameter{ value = null};
    public FloatParameter cloudScale = new FloatParameter{ value=0.62f};
    public FloatParameter densityMultiplier = new FloatParameter{ value=1f};
    public FloatParameter densityOffset = new FloatParameter{ value = -4.27f};
    public Vector3Parameter shapeOffset = new Vector3Parameter{ value = new Vector3(190.44f,0,0)};
    public Vector2Parameter heightOffset = new Vector2Parameter{ value = new Vector2(0,0)};
    public Vector4Parameter shapeNoiseWeights = new Vector4Parameter{ value = new Vector4(1,0.48f,0.15f,0f)};
    public FloatParameter detailNoiseScale = new FloatParameter{ value = 3f};
    public FloatParameter detailNoiseWeight = new FloatParameter{ value = 3.42f};
    public Vector3Parameter detailNoiseWeights = new Vector3Parameter{ value =new Vector3(1,0.5f,0.5f)};
    public Vector3Parameter detailOffset = new Vector3Parameter{ value =new Vector3(51.25f,0,0)};
    

    public FloatParameter lightAbsorptionThroughCloud = new FloatParameter{ value= 0.75f};
    public FloatParameter lightAbsorptionTowardSun = new FloatParameter{ value=1.21f};
    public FloatParameter darknessThreshold = new FloatParameter{ value=.15f};
    public FloatParameter forwardScattering = new FloatParameter{value=.811f};
    public FloatParameter backScattering = new FloatParameter{value=.33f};
    public FloatParameter baseBrightness = new FloatParameter{value=1f};
    public FloatParameter phaseFactor = new FloatParameter{value=.488f};

    public FloatParameter timeScale = new FloatParameter{value=1f};
    public FloatParameter baseSpeed = new FloatParameter{value=0.5f};
    public FloatParameter detailSpeed = new FloatParameter{value=1f};

    public UnityEngine.Rendering.PostProcessing.MinMaxAttribute shadowRemap = new MinMaxAttribute(0f,1f);
    public BoolParameter shadowsEnabled = new BoolParameter{ value = true};

    public ColorParameter colA = new ColorParameter{value=new Color(227f/255f,241f/255f,1f,1f)};
    public ColorParameter colB = new ColorParameter{value=new Color(113f/255f, 164f/255f, 204f/255f, 1f)};
    public Vector3Parameter boundsScale = new Vector3Parameter{value = Vector3.one};
    public Vector3Parameter boundsPosition = new Vector3Parameter{value = Vector3.zero};
}

public sealed class VolumetricCloudsRenderer : PostProcessEffectRenderer<VolumetricClouds> {
    Material m_Material;
    Camera m_Camera;
    Light m_Sun;

    public override void Render(PostProcessRenderContext context) {
        if (m_Material == null)
            return;

        var sheet = context.propertySheets.Get(Shader.Find(kShaderName));
        //m_Material.SetTexture ("_InputTexture",context.command.GetTemporaryRT(context.source));
        //sheet.properties.SetTexture ("_MainTex", context.source);
        sheet.properties.SetTexture ("NoiseTex", GetNoiseTexture(CloudNoiseType.Shape));
        sheet.properties.SetTexture ("DetailNoiseTex", GetNoiseTexture(CloudNoiseType.Detail));
        sheet.properties.SetTexture ("BlueNoise", settings.blueNoise.value);

        Vector3 size = settings.boundsScale.value;
        int width = Mathf.CeilToInt (size.x);
        int height = Mathf.CeilToInt (size.y);
        int depth = Mathf.CeilToInt (size.z);

        if (m_Camera == null) {
            m_Camera = Camera.current;
        }
        if (m_Camera == null) {
            m_Camera = Camera.main;
        }
        if (m_Camera != null) {
            sheet.properties.SetMatrix("inverseCameraProjectionMatrix", m_Camera.projectionMatrix.inverse);
        }
        //m_Material.SetMatrix("inverseCameraViewMatrix", m_Camera.cameraToWorldMatrix);
        if (m_Sun == null) {
            foreach(Light l in GameObject.FindObjectsOfType<Light>()) {
                if (l.type == LightType.Directional) {
                    m_Sun = l;
                    break;
                }
            }
        }
        //if (settings.shadowsEnabled.value) {
            //m_Material.EnableKeyword("CLOUD_SHADOWS_ON");
            //m_Material.DisableKeyword("CLOUD_SHADOWS_OFF");
        //} else {
            //m_Material.DisableKeyword("CLOUD_SHADOWS_ON");
            //m_Material.EnableKeyword("CLOUD_SHADOWS_OFF");
        //}
        sheet.properties.SetVector("WorldSpaceLightPos", m_Sun != null ? -m_Sun.transform.forward : Vector3.up );
        sheet.properties.SetVector("LightColor", m_Sun != null ? m_Sun.color : Color.white);
        sheet.properties.SetFloat("lightIntensity", m_Sun != null ? Mathf.Clamp01(Vector3.Dot(-m_Sun.transform.forward, Vector3.up)*7f+1.2f) : 1f);
        sheet.properties.SetFloat ("scale", settings.cloudScale.value);
        sheet.properties.SetFloat ("densityMultiplier", settings.densityMultiplier.value);
        sheet.properties.SetFloat ("densityOffset", settings.densityOffset.value);
        sheet.properties.SetFloat ("lightAbsorptionThroughCloud", settings.lightAbsorptionThroughCloud.value);
        sheet.properties.SetFloat ("lightAbsorptionTowardSun", settings.lightAbsorptionTowardSun.value);
        sheet.properties.SetFloat ("darknessThreshold", settings.darknessThreshold.value);
        sheet.properties.SetVector ("params", settings.cloudTestParams.value);
        sheet.properties.SetFloat ("rayOffsetStrength", settings.rayOffsetStrength.value);

        sheet.properties.SetFloat ("shadowMin", settings.shadowRemap.min);
        sheet.properties.SetFloat ("shadowMax", settings.shadowRemap.max);
        sheet.properties.SetFloat ("detailNoiseScale", settings.detailNoiseScale.value);
        sheet.properties.SetFloat ("detailNoiseWeight", settings.detailNoiseWeight.value);
        sheet.properties.SetVector ("shapeOffset", settings.shapeOffset.value);
        sheet.properties.SetVector ("detailOffset", settings.detailOffset.value);
        sheet.properties.SetVector ("detailWeights", settings.detailNoiseWeights.value);
        sheet.properties.SetVector ("shapeNoiseWeights", settings.shapeNoiseWeights.value);
        sheet.properties.SetVector ("phaseParams", new Vector4 (settings.forwardScattering.value, settings.backScattering.value, settings.baseBrightness.value, settings.phaseFactor.value));

        sheet.properties.SetVector ("boundsMin", settings.boundsPosition.value - settings.boundsScale.value / 2);
        sheet.properties.SetVector ("boundsMax", settings.boundsPosition.value + settings.boundsScale.value / 2);

        sheet.properties.SetInt ("numStepsLight", settings.numStepsLight.value);

        sheet.properties.SetVector ("mapSize", new Vector4 (width, height, depth, 0));

        sheet.properties.SetFloat ("timeScale", (Application.isPlaying) ? settings.timeScale.value : 0);
        sheet.properties.SetFloat ("baseSpeed", settings.baseSpeed.value);
        sheet.properties.SetFloat ("detailSpeed", settings.detailSpeed.value);

        // Set debug params
        //int debugModeIndex = 0;
        //if (m_Noise.viewerEnabled) {
            //debugModeIndex = (m_Noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        //}

        //sheet.properties.SetInt ("debugViewMode", debugModeIndex);
        //sheet.properties.SetFloat ("debugNoiseSliceDepth", m_Noise.viewerSliceDepth);
        //sheet.properties.SetFloat ("debugTileAmount", m_Noise.viewerTileAmount);
        //sheet.properties.SetFloat ("viewerSize", m_Noise.viewerSize);
        //sheet.properties.SetVector ("debugChannelWeight", m_Noise.ChannelMask);
        //sheet.properties.SetInt ("debugGreyscale", (m_Noise.viewerGreyscale) ? 1 : 0);
        //sheet.properties.SetInt ("debugShowAllChannels", (m_Noise.viewerShowAllChannels) ? 1 : 0);

        sheet.properties.SetColor ("colA", settings.colA.value);
        sheet.properties.SetColor ("colB", settings.colB.value);

        // Bit does the following:
        // - sets _MainTex property on sheet.properties to the source texture
        // - sets the render target to the destination texture
        // - draws a full-screen quad
        // This copies the src texture to the dest texture, with whatever modifications the shader makes
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        //Graphics.Blit (context.source, context.destination, m_Material);
        //HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Release() {
        UnityEngine.Rendering.CoreUtils.Destroy(m_Material);
    }
}*/