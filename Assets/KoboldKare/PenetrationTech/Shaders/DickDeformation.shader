// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DickDeformation"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_DickOrigin("DickOrigin", Vector) = (0,0,0,0)
		_DickForward("DickForward", Vector) = (0,1,0,0)
		_DecalColorMap("DecalColorMap", 2D) = "black" {}
		_MainTex("MainTex", 2D) = "white" {}
		_MaskMap("MaskMap", 2D) = "gray" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		_PullAmount("PullAmount", Range( 0 , 1)) = 0
		_HoleProgress("HoleProgress", Float) = 0
		_CumProgress("CumProgress", Float) = 0
		_SquishAmount("SquishAmount", Range( 0 , 1)) = 0
		_CumAmount("CumAmount", Range( 0 , 1)) = 0
		_BlendshapeMultiplier("BlendshapeMultiplier", Range( 0 , 100)) = 1
		_ModelScale("ModelScale", Float) = 1
		_HueBrightnessContrastSaturation("_HueBrightnessContrastSaturation", Vector) = (0,0,0,0)
		_DickRight("DickRight", Vector) = (1,0,0,0)
		_DickUp("DickUp", Vector) = (0,0,1,0)
		_OrificePosition("OrificePosition", Vector) = (0,0,0,0)
		_DickLength("DickLength", Range( 0.01 , 10)) = 1
		[ASEEnd]_OrificeNormal("OrificeNormal", Vector) = (0,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
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
		Cull Back
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 4.0

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
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord10 : TEXCOORD10;
				float4 ase_texcoord11 : TEXCOORD11;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_DecalColorMap);
			SAMPLER(sampler_DecalColorMap);
			TEXTURE2D(_BumpMap);
			SAMPLER(sampler_BumpMap);
			TEXTURE2D(_MaskMap);
			SAMPLER(sampler_MaskMap);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BumpMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MaskMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g282( float4 hsbc, float4 startColor )
			{
				    float _Hue = 360 * hsbc.r;
				    float _Brightness = hsbc.g * 2 - 1;
				    float _Contrast = hsbc.b * 2;
				    float _Saturation = hsbc.a * 2;
				 
				    float4 outputColor = startColor;
				    float angle = radians(_Hue);
				    float3 k = float3(0.57735, 0.57735, 0.57735);
				    float cosAngle = cos(angle);
				    //Rodrigues' rotation formula
				    outputColor.rgb = saturate(outputColor.rgb * cosAngle + cross(k, outputColor.rgb) * sin(angle) + k * dot(k, outputColor.rgb) * (1 - cosAngle));
				    outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast) + 0.5f;
				    outputColor.rgb = outputColor.rgb + _Brightness;        
				    float3 intensity = dot(outputColor.rgb, float3(0.299,0.587,0.114));
				    outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
				    return saturate(outputColor);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.texcoord1.xyzw.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord9 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord10 = v.vertex;
				o.ase_texcoord11 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult354;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;

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
				o.texcoord = v.texcoord;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord3 = v.ase_texcoord3;
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
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_HueBrightnessContrastSaturation);
				float4 hsbc1_g282 = _HueBrightnessContrastSaturation_Instance;
				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float4 startColor1_g282 = tex2DNode20;
				float4 localMyCustomExpression1_g282 = MyCustomExpression1_g282( hsbc1_g282 , startColor1_g282 );
				float2 texCoord310 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode309 = SAMPLE_TEXTURE2D_LOD( _DecalColorMap, sampler_DecalColorMap, texCoord310, 0.0 );
				float4 lerpResult308 = lerp( localMyCustomExpression1_g282 , tex2DNode309 , tex2DNode309.a);
				
				float4 _BumpMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BumpMap_ST);
				float2 uv_BumpMap = IN.ase_texcoord7.xy * _BumpMap_ST_Instance.xy + _BumpMap_ST_Instance.zw;
				
				float4 _MaskMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MaskMap_ST);
				float2 uv_MaskMap = IN.ase_texcoord7.xy * _MaskMap_ST_Instance.xy + _MaskMap_ST_Instance.zw;
				float4 tex2DNode21 = SAMPLE_TEXTURE2D( _MaskMap, sampler_MaskMap, uv_MaskMap );
				
				float lerpResult311 = lerp( tex2DNode21.a , 0.9 , tex2DNode309.a);
				
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord9.x ) + ( temp_output_95_1 * IN.ase_texcoord9.y ) + ( temp_output_95_2 * IN.ase_texcoord9.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord10.xyz , ( IN.ase_texcoord10.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord11.x ) + ( temp_output_95_1 * IN.ase_texcoord11.y ) + ( temp_output_95_2 * IN.ase_texcoord11.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				float3 Albedo = lerpResult308.rgb;
				float3 Normal = UnpackNormalScale( SAMPLE_TEXTURE2D( _BumpMap, sampler_BumpMap, uv_BumpMap ), 1.0f );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = tex2DNode21.r;
				float Smoothness = lerpResult311;
				float Occlusion = (0.9 + (tex2DNode21.g - 0.0) * (1.0 - 0.9) / (1.0 - 0.0));
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

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

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

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
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
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

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				float3 ase_normal : NORMAL;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			

			float3 _LightDirection;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.ase_texcoord1.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord4 = v.vertex;
				o.ase_texcoord5 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult354;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord3 = v.ase_texcoord3;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
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

				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord3.x ) + ( temp_output_95_1 * IN.ase_texcoord3.y ) + ( temp_output_95_2 * IN.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord4.xyz , ( IN.ase_texcoord4.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord5.x ) + ( temp_output_95_1 * IN.ase_texcoord5.y ) + ( temp_output_95_2 * IN.ase_texcoord5.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				float3 ase_normal : NORMAL;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.ase_texcoord1.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord4 = v.vertex;
				o.ase_texcoord5 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult354;
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord3 = v.ase_texcoord3;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
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

				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord3.x ) + ( temp_output_95_1 * IN.ase_texcoord3.y ) + ( temp_output_95_2 * IN.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord4.xyz , ( IN.ase_texcoord4.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord5.x ) + ( temp_output_95_1 * IN.ase_texcoord5.y ) + ( temp_output_95_2 * IN.ase_texcoord5.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
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
				float4 ase_texcoord3 : TEXCOORD3;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_DecalColorMap);
			SAMPLER(sampler_DecalColorMap);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g282( float4 hsbc, float4 startColor )
			{
				    float _Hue = 360 * hsbc.r;
				    float _Brightness = hsbc.g * 2 - 1;
				    float _Contrast = hsbc.b * 2;
				    float _Saturation = hsbc.a * 2;
				 
				    float4 outputColor = startColor;
				    float angle = radians(_Hue);
				    float3 k = float3(0.57735, 0.57735, 0.57735);
				    float cosAngle = cos(angle);
				    //Rodrigues' rotation formula
				    outputColor.rgb = saturate(outputColor.rgb * cosAngle + cross(k, outputColor.rgb) * sin(angle) + k * dot(k, outputColor.rgb) * (1 - cosAngle));
				    outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast) + 0.5f;
				    outputColor.rgb = outputColor.rgb + _Brightness;        
				    float3 intensity = dot(outputColor.rgb, float3(0.299,0.587,0.114));
				    outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
				    return saturate(outputColor);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.texcoord2.x ) + ( temp_output_95_1 * v.texcoord2.y ) + ( temp_output_95_2 * v.texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.texcoord1.w ) + ( temp_output_95_1 * v.texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.texcoord1;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord4 = v.texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord5 = v.vertex;
				o.ase_texcoord6 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult354;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
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
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
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
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord3 = v.ase_texcoord3;
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
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_HueBrightnessContrastSaturation);
				float4 hsbc1_g282 = _HueBrightnessContrastSaturation_Instance;
				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float4 startColor1_g282 = tex2DNode20;
				float4 localMyCustomExpression1_g282 = MyCustomExpression1_g282( hsbc1_g282 , startColor1_g282 );
				float2 texCoord310 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode309 = SAMPLE_TEXTURE2D_LOD( _DecalColorMap, sampler_DecalColorMap, texCoord310, 0.0 );
				float4 lerpResult308 = lerp( localMyCustomExpression1_g282 , tex2DNode309 , tex2DNode309.a);
				
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord4.x ) + ( temp_output_95_1 * IN.ase_texcoord4.y ) + ( temp_output_95_2 * IN.ase_texcoord4.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord5.xyz , ( IN.ase_texcoord5.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord6.x ) + ( temp_output_95_1 * IN.ase_texcoord6.y ) + ( temp_output_95_2 * IN.ase_texcoord6.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				
				float3 Albedo = lerpResult308.rgb;
				float3 Emission = 0;
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				float4 ase_texcoord3 : TEXCOORD3;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_DecalColorMap);
			SAMPLER(sampler_DecalColorMap);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g282( float4 hsbc, float4 startColor )
			{
				    float _Hue = 360 * hsbc.r;
				    float _Brightness = hsbc.g * 2 - 1;
				    float _Contrast = hsbc.b * 2;
				    float _Saturation = hsbc.a * 2;
				 
				    float4 outputColor = startColor;
				    float angle = radians(_Hue);
				    float3 k = float3(0.57735, 0.57735, 0.57735);
				    float cosAngle = cos(angle);
				    //Rodrigues' rotation formula
				    outputColor.rgb = saturate(outputColor.rgb * cosAngle + cross(k, outputColor.rgb) * sin(angle) + k * dot(k, outputColor.rgb) * (1 - cosAngle));
				    outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast) + 0.5f;
				    outputColor.rgb = outputColor.rgb + _Brightness;        
				    float3 intensity = dot(outputColor.rgb, float3(0.299,0.587,0.114));
				    outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
				    return saturate(outputColor);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.ase_texcoord1.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.ase_texcoord1;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord4 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord5 = v.vertex;
				o.ase_texcoord6 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult354;

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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord3 = v.ase_texcoord3;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
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

			half4 frag(VertexOutput IN  ) : SV_TARGET
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_HueBrightnessContrastSaturation);
				float4 hsbc1_g282 = _HueBrightnessContrastSaturation_Instance;
				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float4 startColor1_g282 = tex2DNode20;
				float4 localMyCustomExpression1_g282 = MyCustomExpression1_g282( hsbc1_g282 , startColor1_g282 );
				float2 texCoord310 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode309 = SAMPLE_TEXTURE2D_LOD( _DecalColorMap, sampler_DecalColorMap, texCoord310, 0.0 );
				float4 lerpResult308 = lerp( localMyCustomExpression1_g282 , tex2DNode309 , tex2DNode309.a);
				
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord4.x ) + ( temp_output_95_1 * IN.ase_texcoord4.y ) + ( temp_output_95_2 * IN.ase_texcoord4.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord5.xyz , ( IN.ase_texcoord5.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord6.x ) + ( temp_output_95_1 * IN.ase_texcoord6.y ) + ( temp_output_95_2 * IN.ase_texcoord6.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				
				float3 Albedo = lerpResult308.rgb;
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				float4 ase_texcoord3 : TEXCOORD3;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.ase_texcoord1.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord4 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord5 = v.vertex;
				o.ase_texcoord6 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult354;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;

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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord3 = v.ase_texcoord3;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
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

				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord3.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord4.x ) + ( temp_output_95_1 * IN.ase_texcoord4.y ) + ( temp_output_95_2 * IN.ase_texcoord4.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord5.xyz , ( IN.ase_texcoord5.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord6.x ) + ( temp_output_95_1 * IN.ase_texcoord6.y ) + ( temp_output_95_2 * IN.ase_texcoord6.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;
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
				
				return float4(PackNormalOctRectEncode(TransformWorldToViewDir(IN.worldNormal, true)), 0.0, 0.0);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100500
			#define ASE_USING_SAMPLING_MACROS 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _GBUFFER_NORMALS_OCT
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_POSITION
			#pragma multi_compile_instancing


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord10 : TEXCOORD10;
				float4 ase_texcoord11 : TEXCOORD11;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _OrificePosition;
			float3 _OrificeNormal;
			float3 _DickUp;
			float3 _DickRight;
			float _HoleProgress;
			float _CumAmount;
			float _DickLength;
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
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_DecalColorMap);
			SAMPLER(sampler_DecalColorMap);
			UNITY_INSTANCING_BUFFER_START(NaelstrofDickDeformation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickForward)
				UNITY_DEFINE_INSTANCED_PROP(float3, _DickOrigin)
				UNITY_DEFINE_INSTANCED_PROP(float, _ModelScale)
				UNITY_DEFINE_INSTANCED_PROP(float, _BlendshapeMultiplier)
				UNITY_DEFINE_INSTANCED_PROP(float, _SquishAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _PullAmount)
				UNITY_DEFINE_INSTANCED_PROP(float, _CumProgress)
			UNITY_INSTANCING_BUFFER_END(NaelstrofDickDeformation)


			float3 MyCustomExpression20_g303( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g282( float4 hsbc, float4 startColor )
			{
				    float _Hue = 360 * hsbc.r;
				    float _Brightness = hsbc.g * 2 - 1;
				    float _Contrast = hsbc.b * 2;
				    float _Saturation = hsbc.a * 2;
				 
				    float4 outputColor = startColor;
				    float angle = radians(_Hue);
				    float3 k = float3(0.57735, 0.57735, 0.57735);
				    float cosAngle = cos(angle);
				    //Rodrigues' rotation formula
				    outputColor.rgb = saturate(outputColor.rgb * cosAngle + cross(k, outputColor.rgb) * sin(angle) + k * dot(k, outputColor.rgb) * (1 - cosAngle));
				    outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast) + 0.5f;
				    outputColor.rgb = outputColor.rgb + _Brightness;        
				    float3 intensity = dot(outputColor.rgb, float3(0.299,0.587,0.114));
				    outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
				    return saturate(outputColor);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float3 temp_output_42_0_g299 = DickForward219;
				float3 temp_output_11_0_g300 = temp_output_42_0_g299;
				float3 worldToObj2_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificePosition, 1 ) ).xyz;
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float3 temp_output_56_0_g299 = DickOrigin225;
				float3 temp_output_59_0_g299 = ( worldToObj2_g299 - temp_output_56_0_g299 );
				float temp_output_3_0_g299 = length( temp_output_59_0_g299 );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float3 normalizeResult27_g8 = normalize( v.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * v.ase_texcoord2.x ) + ( temp_output_95_1 * v.ase_texcoord2.y ) + ( temp_output_95_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier_Instance );
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( v.vertex.xyz , ( v.vertex.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * v.ase_texcoord3.x ) + ( temp_output_95_1 * v.ase_texcoord3.y ) + ( temp_output_95_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float temp_output_233_0 = ( _ModelScale_Instance * 0.05 );
				float temp_output_119_0 = ( 1.0 - saturate( ( SizeScaler190 * ( abs( dotResult30 ) - temp_output_233_0 ) ) ) );
				float _CumProgress_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_CumProgress);
				float dotResult164 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _CumProgress_Instance ) ) ) );
				float3 CumDelta67 = ( ( ( temp_output_95_0 * v.texcoord1.xyzw.w ) + ( temp_output_95_1 * v.ase_texcoord2.w ) + ( temp_output_95_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier_Instance );
				float3 temp_output_43_0 = ( ( temp_output_119_0 * SquishDelta45 * _SquishAmount_Instance ) + ( temp_output_119_0 * PullDelta58 * _PullAmount_Instance ) + ( ( 1.0 - saturate( ( ( abs( dotResult164 ) - temp_output_233_0 ) * SizeScaler190 ) ) ) * CumDelta67 * _CumAmount ) + v.vertex.xyz );
				float3 temp_output_58_0_g299 = ( temp_output_43_0 - temp_output_56_0_g299 );
				float3 temp_output_14_0_g300 = ( ( -temp_output_11_0_g300 * temp_output_3_0_g299 ) + temp_output_58_0_g299 );
				float dotResult22_g300 = dot( temp_output_11_0_g300 , temp_output_14_0_g300 );
				float3 worldToObjDir14_g299 = mul( GetWorldToObjectMatrix(), float4( _OrificeNormal, 0 ) ).xyz;
				float3 normalizeResult27_g301 = normalize( -worldToObjDir14_g299 );
				float3 temp_output_7_0_g300 = normalizeResult27_g301;
				float3 temp_output_43_0_g299 = _DickUp;
				float3 temp_output_4_0_g300 = temp_output_43_0_g299;
				float dotResult23_g300 = dot( temp_output_4_0_g300 , temp_output_14_0_g300 );
				float3 normalizeResult31_g301 = normalize( temp_output_4_0_g300 );
				float3 normalizeResult29_g301 = normalize( cross( normalizeResult27_g301 , normalizeResult31_g301 ) );
				float3 temp_output_7_1_g300 = cross( normalizeResult29_g301 , normalizeResult27_g301 );
				float3 temp_output_44_0_g299 = _DickRight;
				float3 temp_output_20_0_g300 = temp_output_44_0_g299;
				float dotResult21_g300 = dot( temp_output_20_0_g300 , temp_output_14_0_g300 );
				float3 temp_output_7_2_g300 = normalizeResult29_g301;
				float3 temp_output_2_0_g302 = temp_output_58_0_g299;
				float3 temp_output_3_0_g302 = temp_output_42_0_g299;
				float dotResult6_g302 = dot( temp_output_2_0_g302 , temp_output_3_0_g302 );
				float temp_output_20_0_g302 = ( dotResult6_g302 / temp_output_3_0_g299 );
				float temp_output_26_0_g306 = temp_output_20_0_g302;
				float temp_output_19_0_g306 = ( 1.0 - temp_output_26_0_g306 );
				float3 temp_output_8_0_g302 = float3( 0,0,0 );
				float3 temp_output_9_0_g302 = ( temp_output_42_0_g299 * temp_output_3_0_g299 * 0.5 );
				float3 temp_output_10_0_g302 = ( temp_output_59_0_g299 + ( worldToObjDir14_g299 * 0.5 * temp_output_3_0_g299 ) );
				float3 temp_output_11_0_g302 = temp_output_59_0_g299;
				float temp_output_1_0_g304 = temp_output_20_0_g302;
				float temp_output_8_0_g304 = ( 1.0 - temp_output_1_0_g304 );
				float3 temp_output_3_0_g304 = temp_output_9_0_g302;
				float3 temp_output_4_0_g304 = temp_output_10_0_g302;
				float3 temp_output_7_0_g303 = ( ( 3.0 * temp_output_8_0_g304 * temp_output_8_0_g304 * ( temp_output_3_0_g304 - temp_output_8_0_g302 ) ) + ( 6.0 * temp_output_8_0_g304 * temp_output_1_0_g304 * ( temp_output_4_0_g304 - temp_output_3_0_g304 ) ) + ( 3.0 * temp_output_1_0_g304 * temp_output_1_0_g304 * ( temp_output_11_0_g302 - temp_output_4_0_g304 ) ) );
				float3 bezierDerivitive20_g303 = temp_output_7_0_g303;
				float3 forward20_g303 = temp_output_3_0_g302;
				float3 temp_output_4_0_g302 = temp_output_43_0_g299;
				float3 up20_g303 = temp_output_4_0_g302;
				float3 localMyCustomExpression20_g303 = MyCustomExpression20_g303( bezierDerivitive20_g303 , forward20_g303 , up20_g303 );
				float3 normalizeResult27_g305 = normalize( localMyCustomExpression20_g303 );
				float3 normalizeResult24_g303 = normalize( cross( temp_output_7_0_g303 , localMyCustomExpression20_g303 ) );
				float3 normalizeResult31_g305 = normalize( normalizeResult24_g303 );
				float3 normalizeResult29_g305 = normalize( cross( normalizeResult27_g305 , normalizeResult31_g305 ) );
				float3 temp_output_41_22_g302 = cross( normalizeResult29_g305 , normalizeResult27_g305 );
				float3 temp_output_5_0_g302 = temp_output_44_0_g299;
				float dotResult15_g302 = dot( temp_output_2_0_g302 , temp_output_5_0_g302 );
				float3 temp_output_41_0_g302 = normalizeResult27_g305;
				float dotResult18_g302 = dot( temp_output_2_0_g302 , temp_output_4_0_g302 );
				float dotResult17_g299 = dot( temp_output_58_0_g299 , temp_output_42_0_g299 );
				float temp_output_31_0_g299 = saturate( sign( ( temp_output_3_0_g299 - dotResult17_g299 ) ) );
				float3 lerpResult36_g299 = lerp( ( ( dotResult22_g300 * temp_output_7_0_g300 ) + ( dotResult23_g300 * temp_output_7_1_g300 ) + ( dotResult21_g300 * temp_output_7_2_g300 ) + temp_output_59_0_g299 ) , ( ( ( temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_19_0_g306 * temp_output_8_0_g302 ) + ( temp_output_19_0_g306 * temp_output_19_0_g306 * 3.0 * temp_output_26_0_g306 * temp_output_9_0_g302 ) + ( 3.0 * temp_output_19_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_10_0_g302 ) + ( temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_26_0_g306 * temp_output_11_0_g302 ) ) + ( temp_output_41_22_g302 * dotResult15_g302 ) + ( temp_output_41_0_g302 * dotResult18_g302 ) ) , temp_output_31_0_g299);
				float temp_output_35_0_g299 = saturate( ( ( temp_output_3_0_g299 - _DickLength ) * 8.0 ) );
				float3 lerpResult38_g299 = lerp( lerpResult36_g299 , temp_output_58_0_g299 , temp_output_35_0_g299);
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 lerpResult338 = lerp( ( lerpResult38_g299 + temp_output_56_0_g299 ) , temp_output_43_0 , temp_output_252_0);
				
				float3 temp_output_48_0_g299 = v.ase_normal;
				float3 temp_output_24_0_g300 = temp_output_48_0_g299;
				float dotResult25_g300 = dot( temp_output_11_0_g300 , temp_output_24_0_g300 );
				float dotResult26_g300 = dot( temp_output_4_0_g300 , temp_output_24_0_g300 );
				float dotResult27_g300 = dot( temp_output_20_0_g300 , temp_output_24_0_g300 );
				float3 normalizeResult33_g300 = normalize( ( ( dotResult25_g300 * temp_output_7_0_g300 ) + ( dotResult26_g300 * temp_output_7_1_g300 ) + ( dotResult27_g300 * temp_output_7_2_g300 ) ) );
				float3 temp_output_21_0_g302 = temp_output_48_0_g299;
				float dotResult23_g302 = dot( temp_output_21_0_g302 , temp_output_3_0_g302 );
				float dotResult24_g302 = dot( temp_output_21_0_g302 , temp_output_4_0_g302 );
				float dotResult25_g302 = dot( temp_output_21_0_g302 , temp_output_5_0_g302 );
				float3 normalizeResult31_g302 = normalize( ( ( normalizeResult29_g305 * dotResult23_g302 ) + ( temp_output_41_0_g302 * dotResult24_g302 ) + ( temp_output_41_22_g302 * dotResult25_g302 ) ) );
				float3 lerpResult37_g299 = lerp( normalizeResult33_g300 , normalizeResult31_g302 , temp_output_31_0_g299);
				float3 lerpResult39_g299 = lerp( lerpResult37_g299 , temp_output_48_0_g299 , temp_output_35_0_g299);
				float3 lerpResult354 = lerp( lerpResult39_g299 , v.ase_normal , temp_output_252_0);
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord9 = v.ase_texcoord2;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord10 = v.vertex;
				o.ase_texcoord11 = v.ase_texcoord3;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult338;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult354;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;

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
				o.texcoord = v.texcoord;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord3 = v.ase_texcoord3;
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
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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
			FragmentOutput frag ( VertexOutput IN 
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_HueBrightnessContrastSaturation);
				float4 hsbc1_g282 = _HueBrightnessContrastSaturation_Instance;
				float4 _MainTex_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_MainTex_ST);
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST_Instance.xy + _MainTex_ST_Instance.zw;
				float4 tex2DNode20 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float4 startColor1_g282 = tex2DNode20;
				float4 localMyCustomExpression1_g282 = MyCustomExpression1_g282( hsbc1_g282 , startColor1_g282 );
				float2 texCoord310 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode309 = SAMPLE_TEXTURE2D_LOD( _DecalColorMap, sampler_DecalColorMap, texCoord310, 0.0 );
				float4 lerpResult308 = lerp( localMyCustomExpression1_g282 , tex2DNode309 , tex2DNode309.a);
				
				float3 normalizeResult27_g8 = normalize( IN.ase_normal );
				float3 temp_output_95_0 = normalizeResult27_g8;
				float3 normalizeResult31_g8 = normalize( IN.ase_tangent.xyz );
				float3 normalizeResult29_g8 = normalize( cross( normalizeResult27_g8 , normalizeResult31_g8 ) );
				float3 temp_output_95_1 = cross( normalizeResult29_g8 , normalizeResult27_g8 );
				float3 temp_output_95_2 = normalizeResult29_g8;
				float _BlendshapeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_BlendshapeMultiplier);
				float3 SquishDelta45 = ( ( ( temp_output_95_0 * IN.ase_texcoord9.x ) + ( temp_output_95_1 * IN.ase_texcoord9.y ) + ( temp_output_95_2 * IN.ase_texcoord9.z ) ) * _BlendshapeMultiplier_Instance );
				float temp_output_252_0 = ( 1.0 - saturate( ( length( SquishDelta45 ) * 10000.0 ) ) );
				float3 _DickForward_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickForward);
				float3 DickForward219 = _DickForward_Instance;
				float _SquishAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_SquishAmount);
				float3 lerpResult242 = lerp( IN.ase_texcoord10.xyz , ( IN.ase_texcoord10.xyz + SquishDelta45 ) , _SquishAmount_Instance);
				float3 PullDelta58 = ( ( ( temp_output_95_0 * IN.ase_texcoord11.x ) + ( temp_output_95_1 * IN.ase_texcoord11.y ) + ( temp_output_95_2 * IN.ase_texcoord11.z ) ) * _BlendshapeMultiplier_Instance );
				float _PullAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_PullAmount);
				float3 lerpResult245 = lerp( lerpResult242 , ( lerpResult242 + PullDelta58 ) , _PullAmount_Instance);
				float3 _DickOrigin_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_DickOrigin);
				float3 DickOrigin225 = _DickOrigin_Instance;
				float dotResult30 = dot( DickForward219 , ( lerpResult245 - ( DickOrigin225 + ( DickForward219 * _HoleProgress ) ) ) );
				float _ModelScale_Instance = UNITY_ACCESS_INSTANCED_PROP(NaelstrofDickDeformation,_ModelScale);
				float SizeScaler190 = ( 10.0 / _ModelScale_Instance );
				float lerpResult230 = lerp( tex2DNode20.a , temp_output_252_0 , saturate( ( dotResult30 * SizeScaler190 ) ));
				
				float3 Albedo = lerpResult308.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = lerpResult230;
				float AlphaClipThreshold = 0.99;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

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

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				BRDFData brdfData;
				InitializeBRDFData( Albedo, Metallic, Specular, Smoothness, Alpha, brdfData);
				half4 color;
				color.rgb = GlobalIllumination( brdfData, inputData.bakedGI, Occlusion, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

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
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
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
				
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb);
			}

			ENDHLSL
		}
		
	}
	/*ase_lod*/
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18909
242;235;1675;736;598.7559;279.1501;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;69;-2739.087,2263.13;Inherit;False;2983.238;1979.695;Get the blendshape deltas;25;45;67;58;99;100;101;98;44;56;65;60;50;51;53;62;11;12;13;59;57;17;95;94;93;313;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TangentVertexDataNode;94;-2635.746,2789.59;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;93;-2635.177,2613.508;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;95;-2358.914,2750.264;Inherit;False;Create Orthogonal Vector;-1;;8;83358ef05db30f04ba825a1be5f469d8;0;2;25;FLOAT3;1,0,0;False;26;FLOAT3;0,1,0;False;3;FLOAT3;0;FLOAT3;1;FLOAT3;2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;17;-1460.302,2592.438;Inherit;False;2;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-571.4123,2504.613;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-568.4446,2825.613;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-566.4446,2666.613;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;57;-1391.055,3149.451;Inherit;False;3;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;98;-2392.944,3758;Inherit;False;InstancedProperty;_BlendshapeMultiplier;BlendshapeMultiplier;11;0;Create;True;0;0;0;False;0;False;1;1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-331.0131,2690.836;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-164.7497,2771.363;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-637.3448,3213.279;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-639.3448,3372.279;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-642.3125,3051.278;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;2.463362,2765.314;Inherit;False;SquishDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-401.9133,3237.501;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;19;-4542.249,469.9885;Inherit;False;InstancedProperty;_DickForward;DickForward;1;0;Create;True;0;0;0;False;0;False;0,1,0;-0.04222661,0.07983255,0.9959134;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-227.9227,3341.627;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;243;-3734.268,312.6523;Inherit;False;45;SquishDelta;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;23;-3936.095,-256.006;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;219;-3948.28,487.6918;Inherit;False;DickForward;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;187;-2669.905,1184.666;Inherit;False;2473.416;942.0834;Cum Blendshape Calculation;16;83;82;81;164;163;162;161;160;167;165;166;168;169;192;222;227;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;244;-3502.328,120.1372;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-51.83869,3345.879;Inherit;False;PullDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-3559.859,706.8803;Inherit;False;InstancedProperty;_SquishAmount;SquishAmount;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;18;-4453.594,30.81464;Inherit;False;InstancedProperty;_DickOrigin;DickOrigin;0;0;Create;True;0;0;0;False;0;False;0,0,0;-0.0009217065,-0.07095625,0.06483459;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;160;-2518.169,1583.195;Inherit;False;InstancedProperty;_CumProgress;CumProgress;8;0;Create;True;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;222;-2515.975,1338.596;Inherit;False;219;DickForward;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;242;-3336.482,47.21044;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;221;-2905.079,-33.2694;Inherit;False;219;DickForward;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;247;-3620.97,471.0086;Inherit;False;58;PullDelta;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-2881.443,492.903;Inherit;False;Property;_HoleProgress;HoleProgress;7;0;Create;True;0;0;0;False;0;False;0;99;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;225;-3832.947,84.62608;Inherit;False;DickOrigin;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-2241.579,1560.815;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;227;-2522.322,1461.552;Inherit;False;225;DickOrigin;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-2587.047,477.9815;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;246;-3258.381,358.9415;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-3432.963,871.9771;Inherit;False;InstancedProperty;_PullAmount;PullAmount;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;228;-2910.101,344.8333;Inherit;False;225;DickOrigin;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;245;-3051.807,174.1791;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;195;-4353.269,-1057.346;Inherit;False;918.74;421.0372;This adjusts how sharply blendshapes get triggered;5;190;145;143;196;233;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-2433.052,354.8131;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;162;-2042.403,1553.078;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;-2285.921,53.3166;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;196;-4320.076,-981.6472;Inherit;False;Constant;_Sharpness;Sharpness;16;0;Create;True;0;0;0;False;0;False;10;0;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-4277.356,-858.3486;Inherit;False;InstancedProperty;_ModelScale;ModelScale;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;163;-1867.708,1515.516;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;164;-1706.604,1334.033;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;313;-1408.634,3618.123;Inherit;False;1;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;145;-3980.439,-954.0433;Inherit;False;2;0;FLOAT;10;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;30;-2069.459,-10.97945;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;190;-3776.292,-948.2942;Inherit;False;SizeScaler;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-657.5693,3797.319;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;166;-1505.424,1339.284;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;146;-1820.456,48.75362;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-659.5693,3956.319;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;233;-3902.713,-792.5862;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-662.537,3635.318;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;167;-1325.742,1313.885;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;159;-1680.05,169.6011;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;192;-1425.168,1573.861;Inherit;False;190;SizeScaler;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-422.1379,3821.542;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;194;-1541.584,11.43573;Inherit;False;190;SizeScaler;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;165;-1111.63,1406.876;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-267.1926,3806.033;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;237;-1305.295,176.5024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;314;-1724.224,-280.4567;Inherit;False;45;SquishDelta;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-87.21741,3801.863;Inherit;False;CumDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;121;-1147.161,225.4262;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;168;-925.3059,1469.345;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;119;-961.0616,301.0597;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-691.579,1572.19;Inherit;False;67;CumDelta;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LengthOpNode;253;-1480.818,-254.7121;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-750.6544,1729.235;Inherit;False;Property;_CumAmount;CumAmount;10;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;169;-756.3054,1399.582;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1092.839,418.5921;Inherit;False;45;SquishDelta;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-990.3749,698.7491;Inherit;False;58;PullDelta;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-1220.172,-257.1061;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10000;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;318;-522.2586,58.279;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-671.4666,671.0889;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-662.251,363.5226;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-406.6159,1383.721;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;251;-1072.415,-278.449;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;322;1.818591,91.27537;Inherit;False;Property;_DickRight;DickRight;14;0;Create;True;0;0;0;False;0;False;1,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;323;59.8186,247.2754;Inherit;False;Property;_DickUp;DickUp;15;0;Create;True;0;0;0;False;0;False;0,0,1;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;20;-543.1502,-1028.013;Inherit;True;Property;_MainTex;MainTex;3;0;Create;True;0;0;0;False;0;False;-1;None;b5aefc08227662043a0fb60a69dad8df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;326;174.0131,630.903;Inherit;False;Property;_OrificeNormal;OrificeNormal;18;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;324;23.76484,-63.15044;Inherit;False;Property;_DickLength;DickLength;17;0;Create;True;0;0;0;False;0;False;1;1;0.01;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-158.0437,-0.8708649;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;319;26.57915,17.13566;Inherit;False;219;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;325;165.241,430.4883;Inherit;False;Property;_OrificePosition;OrificePosition;16;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;299;-4.693151,-1307.942;Inherit;False;InstancedProperty;_HueBrightnessContrastSaturation;_HueBrightnessContrastSaturation;13;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.5019608,0.5019608,0.5019608;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;329;281.9426,-193.6075;Inherit;False;225;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;310;222.732,-1498.147;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;-1344.672,-108.9958;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;252;-906.3936,-281.077;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;301;364.3369,-907.5203;Inherit;False;HueShift;-1;;282;1952e423258605d4aaa526c67ba2eb7c;0;2;2;FLOAT4;0,0.5,0.5,0.5;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;378;483.2337,-55.2225;Inherit;False;TentacleDeform;-1;;299;eea2eec91f9bfe142bbaae3dfd71166e;0;9;56;FLOAT3;0,0,0;False;47;FLOAT3;0,0,0;False;48;FLOAT3;0,0,0;False;41;FLOAT;1;False;42;FLOAT3;0,0,1;False;43;FLOAT3;0,1,0;False;44;FLOAT3;1,0,0;False;45;FLOAT3;0,0,0;False;46;FLOAT3;0,1,0;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;309;655.9979,-1376.182;Inherit;True;Property;_DecalColorMap;DecalColorMap;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;355;650.1113,-213.197;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;40;-969.0652,-83.91145;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;22;-540.2421,-775.7159;Inherit;True;Property;_BumpMap;BumpMap;5;0;Create;True;0;0;0;False;0;False;-1;None;497fe4dc767712e409ed141a239bd919;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;311;863.6896,-666.2214;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;338;974.015,-134.246;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;354;1016.987,-320.5945;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;189;-2686.482,945.3115;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;21;-535.8345,-559.4559;Inherit;True;Property;_MaskMap;MaskMap;4;0;Create;True;0;0;0;False;0;False;-1;None;2f369d12fa0c3144b8fa464d880dec52;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;298;133.39,-498.5808;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.9;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;269;726.3049,-368.3584;Inherit;False;Constant;_AlphaClip;AlphaClip;14;0;Create;True;0;0;0;False;0;False;0.99;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;308;1037.399,-875.3406;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;230;-37.359,-302.8931;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;305;1303.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;306;1303.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;302;1303.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;307;1303.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;2;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;315;1302.694,-625.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;304;1303.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;303;1302.694,-685.3577;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Naelstrof/DickDeformation;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;4;0;False;True;2;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;0;  Early Z;0;Vertex Position,InvertActionOnDeselection;0;0;8;False;True;True;True;True;True;True;True;False;;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;316;1302.694,-685.3577;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;2;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;95;25;93;0
WireConnection;95;26;94;0
WireConnection;11;0;95;0
WireConnection;11;1;17;1
WireConnection;13;0;95;2
WireConnection;13;1;17;3
WireConnection;12;0;95;1
WireConnection;12;1;17;2
WireConnection;44;0;11;0
WireConnection;44;1;12;0
WireConnection;44;2;13;0
WireConnection;99;0;44;0
WireConnection;99;1;98;0
WireConnection;50;0;95;1
WireConnection;50;1;57;2
WireConnection;51;0;95;2
WireConnection;51;1;57;3
WireConnection;53;0;95;0
WireConnection;53;1;57;1
WireConnection;45;0;99;0
WireConnection;56;0;53;0
WireConnection;56;1;50;0
WireConnection;56;2;51;0
WireConnection;100;0;56;0
WireConnection;100;1;98;0
WireConnection;219;0;19;0
WireConnection;244;0;23;0
WireConnection;244;1;243;0
WireConnection;58;0;100;0
WireConnection;242;0;23;0
WireConnection;242;1;244;0
WireConnection;242;2;73;0
WireConnection;225;0;18;0
WireConnection;161;0;222;0
WireConnection;161;1;160;0
WireConnection;25;0;221;0
WireConnection;25;1;24;0
WireConnection;246;0;242;0
WireConnection;246;1;247;0
WireConnection;245;0;242;0
WireConnection;245;1;246;0
WireConnection;245;2;72;0
WireConnection;26;0;228;0
WireConnection;26;1;25;0
WireConnection;162;0;227;0
WireConnection;162;1;161;0
WireConnection;28;0;245;0
WireConnection;28;1;26;0
WireConnection;163;0;245;0
WireConnection;163;1;162;0
WireConnection;164;0;222;0
WireConnection;164;1;163;0
WireConnection;145;0;196;0
WireConnection;145;1;143;0
WireConnection;30;0;221;0
WireConnection;30;1;28;0
WireConnection;190;0;145;0
WireConnection;59;0;95;1
WireConnection;59;1;17;4
WireConnection;166;0;164;0
WireConnection;146;0;30;0
WireConnection;60;0;95;2
WireConnection;60;1;57;4
WireConnection;233;0;143;0
WireConnection;62;0;95;0
WireConnection;62;1;313;4
WireConnection;167;0;166;0
WireConnection;167;1;233;0
WireConnection;159;0;146;0
WireConnection;159;1;233;0
WireConnection;65;0;62;0
WireConnection;65;1;59;0
WireConnection;65;2;60;0
WireConnection;165;0;167;0
WireConnection;165;1;192;0
WireConnection;101;0;65;0
WireConnection;101;1;98;0
WireConnection;237;0;194;0
WireConnection;237;1;159;0
WireConnection;67;0;101;0
WireConnection;121;0;237;0
WireConnection;168;0;165;0
WireConnection;119;0;121;0
WireConnection;253;0;314;0
WireConnection;169;0;168;0
WireConnection;250;0;253;0
WireConnection;71;0;119;0
WireConnection;71;1;70;0
WireConnection;71;2;72;0
WireConnection;48;0;119;0
WireConnection;48;1;49;0
WireConnection;48;2;73;0
WireConnection;81;0;169;0
WireConnection;81;1;82;0
WireConnection;81;2;83;0
WireConnection;251;0;250;0
WireConnection;43;0;48;0
WireConnection;43;1;71;0
WireConnection;43;2;81;0
WireConnection;43;3;318;0
WireConnection;122;0;30;0
WireConnection;122;1;194;0
WireConnection;252;0;251;0
WireConnection;301;2;299;0
WireConnection;301;3;20;0
WireConnection;378;56;329;0
WireConnection;378;47;43;0
WireConnection;378;41;324;0
WireConnection;378;42;319;0
WireConnection;378;43;323;0
WireConnection;378;44;322;0
WireConnection;378;45;325;0
WireConnection;378;46;326;0
WireConnection;309;1;310;0
WireConnection;40;0;122;0
WireConnection;311;0;21;4
WireConnection;311;2;309;4
WireConnection;338;0;378;0
WireConnection;338;1;43;0
WireConnection;338;2;252;0
WireConnection;354;0;378;40
WireConnection;354;1;355;0
WireConnection;354;2;252;0
WireConnection;298;0;21;2
WireConnection;308;0;301;0
WireConnection;308;1;309;0
WireConnection;308;2;309;4
WireConnection;230;0;20;4
WireConnection;230;1;252;0
WireConnection;230;2;40;0
WireConnection;303;0;308;0
WireConnection;303;1;22;0
WireConnection;303;3;21;1
WireConnection;303;4;311;0
WireConnection;303;5;298;0
WireConnection;303;6;230;0
WireConnection;303;7;269;0
WireConnection;303;8;338;0
WireConnection;303;10;354;0
ASEEND*/
//CHKSM=6EE492612C55A16BF2EC168E7B8B9CF231510C38