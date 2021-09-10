using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VolumetricClouds : ScriptableRendererFeature {
    public class VolumetricCloudsPass : ScriptableRenderPass {
        public RenderTexture shapeTexture;
        public RenderTexture detailTexture;
        
        AsyncOperationHandle<ComputeShader> noiseComputeLoader;
        AsyncOperationHandle<WorleyNoiseSettings>[] worleyNoiseLoaders = new AsyncOperationHandle<WorleyNoiseSettings>[7];
        ComputeShader noiseCompute;
        VolumetricCloudsSettings settings;
        List<WorleyNoiseSettings> shapeSettings = new List<WorleyNoiseSettings>();
        List<WorleyNoiseSettings> detailSettings = new List<WorleyNoiseSettings>();

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
 
        RenderTargetHandle m_TemporaryColorTexture;
        string m_ProfilerTag;
        bool ready = false;
        public void LoadCompleteCheck(AsyncOperationHandle handle) {
            if (ready) {
                return;
            }
            if (!noiseComputeLoader.IsValid() || !noiseComputeLoader.IsDone) {
                return;
            }
            foreach(var h in worleyNoiseLoaders) {
                if (!h.IsValid()) {
                    return;
                }
                if (!h.IsDone) {
                    return;
                }
            }

            noiseCompute = noiseComputeLoader.Result;

            shapeSettings.Clear();
            for (int i=1;i<5;i++) {
                shapeSettings.Add(worleyNoiseLoaders[i-1].Result);
            }
            detailSettings.Clear();
            for (int i=1;i<4;i++) {
                detailSettings.Add(worleyNoiseLoaders[i+3].Result);
            }
            CreateTexture (ref shapeTexture, settings.shapeNoiseResolution, "CloudShapeNoise");
            CreateTexture (ref detailTexture, settings.detailNoiseResolution, "CloudDetailNoise");
            RenderNoise(CloudNoiseType.Shape);
            RenderNoise(CloudNoiseType.Detail);
            ready = true;
        }
        public VolumetricCloudsPass(VolumetricCloudsSettings settings, string tag) {
            this.settings = settings;
            if (!ready) {
                noiseComputeLoader = Addressables.LoadAssetAsync<ComputeShader>("NoiseGenCompute");
                ((AsyncOperationHandle)noiseComputeLoader).Completed += LoadCompleteCheck;
                for (int i=1;i<5;i++) {
                    worleyNoiseLoaders[i-1] = Addressables.LoadAssetAsync<WorleyNoiseSettings>("Shape_"+i);
                }
                for (int i=1;i<4;i++) {
                    worleyNoiseLoaders[i+3] = Addressables.LoadAssetAsync<WorleyNoiseSettings>("Detail_"+i);
                }
                foreach(var handle in worleyNoiseLoaders) {
                    ((AsyncOperationHandle)handle).Completed += LoadCompleteCheck;
                }
            }
            this.renderPassEvent = settings.Event;
            this.blitMaterial = settings.blitMaterial;
            this.blitShaderPassIndex = settings.blitMaterialPassIndex;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }
         
        public void Setup(ScriptableRenderer source, RenderTargetHandle destination) {
            this.rendererSource = source;
            this.destination = destination;
        }
         
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (!renderingData.cameraData.postProcessEnabled || !ready) {
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
    bool valid;


    public bool Validate() {
        if (settings.blitMaterial == null) {
            return false;
        }
        return true;
    }
         
 
    public override void Create() {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);

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
        blitPass = new VolumetricCloudsPass(settings, name);
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