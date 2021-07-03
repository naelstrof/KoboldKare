
namespace UnityEngine.Rendering.Universal {
    // A renderer feature contains data and logic to enqueue one or more render passes in the LWRP renderer.
    // In order to add a render feature to a LWRP renderer, click on the renderer asset and then on the + icon in 
    // the renderer features list. LWRP uses reflection to list all renderer features in the project as available to be 
    // added as renderer features.
    public class RaymarchRenderFeature : ScriptableRendererFeature {
        [System.Serializable]
        public struct RaymarchRenderSettings {
            // The render pass event to inject this render feature.
            public RenderPassEvent renderPassEvent;
            public ComputeShader raymarching;
            public FilterMode filterMode;
        }

        // Contains settings for the render pass.
        public RaymarchRenderSettings m_Settings;
        
        // The actual render pass we are injecting.
        RaymarchRenderPass m_RaymarchRenderPass;

        public override void Create() {
            // Caches the render pass. Create method is called when the renderer instance is being constructed. 
            m_RaymarchRenderPass = new RaymarchRenderPass(m_Settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            // Enqueues the render pass for execution. Here you can inject one or more render passes in the renderer
            // AddRenderPasses is called everyframe. 
            if (m_Settings.raymarching == null) {
                return;
            }
            //m_RaymarchRenderPass.cameraColorTarget = renderer.cameraColorTarget;
            //m_RaymarchRenderPass.cameraDepthTarget = renderer.cameraDepthTarget;
            m_RaymarchRenderPass.ConfigureInput(ScriptableRenderPassInput.Color & ScriptableRenderPassInput.Depth);
            m_RaymarchRenderPass.m_Renderer = renderer;
            renderer.EnqueuePass(m_RaymarchRenderPass);
        }
    }
}