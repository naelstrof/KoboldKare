using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal {
    // The render pass contains logic to configure render target and perform drawing.
    // It contains a renderPassEvent that tells the pipeline where to inject the custom render pass. 
    // The execute method contains the rendering logic.
    public class RaymarchRenderPass : ScriptableRenderPass {
        public ScriptableRenderer m_Renderer;
        private Light cachedLightSource;
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
            List<Shape> allShapes = new List<Shape> (Object.FindObjectsOfType<Shape> ());
            allShapes.Sort ((a, b) => a.operation.CompareTo (b.operation));

            List<Shape> orderedShapes = new List<Shape> ();

            for (int i = 0; i < allShapes.Count; i++) {
                // Add top-level shapes (those without a parent)
                if (allShapes[i].transform.parent == null) {

                    Transform parentShape = allShapes[i].transform;
                    orderedShapes.Add (allShapes[i]);
                    allShapes[i].numChildren = parentShape.childCount;
                    // Add all children of the shape (nested children not supported currently)
                    for (int j = 0; j < parentShape.childCount; j++) {
                        if (parentShape.GetChild (j).GetComponent<Shape> () != null) {
                            orderedShapes.Add (parentShape.GetChild (j).GetComponent<Shape> ());
                            orderedShapes[orderedShapes.Count - 1].numChildren = 0;
                        }
                    }
                }

            }

            ShapeData[] shapeData = new ShapeData[orderedShapes.Count];
            for (int i = 0; i < orderedShapes.Count; i++) {
                var s = orderedShapes[i];
                Vector3 col = new Vector3 (s.colour.r, s.colour.g, s.colour.b);
                shapeData[i] = new ShapeData () {
                    position = s.Position,
                    scale = s.Scale, colour = col,
                    radius = s.radius,
                    shapeType = (int) s.shapeType,
                    operation = (int) s.operation,
                    blendStrength = s.blendStrength*3,
                    numChildren = s.numChildren
                };
            }

            if (shapeBuffer != null){
                shapeBuffer.Dispose();
            }
            shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize ());
            cmd.SetComputeBufferData(shapeBuffer, shapeData, 0, 0, shapeData.Length);
            cmd.SetComputeBufferParam(m_Settings.raymarching, 0, "shapes", shapeBuffer);
            cmd.SetComputeIntParam(m_Settings.raymarching, "numShapes", shapeData.Length);
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
            cmd.SetComputeFloatParam(m_Settings.raymarching, "_FarClipPlane", cam.farClipPlane);
            cmd.SetComputeFloatParam(m_Settings.raymarching, "_NearClipPlane", cam.nearClipPlane);
            // To help decode depth buffer values....
            //float zFar = cam.farClipPlane;
            //float zNear = cam.nearClipPlane;
            //cmd.SetComputeVectorParam(m_Settings.raymarching, "_ProjectionParams",
                                                //new Vector3(zFar / ( zFar - zNear),
                                                //zFar * zNear / (zNear - zFar), 0));
            cmd.SetComputeIntParam(m_Settings.raymarching, "positionLight", lightIsDirectional ? 0 : 1);
        }

        struct ShapeData {
            public Vector3 position;
            public Vector3 scale;
            public Vector3 colour;
            public float radius;
            public int shapeType;
            public int operation;
            public float blendStrength;
            public int numChildren;

            public static int GetSize () {
                return sizeof (float) * 11 + sizeof (int) * 3;
            }
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
            //ConfigureTarget(m_Renderer.cameraDepthTarget, m_Renderer.cameraDepthTarget);
            CTD.enableRandomWrite = true;
            CTD.depthBufferBits = 0;
            CTD.sRGB = false;
            // Since we can't write directly to the camera color texture, we create a texture to write to (then we blit it to the camera after).
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, CTD);
            CreateScene (cmd);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            Camera camera = renderingData.cameraData.camera;
            RenderTextureDescriptor CTD = renderingData.cameraData.cameraTargetDescriptor;
            // Skip depth renders, as they don't support writing on the compute shader.
            if (CTD.colorFormat == RenderTextureFormat.Depth) {
                return;
            }

            var cmd = CommandBufferPool.Get(m_ProfilerTag);
            SetParameters (cmd, camera);
            cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Source", m_Renderer.cameraColorTarget);
            // Wtf, can't get the depth buffer in what seems to be the official way... Guess we just set it from the global _CameraDepthTexture :clown:
            //cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Depth", m_Renderer.cameraDepthTarget);
            m_Settings.raymarching.SetTextureFromGlobal(0, "Depth", "_CameraDepthTexture");
            cmd.SetComputeTextureParam(m_Settings.raymarching, 0, "Destination", m_TemporaryColorTexture.Identifier());

            int threadGroupsX = Mathf.CeilToInt (camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt (camera.pixelHeight / 8.0f);

            cmd.DispatchCompute(m_Settings.raymarching, 0, threadGroupsX, threadGroupsY, 1);
            cmd.Blit(m_TemporaryColorTexture.Identifier(), m_Renderer.cameraColorTarget);
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