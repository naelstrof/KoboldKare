Shader "ShearTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    HLSLINCLUDE
        #pragma target 4.5

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    ENDHLSL


    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                uint vertexID : SV_VertexID;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4x4 inverseCameraProjectionMatrix;
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = GetFullScreenTriangleVertexPosition(v.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(v.vertexID);
                return output;
            }

            // Textures
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 frag (v2f i) : SV_Target
            {
                uint2 pixelCoords = i.uv * _ScreenSize.xy;
                float3 backgroundCol = LOAD_TEXTURE2D(_MainTex, i.uv).rgb;
                return float4(backgroundCol,0);
            }

            ENDHLSL
        }
    }
}