// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mtree/SRP/Leafs URP"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Header(Albedo Texture)]_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_Cutoff("Cutoff", Range( 0 , 1)) = 0.5
		[Header(Normal Texture)]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Strength", Float) = 1
		[Enum(On,0,Off,1)][Header(Color Settings)]_ColorShifting("Color Shifting", Int) = 1
		_Hue("Hue", Range( -0.5 , 0.5)) = -0.5
		_Value("Value", Range( 0 , 3)) = 1
		_Saturation("Saturation", Range( 0 , 2)) = 1
		_ColorVariation("Color Variation", Range( 0 , 0.3)) = 0.15
		[Header(Other Settings)]_OcclusionStrength("AO strength", Range( 0 , 1)) = 0.6
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Glossiness("Smoothness", Range( 0 , 1)) = 0
		[Header(Wind)]_GlobalWindInfluence("Global Wind Influence", Range( 0 , 1)) = 1
		_GlobalTurbulenceInfluence("Global Turbulence Influence", Range( 0 , 1)) = 1
		[ASEEnd][Enum(Leaves,0,Palm,1,Grass,2,Off,3)]_WindModeLeaves("Wind Mode Leaves", Int) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		_TransStrength( "Strength", Range( 0, 50 ) ) = 1
		_TransNormal( "Normal Distortion", Range( 0, 1 ) ) = 0.5
		_TransScattering( "Scattering", Range( 1, 50 ) ) = 2
		_TransDirect( "Direct", Range( 0, 1 ) ) = 0.9
		_TransAmbient( "Ambient", Range( 0, 1 ) ) = 0.1
		_TransShadow( "Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Off
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110


			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma multi_compile _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			
			#pragma multi_compile _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTERED_RENDERING

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;
			sampler2D _BumpMap;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord8.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv_MainTex = IN.ase_texcoord8.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float4 VAR_AlbedoTexture267 = tex2DNode13;
				float4 VAR_Albedo101 = ( _Color * VAR_AlbedoTexture267 );
				float4 VAR_Albedo18_g498 = VAR_Albedo101;
				float3 hsvTorgb9_g498 = RGBToHSV( VAR_Albedo18_g498.rgb );
				float3 hsvTorgb13_g498 = HSVToRGB( float3(( ( ( IN.ase_color.g - 0.5 ) * _ColorVariation ) + _Hue + hsvTorgb9_g498 ).x,( hsvTorgb9_g498.y * _Saturation ),( hsvTorgb9_g498.z * _Value )) );
				float4 lerpResult19_g498 = lerp( float4( hsvTorgb13_g498 , 0.0 ) , VAR_Albedo18_g498 , (float)_ColorShifting);
				float4 OUT_Albedo254 = lerpResult19_g498;
				
				float2 uv_BumpMap = IN.ase_texcoord8.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float3 unpack53 = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
				unpack53.z = lerp( 1, unpack53.z, saturate(_BumpScale) );
				float3 OUT_Normal255 = unpack53;
				
				float lerpResult268 = lerp( 0.0 , VAR_AlbedoTexture267.r , _Glossiness);
				float OUT_Smoothness50 = lerpResult268;
				
				float lerpResult41 = lerp( 1.0 , IN.ase_color.a , _OcclusionStrength);
				float OUT_AO44 = lerpResult41;
				
				float OUT_Alpha46 = tex2DNode13.a;
				
				float3 temp_cast_6 = (OUT_AO44).xxx;
				
				float3 Albedo = OUT_Albedo254.rgb;
				float3 Normal = OUT_Normal255;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = OUT_Smoothness50;
				float Occlusion = OUT_AO44;
				float Alpha = OUT_Alpha46;
				float AlphaClipThreshold = _Cutoff;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = temp_cast_6;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif
				
				#ifdef _CLEARCOAT
				float CoatMask = 0;
				float CoatSmoothness = 0;
				#endif


				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif


				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#endif

				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif

					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = Albedo;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;


				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);


			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif

				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif


				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float OUT_Alpha46 = tex2DNode13.a;
				
				float Alpha = OUT_Alpha46;
				float AlphaClipThreshold = _Cutoff;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float OUT_Alpha46 = tex2DNode13.a;
				
				float Alpha = OUT_Alpha46;
				float AlphaClipThreshold = _Cutoff;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature_local _ EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
				float4 VizUV : TEXCOORD2;
				float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord4.xy = v.texcoord0.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

			#ifdef EDITOR_VISUALIZATION
				float2 VizUV = 0;
				float4 LightCoord = 0;
				UnityEditorVizData(v.vertex.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
				o.VizUV = float4(VizUV, 0, 0);
				o.LightCoord = LightCoord;
			#endif

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = o.clipPos;
				o.shadowCoord = GetShadowCoord( vertexInput );
			#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float4 VAR_AlbedoTexture267 = tex2DNode13;
				float4 VAR_Albedo101 = ( _Color * VAR_AlbedoTexture267 );
				float4 VAR_Albedo18_g498 = VAR_Albedo101;
				float3 hsvTorgb9_g498 = RGBToHSV( VAR_Albedo18_g498.rgb );
				float3 hsvTorgb13_g498 = HSVToRGB( float3(( ( ( IN.ase_color.g - 0.5 ) * _ColorVariation ) + _Hue + hsvTorgb9_g498 ).x,( hsvTorgb9_g498.y * _Saturation ),( hsvTorgb9_g498.z * _Value )) );
				float4 lerpResult19_g498 = lerp( float4( hsvTorgb13_g498 , 0.0 ) , VAR_Albedo18_g498 , (float)_ColorShifting);
				float4 OUT_Albedo254 = lerpResult19_g498;
				
				float OUT_Alpha46 = tex2DNode13.a;
				
				
				float3 Albedo = OUT_Albedo254.rgb;
				float3 Emission = 0;
				float Alpha = OUT_Alpha46;
				float AlphaClipThreshold = _Cutoff;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
			#ifdef EDITOR_VISUALIZATION
				metaInput.VizUV = IN.VizUV.xy;
				metaInput.LightCoord = IN.LightCoord;
			#endif
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				float4 worldTangent : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _BumpMap;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 tangentWS = float4(TransformObjectToWorldDir( v.ase_tangent.xyz), v.ase_tangent.w);
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_BumpMap = IN.ase_texcoord4.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float3 unpack53 = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
				unpack53.z = lerp( 1, unpack53.z, saturate(_BumpScale) );
				float3 OUT_Normal255 = unpack53;
				
				float2 uv_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float OUT_Alpha46 = tex2DNode13.a;
				
				float3 Normal = OUT_Normal255;
				float Alpha = OUT_Alpha46;
				float AlphaClipThreshold = _Cutoff;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				
				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					return half4(packedNormalWS, 0.0);
				#else
					
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif

					return half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif
			}
			ENDHLSL
		}

		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }
        
			Cull Off

			HLSLPROGRAM
        
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

        
			#pragma only_renderers d3d11 glcore gles gles3 ps5 
			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			

			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 uv_MainTex = IN.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float OUT_Alpha46 = tex2DNode13.a;
				
				surfaceDescription.Alpha = OUT_Alpha46;
				surfaceDescription.AlphaClipThreshold = _Cutoff;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

			ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }
        
			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSLUCENCY_ASE 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110


			#pragma only_renderers d3d11 glcore gles gles3 ps5 
			#pragma vertex vert
			#pragma fragment frag

        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY
			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			int _WindModeLeaves;
			float _GlobalWindInfluence;
			float _GlobalTurbulenceInfluence;
			float _ColorVariation;
			half _Hue;
			float _Saturation;
			float _Value;
			int _ColorShifting;
			half _BumpScale;
			float _Metallic;
			float _Glossiness;
			half _OcclusionStrength;
			half _Cutoff;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _WindStrength;
			float _RandomWindOffset;
			float _WindPulse;
			float _WindDirection;
			float _WindTurbulence;
			sampler2D _MainTex;


			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 If252_g499( int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None )
			{
				float3 Output = m_None;
				if(m_Switch == 0){Output = m_Leaves;}
				if(m_Switch == 1){Output = m_Palm;}
				if(m_Switch == 2){Output = m_Grass;}
				if(m_Switch == 3){Output = m_None;}
				return Output;
			}
			

        
			float4 _SelectionID;

        
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				int m_Switch252_g499 = _WindModeLeaves;
				float3 VAR_VertexPosition21_g499 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break109_g499 = VAR_VertexPosition21_g499;
				float VAR_WindStrength43_g499 = ( _WindStrength * _GlobalWindInfluence );
				float4 transform37_g499 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult38_g499 = (float2(transform37_g499.x , transform37_g499.z));
				float dotResult2_g500 = dot( appendResult38_g499 , float2( 12.9898,78.233 ) );
				float lerpResult8_g500 = lerp( 0.8 , ( ( _RandomWindOffset / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2_g500 ) * 43758.55 ) ));
				float VAR_RandomTime16_g499 = ( ( _TimeParameters.x * 0.05 ) * lerpResult8_g500 );
				float FUNC_Turbulence36_g499 = ( sin( ( ( VAR_RandomTime16_g499 * 40.0 ) - ( VAR_VertexPosition21_g499.z / 15.0 ) ) ) * 0.5 );
				float VAR_WindPulse274_g499 = _WindPulse;
				float FUNC_Angle73_g499 = ( VAR_WindStrength43_g499 * ( 1.0 + sin( ( ( ( ( VAR_RandomTime16_g499 * 2.0 ) + FUNC_Turbulence36_g499 ) - ( VAR_VertexPosition21_g499.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * 0.2 * VAR_WindPulse274_g499 );
				float VAR_SinA80_g499 = sin( FUNC_Angle73_g499 );
				float VAR_CosA78_g499 = cos( FUNC_Angle73_g499 );
				float _WindDirection164_g499 = _WindDirection;
				float2 localDirectionalEquation164_g499 = DirectionalEquation( _WindDirection164_g499 );
				float2 break165_g499 = localDirectionalEquation164_g499;
				float VAR_xLerp83_g499 = break165_g499.x;
				float lerpResult118_g499 = lerp( break109_g499.x , ( ( break109_g499.y * VAR_SinA80_g499 ) + ( break109_g499.x * VAR_CosA78_g499 ) ) , VAR_xLerp83_g499);
				float3 break98_g499 = VAR_VertexPosition21_g499;
				float3 break105_g499 = VAR_VertexPosition21_g499;
				float VAR_zLerp95_g499 = break165_g499.y;
				float lerpResult120_g499 = lerp( break105_g499.z , ( ( break105_g499.y * VAR_SinA80_g499 ) + ( break105_g499.z * VAR_CosA78_g499 ) ) , VAR_zLerp95_g499);
				float3 appendResult122_g499 = (float3(lerpResult118_g499 , ( ( break98_g499.y * VAR_CosA78_g499 ) - ( break98_g499.z * VAR_SinA80_g499 ) ) , lerpResult120_g499));
				float3 FUNC_vertexPos123_g499 = appendResult122_g499;
				float3 break236_g499 = FUNC_vertexPos123_g499;
				half FUNC_SinFunction195_g499 = sin( ( ( VAR_RandomTime16_g499 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + FUNC_Turbulence36_g499 + ( VAR_VertexPosition21_g499.z / 2.0 ) ) );
				float VAR_GlobalWindTurbulence194_g499 = ( _WindTurbulence * _GlobalTurbulenceInfluence );
				float3 appendResult237_g499 = (float3(break236_g499.x , ( break236_g499.y + ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) ) , break236_g499.z));
				float3 OUT_Leafs_Standalone244_g499 = appendResult237_g499;
				float3 m_Leaves252_g499 = OUT_Leafs_Standalone244_g499;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 appendResult234_g499 = (float3(( normalizedWorldNormal.x * v.ase_color.g ) , ( normalizedWorldNormal.y / v.ase_color.r ) , ( normalizedWorldNormal.z * v.ase_color.g )));
				float3 OUT_Palm_Standalone243_g499 = ( ( ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) * VAR_GlobalWindTurbulence194_g499 ) * appendResult234_g499 ) + FUNC_vertexPos123_g499 );
				float3 m_Palm252_g499 = OUT_Palm_Standalone243_g499;
				float3 break221_g499 = FUNC_vertexPos123_g499;
				float temp_output_202_0_g499 = ( FUNC_SinFunction195_g499 * v.ase_color.b * ( FUNC_Angle73_g499 + ( VAR_WindStrength43_g499 / 200.0 ) ) );
				float lerpResult203_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_xLerp83_g499);
				float lerpResult196_g499 = lerp( 0.0 , temp_output_202_0_g499 , VAR_zLerp95_g499);
				float3 appendResult197_g499 = (float3(( break221_g499.x + lerpResult203_g499 ) , break221_g499.y , ( break221_g499.z + lerpResult196_g499 )));
				float3 OUT_Grass_Standalone245_g499 = appendResult197_g499;
				float3 m_Grass252_g499 = OUT_Grass_Standalone245_g499;
				float3 m_None252_g499 = FUNC_vertexPos123_g499;
				float3 localIf252_g499 = If252_g499( m_Switch252_g499 , m_Leaves252_g499 , m_Palm252_g499 , m_Grass252_g499 , m_None252_g499 );
				float3 OUT_Leafs262_g499 = localIf252_g499;
				float3 temp_output_5_0_g499 = mul( GetWorldToObjectMatrix(), float4( OUT_Leafs262_g499 , 0.0 ) ).xyz;
				float3 OUT_VertexPos261 = temp_output_5_0_g499;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = OUT_VertexPos261;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 uv_MainTex = IN.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float OUT_Alpha46 = tex2DNode13.a;
				
				surfaceDescription.Alpha = OUT_Alpha46;
				surfaceDescription.AlphaClipThreshold = _Cutoff;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;
				
				return outColor;
			}
        
			ENDHLSL
        }
		
	}
	
	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;1;-1855.988,-2014.438;Inherit;False;1482.458;558.947;;7;46;101;14;13;11;10;267;Albedo;1,0.1254902,0.1254902,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;265;791.8203,-575.662;Inherit;False;757.7145;754.4375;;8;222;257;256;9;258;8;357;30;Output;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;262;-1903.316,788.9074;Inherit;False;602.7547;158.8317;;2;261;259;VertexPos;0,1,0.09019608,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;5;-1871.949,-827.6025;Inherit;False;1742.909;463.5325;;8;53;47;48;37;255;392;391;393;Normal;0,0.627451,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;4;-1865.419,-230.3994;Inherit;False;1068.058;483.6455;;5;50;26;266;268;269;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;3;-1887.419,352.0771;Inherit;False;789.6466;355.3238;;4;44;41;31;24;AO;0.5372549,0.3568628,0.3568628,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2;-1846.601,-1273.244;Inherit;False;1068.96;320.8381;;3;254;364;102;Color Settings;1,0.1254902,0.1254902,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;255;-436.7896,-750.7148;Inherit;False;OUT_Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-1350.784,466.1693;Inherit;False;OUT_AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;261;-1559.753,837.1774;Inherit;False;OUT_VertexPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;254;-1113.419,-1164.183;Inherit;False;OUT_Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;9;851.04,-313.8114;Inherit;False;50;OUT_Smoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;392;-868.3899,-571.1698;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;222;809.1625,-387.5882;Inherit;False;Property;_Metallic;Metallic;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-1046.058,-90.60033;Inherit;False;OUT_Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;357;913.8409,-241.9332;Inherit;False;44;OUT_AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;256;906.4799,-525.662;Inherit;False;254;OUT_Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;8;901.0649,-150.5836;Inherit;False;46;OUT_Alpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;393;-649.3899,-640.1698;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;258;877.3975,-70.94308;Inherit;False;261;OUT_VertexPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;30;889.184,93.30617;Half;False;Property;_Cutoff;Cutoff;2;0;Create;True;0;0;0;False;0;False;0.5;0.355;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;391;-1069.39,-526.1698;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;257;906.4799,-456.6621;Inherit;False;255;OUT_Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;364;-1542.544,-1165.123;Inherit;False;Mtree Color Shifting;5;;498;11c05beb300ec1e4a931d33c551c69f9;0;1;15;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-678.8424,-1666.567;Inherit;False;OUT_Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;259;-1852.316,842.3964;Inherit;False;Mtree Wind;14;;499;2ffbceb0d88c2164eb1d1c7b2010084d;7,269,1,281,0,272,0,255,1,278,0,280,0,282,0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1819.385,-1751.215;Float;True;Property;_MainTex;Albedo;1;0;Create;False;0;0;0;False;1;;False;None;c73e3c0146c597940a6b333e52ba39df;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;13;-1591.027,-1750.344;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1505.24,-1930.904;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;1;Header(Albedo Texture);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;267;-1232.125,-1749.935;Inherit;False;VAR_AlbedoTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.UnpackScaleNormalNode;53;-1207.502,-736.6102;Inherit;False;Tangent;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-740.5823,-1777.458;Inherit;False;VAR_Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;266;-1805.141,-82.6242;Inherit;False;267;VAR_AlbedoTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;37;-1821.949,-773.3027;Float;True;Property;_BumpMap;Normal Map;3;0;Create;False;0;0;0;False;1;Header(Normal Texture);False;None;a2b769acb5c411e438d48f983e921d37;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.BreakToComponentsNode;269;-1539.151,-83.62621;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-947.1429,-1770.289;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1579.589,58.21383;Inherit;False;Property;_Glossiness;Smoothness;13;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;-1570.949,-747.3025;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-1837.419,583.7173;Half;False;Property;_OcclusionStrength;AO strength;11;0;Create;False;0;0;0;False;1;Header(Other Settings);False;0.6;0.591;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;31;-1854.053,400.0771;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;102;-1818.028,-1161.422;Inherit;False;101;VAR_Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;-1532.565,470.5643;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;268;-1239.176,-85.27422;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1483.599,-551.3174;Half;False;Property;_BumpScale;Normal Strength;4;0;Create;False;0;0;0;False;0;False;1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;381;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;384;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;383;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;382;1279.325,-379.9919;Float;False;True;-1;2;UnityEditor.ShaderGraphLitGUI;0;12;Mtree/SRP/Leafs URP;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;2;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;0;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;1;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.916,False,;0;  Scattering;1,False,;0;  Direct;0.921,False,;0;  Ambient;0.158,False,;0;  Shadow;0.848,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;0;638123050950925978;LOD CrossFade;0;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;638123050970380931;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;0;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;False;True;False;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;385;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;388;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;2;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;386;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;2;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;394;1279.325,-299.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;387;1279.325,-379.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;395;1279.325,-299.9919;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;255;0;53;0
WireConnection;44;0;41;0
WireConnection;261;0;259;0
WireConnection;254;0;364;0
WireConnection;392;0;391;0
WireConnection;392;1;53;3
WireConnection;50;0;268;0
WireConnection;393;0;53;1
WireConnection;393;1;53;2
WireConnection;393;2;53;3
WireConnection;364;15;102;0
WireConnection;46;0;13;4
WireConnection;13;0;10;0
WireConnection;267;0;13;0
WireConnection;53;0;47;0
WireConnection;53;1;48;0
WireConnection;101;0;14;0
WireConnection;269;0;266;0
WireConnection;14;0;11;0
WireConnection;14;1;267;0
WireConnection;47;0;37;0
WireConnection;41;1;31;4
WireConnection;41;2;24;0
WireConnection;268;1;269;0
WireConnection;268;2;26;0
WireConnection;382;0;256;0
WireConnection;382;1;257;0
WireConnection;382;3;222;0
WireConnection;382;4;9;0
WireConnection;382;5;357;0
WireConnection;382;6;8;0
WireConnection;382;7;30;0
WireConnection;382;15;357;0
WireConnection;382;8;258;0
ASEEND*/
//CHKSM=7372D0813E0EFC34317E1AC6B0A73D86E7ADFCF5
