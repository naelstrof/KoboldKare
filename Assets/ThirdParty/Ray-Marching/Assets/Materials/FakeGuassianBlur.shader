// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
Shader "FakeGuassianBlur" {
    Properties
    {
        _MainTex ("Texture", any) = "" {}
        _Color("Multiplicative color", Color) = (1.0, 1.0, 1.0, 1.0)
        _BlurRadius("Blur Radius", Float) = 1
        _Resolution("Resolution", Vector) = (1,1,0,0)
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            uniform float4 _MainTex_ST;
            uniform float4 _Color;
            uniform float _BlurRadius;
            uniform float2 _Resolution;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 color1 = fixed4(0,0,0,0);
                float2 off1 = float2(1.3333333333333333,0) * _BlurRadius;
                color1 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord) * 0.29411764705882354;
                color1 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord + (off1 / _Resolution)) * 0.35294117647058826;
                color1 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord - (off1 / _Resolution)) * 0.35294117647058826;

                fixed4 color2 = fixed4(0,0,0,0);
                off1 = float2(0, 1.3333333333333333) * _BlurRadius;
                color2 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord) * 0.29411764705882354;
                color2 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord + (off1 / _Resolution)) * 0.35294117647058826;
                color2 += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord - (off1 / _Resolution)) * 0.35294117647058826;
                return (color1 + color2)*0.5;
            }
            ENDCG

        }
    }
    Fallback Off
}