using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal {
    // The render pass contains logic to configure render target and perform drawing.
    // It contains a renderPassEvent that tells the pipeline where to inject the custom render pass. 
    // The execute method contains the rendering logic.
    public class RaymarchRenderPass : ScriptableRenderPass {
        public ScriptableRenderer m_Renderer;
        private Light cachedLightSource;
        private Texture depthTexture;
        private RenderTargetHandle m_TemporaryColorTexture;
        //public ScriptableRenderer m_Renderer;
        private Light lightSource {
            get {
                if (cachedLightSource == null) {
                    foreach(Light l in Object.FindObjectsOfType<Light>()) {
                        cachedLightSource = l;
                        break;
                    }
                }
                return cachedLightSource;
            }
        }
        private ComputeBuffer shapeBuffer;
        void CreateScene (CommandBuffer cmd) {
            if (shapeBuffer != null){
                shapeBuffer.Dispose();
            }
            var scene = RaymarchScene.GetScene();
            shapeBuffer = new ComputeBuffer (scene.Length, RaymarchScene.ShapeData.GetSize ());
            cmd.SetComputeBufferData(shapeBuffer, scene, 0, 0, scene.Length);
            cmd.SetComputeBufferParam(m_Settings.raymarching, 0, "_Shapes", shapeBuffer);
            cmd.SetComputeIntParam(m_Settings.raymarching, "_NumShapes", scene.Length);
        }

        void OnDisable() {
            if (shapeBuffer != null){
                shapeBuffer.Dispose();
            }
        }

        void SetParameters (CommandBuffer cmd, Camera cam) {
            bool lightIsDirectional = lightSource.type == LightType.Directional;
            cmd.SetComputeMatrixParam(m_Settings.raymarching, "_CameraToWorld", cam.cameraToWorldMatrix);
            cmd.SetComputeMatrixParam(m_Settings.raymarching, "_WorldToCamera", cam.cameraToWorldMatrix.inverse);
            cmd.SetComputeMatrixParam(m_Settings.raymarching, "_CameraInverseProjection", cam.projectionMatrix.inverse);
            cmd.SetComputeVectorParam(m_Settings.raymarching, "_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
            cmd.SetComputeFloatParam(m_Settings.raymarching, "_SceneBlend", m_Settings.sceneBlend);
            cmd.SetComputeIntParam(m_Settings.raymarching, "_PositionLight", lightIsDirectional ? 0 : 1);
        }

        string m_ProfilerTag = "DrawFullScreenRaymarchPass";
        RaymarchRenderFeature.RaymarchRenderSettings m_Settings;

        public RaymarchRenderPass(RaymarchRenderFeature.RaymarchRenderSettings settings) {
            renderPassEvent = settings.renderPassEvent;
            
            m_Settings = settings;
            m_TemporaryColorTexture.Init("_RaymarchTemporaryColorTexture");
        }

        // called each frame before Execute, use it to set up things the pass will need
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor CTD) {
            // Depth textures cannot use the "enableRandomWrite" feature.
            if (CTD.colorFormat == RenderTextureFormat.Depth) {
                return;
            }
            // Since we can't write directly to the camera color texture, we create a texture to write to (then we blit it to the camera after).
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, Mathf.FloorToInt(CTD.width*m_Settings.renderQuality), Mathf.FloorToInt(CTD.height*m_Settings.renderQuality), 0, m_Settings.filterMode, Experimental.Rendering.GraphicsFormat.R8G8B8A8_SNorm, 1, true, RenderTextureMemoryless.None, false);
            CreateScene (cmd);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            Camera camera = renderingData.cameraData.camera;
            RenderTextureDescriptor CTD = renderingData.cameraData.cameraTargetDescriptor;
            // Skip depth renders, as they don't support writing on the compute shader.
            if (CTD.colorFormat == RenderTextureFormat.Depth) {
                return;
            }

            // If we magically just don't have a depth buffer. We cancel the pass.
            // _CameraDepthTexture can be null in some situations (like idling in the editor while alt-tabbed).
            if (depthTexture == null) {
                depthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
            }
            if (depthTexture == null) {
                return;
            }

            var cmd = CommandBufferPool.Get(m_ProfilerTag);
            SetParameters (cmd, camera);
            cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Source", m_Renderer.cameraColorTarget);
            // Wtf, can't get the depth buffer in what seems to be the official way... Guess we just set it from the global _CameraDepthTexture :clown:
            //cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Depth", m_Renderer.cameraDepthTarget);
            m_Settings.raymarching.SetTextureFromGlobal(0, "Depth", "_CameraDepthTexture");
            cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Destination", m_TemporaryColorTexture.Identifier());

            int threadGroupsX = Mathf.CeilToInt ((camera.pixelWidth*m_Settings.renderQuality) / 8.0f);
            int threadGroupsY = Mathf.CeilToInt ((camera.pixelHeight*m_Settings.renderQuality) / 8.0f);

            cmd.DispatchCompute(m_Settings.raymarching, 0, threadGroupsX, threadGroupsY, 1);
            cmd.Blit(m_TemporaryColorTexture.Identifier(), m_Renderer.cameraColorTarget, m_Settings.alphaBlit);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            //var cmd = CommandBufferPool.Get(m_ProfilerTag);
            //cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Settings.material);
            //cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }
}