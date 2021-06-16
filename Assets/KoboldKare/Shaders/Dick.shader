// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Dick"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_DickOrigin("DickOrigin", Vector) = (0,0,0,0)
		_DickForward("DickForward", Vector) = (0,0,1,0)
		_DickLength("DickLength", Float) = 1
		_DickUp("DickUp", Vector) = (0,1,0,0)
		_DecalColorMap("DecalColorMap", 2D) = "black" {}
		_DickRight("DickRight", Vector) = (1,0,0,0)
		_MainTex("MainTex", 2D) = "white" {}
		_OrifaceOutWorldPosition1("OrifaceOutWorldPosition1", Vector) = (0,0,0,0)
		_OrifaceOutWorldPosition3("OrifaceOutWorldPosition3", Vector) = (0,0,0,0)
		_OrifaceWorldPosition("OrifaceWorldPosition", Vector) = (0,0,0,0)
		_OrifaceOutWorldPosition2("OrifaceOutWorldPosition2", Vector) = (0,0,0,0)
		_MaskMap("MaskMap", 2D) = "gray" {}
		_OrifaceWorldNormal("OrifaceWorldNormal", Vector) = (0,0,0,0)
		_BumpMap("BumpMap", 2D) = "bump" {}
		_PenetrationDepth("PenetrationDepth", Range( -1 , 10)) = 0
		_BlendshapeMultiplier("BlendshapeMultiplier", Range( 0 , 100)) = 1
		_HueBrightnessContrastSaturation("_HueBrightnessContrastSaturation", Vector) = (0,0,0,0)
		_OrifaceLength("OrifaceLength", Float) = 0
		_BulgePercentage("BulgePercentage", Range( 0 , 1)) = 0
		_CumProgress("CumProgress", Range( -1 , 2)) = 0
		_SquishPullAmount("SquishPullAmount", Range( -1 , 1)) = 0
		_CumActive("CumActive", Range( 0 , 1)) = 0
		[Toggle(_DEFORM_BALLS_ON)] _DEFORM_BALLS("DEFORM_BALLS", Float) = 0
		[Toggle(_CLIP_DICK_ON)] _CLIP_DICK("CLIP_DICK", Float) = 0
		[ASEEnd][Toggle(_INVISIBLE_WHEN_INSIDE_ON)] _INVISIBLE_WHEN_INSIDE("INVISIBLE_WHEN_INSIDE", Float) = 0
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
		#pragma target 3.0

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

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			sampler2D _MainTex;
			sampler2D _DecalColorMap;
			sampler2D _BumpMap;
			sampler2D _MaskMap;


			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g702( float4 hsbc, float4 startColor )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.texcoord1.xyzw.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord7.z = vertexToFrag515;
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult256;

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

				float4 hsbc1_g702 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g702 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g702 = MyCustomExpression1_g702( hsbc1_g702 , startColor1_g702 );
				float2 texCoord103 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g702 , tex2DNode104 , tex2DNode104.a);
				
				float2 uv_BumpMap = IN.ase_texcoord7.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				
				float2 uv_MaskMap = IN.ase_texcoord7.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode102 = tex2D( _MaskMap, uv_MaskMap );
				
				float lerpResult108 = lerp( tex2DNode102.a , 0.9 , tex2DNode104.a);
				
				float vertexToFrag515 = IN.ase_texcoord7.z;
				
				float3 Albedo = lerpResult105.rgb;
				float3 Normal = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), 1.0f );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = tex2DNode102.r;
				float Smoothness = lerpResult108;
				float Occlusion = 1;
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;
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

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.ase_texcoord1.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord2.x = vertexToFrag515;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult256;

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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
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
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_tangent = v.ase_tangent;
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
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float vertexToFrag515 = IN.ase_texcoord2.x;
				
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;
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

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.ase_texcoord1.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord2.x = vertexToFrag515;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult256;
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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
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
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_tangent = v.ase_tangent;
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
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float vertexToFrag515 = IN.ase_texcoord2.x;
				
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;
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

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			sampler2D _MainTex;
			sampler2D _DecalColorMap;


			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g702( float4 hsbc, float4 startColor )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.texcoord1.w ) + ( temp_output_57_1 * v.texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.texcoord2.x ) + ( temp_output_57_1 * v.texcoord2.y ) + ( temp_output_57_2 * v.texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord2.z = vertexToFrag515;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult256;

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

				float4 hsbc1_g702 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g702 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g702 = MyCustomExpression1_g702( hsbc1_g702 , startColor1_g702 );
				float2 texCoord103 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g702 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag515 = IN.ase_texcoord2.z;
				
				
				float3 Albedo = lerpResult105.rgb;
				float3 Emission = 0;
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;

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
			
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			sampler2D _MainTex;
			sampler2D _DecalColorMap;


			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g702( float4 hsbc, float4 startColor )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.ase_texcoord1.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord2.z = vertexToFrag515;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult256;

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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
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
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord2 = v.ase_texcoord2;
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
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
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

				float4 hsbc1_g702 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g702 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g702 = MyCustomExpression1_g702( hsbc1_g702 , startColor1_g702 );
				float2 texCoord103 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g702 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag515 = IN.ase_texcoord2.z;
				
				
				float3 Albedo = lerpResult105.rgb;
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;

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

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.ase_texcoord1.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord3.x = vertexToFrag515;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult256;
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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_tangent : TANGENT;
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
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_tangent = v.ase_tangent;
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
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float vertexToFrag515 = IN.ase_texcoord3.x;
				
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;
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

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DEFORM_BALLS_ON
			#pragma multi_compile_local __ _CLIP_DICK_ON
			#pragma multi_compile_local __ _INVISIBLE_WHEN_INSIDE_ON


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _OrifaceOutWorldPosition3;
			float3 _OrifaceOutWorldPosition2;
			float3 _OrifaceOutWorldPosition1;
			float3 _DickRight;
			float3 _DickUp;
			float3 _OrifaceWorldNormal;
			float3 _OrifaceWorldPosition;
			float3 _DickOrigin;
			float3 _DickForward;
			float _CumActive;
			float _CumProgress;
			float _SquishPullAmount;
			float _BulgePercentage;
			float _PenetrationDepth;
			float _DickLength;
			float _OrifaceLength;
			float _BlendshapeMultiplier;
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
			sampler2D _MainTex;
			sampler2D _DecalColorMap;


			float3 MyCustomExpression20_g690( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g695( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g704( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g702( float4 hsbc, float4 startColor )
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

				float3 normalizeResult27_g289 = normalize( v.ase_normal );
				float3 temp_output_57_0 = normalizeResult27_g289;
				float3 normalizeResult31_g289 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g289 = normalize( cross( normalizeResult27_g289 , normalizeResult31_g289 ) );
				float3 temp_output_57_1 = cross( normalizeResult29_g289 , normalizeResult27_g289 );
				float3 temp_output_57_2 = normalizeResult29_g289;
				float3 CumDelta79 = ( ( ( temp_output_57_0 * v.texcoord1.xyzw.w ) + ( temp_output_57_1 * v.ase_texcoord2.w ) + ( temp_output_57_2 * v.ase_texcoord3.w ) ) * _BlendshapeMultiplier );
				float3 SquishDelta69 = ( ( ( temp_output_57_0 * v.ase_texcoord2.x ) + ( temp_output_57_1 * v.ase_texcoord2.y ) + ( temp_output_57_2 * v.ase_texcoord2.z ) ) * _BlendshapeMultiplier );
				float3 DickForward41 = _DickForward;
				float dotResult89 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_258_0 = max( _PenetrationDepth , 0.0 );
				float VisibleLength32 = ( _DickLength * ( 1.0 - temp_output_258_0 ) );
				float DickLength35 = _DickLength;
				float BulgePercentage244 = _BulgePercentage;
				float temp_output_1_0_g303 = saturate( ( abs( ( dotResult89 - VisibleLength32 ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float temp_output_91_0 = sqrt( ( 1.0 - ( temp_output_1_0_g303 * temp_output_1_0_g303 ) ) );
				float3 PullDelta72 = ( ( ( temp_output_57_0 * v.ase_texcoord3.x ) + ( temp_output_57_1 * v.ase_texcoord3.y ) + ( temp_output_57_2 * v.ase_texcoord3.z ) ) * _BlendshapeMultiplier );
				float dotResult224 = dot( v.vertex.xyz , DickForward41 );
				float temp_output_1_0_g304 = saturate( ( abs( ( dotResult224 - ( DickLength35 * _CumProgress ) ) ) / ( DickLength35 * BulgePercentage244 ) ) );
				float3 temp_output_218_0 = ( v.vertex.xyz + ( SquishDelta69 * temp_output_91_0 * saturate( -_SquishPullAmount ) ) + ( temp_output_91_0 * PullDelta72 * saturate( _SquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g304 * temp_output_1_0_g304 ) ) ) * CumDelta79 * _CumActive ) );
				float3 DickOrigin37 = _DickOrigin;
				float dotResult538 = dot( ( temp_output_218_0 - DickOrigin37 ) , DickForward41 );
				float PenetrationDepth252 = _PenetrationDepth;
				float temp_output_498_0 = ( PenetrationDepth252 * DickLength35 );
				float OrifaceLength285 = _OrifaceLength;
				float temp_output_533_0 = ( 0.2 * OrifaceLength285 );
				float dotResult500 = dot( ( v.vertex.xyz - DickOrigin37 ) , DickForward41 );
				float temp_output_509_0 = ( -( ( ( temp_output_498_0 - temp_output_533_0 ) + dotResult500 ) - DickLength35 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch534 = temp_output_509_0;
				#else
				float staticSwitch534 = max( temp_output_509_0 , ( ( ( temp_output_498_0 + dotResult500 + temp_output_533_0 ) - ( OrifaceLength285 + DickLength35 ) ) * 10.0 ) );
				#endif
				float InsideLerp523 = saturate( staticSwitch534 );
				float3 lerpResult521 = lerp( ( ( DickForward41 * dotResult538 ) + DickOrigin37 ) , temp_output_218_0 , InsideLerp523);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch514 = lerpResult521;
				#else
				float3 staticSwitch514 = temp_output_218_0;
				#endif
				float3 originalPosition291 = staticSwitch514;
				float3 temp_output_180_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult5 = dot( temp_output_180_0 , DickForward41 );
				float temp_output_42_0_g688 = ( dotResult5 / max( VisibleLength32 , 0.05 ) );
				float temp_output_26_0_g689 = temp_output_42_0_g688;
				float temp_output_19_0_g689 = ( 1.0 - temp_output_26_0_g689 );
				float3 temp_output_8_0_g688 = DickOrigin37;
				float3 temp_output_9_0_g688 = ( DickOrigin37 + ( DickForward41 * VisibleLength32 * 0.25 ) );
				float4 appendResult192 = (float4(_OrifaceWorldPosition , 1.0));
				float4 transform191 = mul(GetWorldToObjectMatrix(),appendResult192);
				float3 OrifacePosition80 = (transform191).xyz;
				float4 appendResult204 = (float4(_OrifaceWorldNormal , 0.0));
				float4 transform203 = mul(GetWorldToObjectMatrix(),appendResult204);
				float3 OrifaceNormal81 = (transform203).xyz;
				float3 temp_output_10_0_g688 = ( OrifacePosition80 + ( OrifaceNormal81 * 0.25 * VisibleLength32 ) );
				float3 temp_output_11_0_g688 = OrifacePosition80;
				float temp_output_1_0_g692 = temp_output_42_0_g688;
				float temp_output_8_0_g692 = ( 1.0 - temp_output_1_0_g692 );
				float3 temp_output_3_0_g692 = temp_output_9_0_g688;
				float3 temp_output_4_0_g692 = temp_output_10_0_g688;
				float3 temp_output_7_0_g690 = ( ( 3.0 * temp_output_8_0_g692 * temp_output_8_0_g692 * ( temp_output_3_0_g692 - temp_output_8_0_g688 ) ) + ( 6.0 * temp_output_8_0_g692 * temp_output_1_0_g692 * ( temp_output_4_0_g692 - temp_output_3_0_g692 ) ) + ( 3.0 * temp_output_1_0_g692 * temp_output_1_0_g692 * ( temp_output_11_0_g688 - temp_output_4_0_g692 ) ) );
				float3 bezierDerivitive20_g690 = temp_output_7_0_g690;
				float3 temp_output_3_0_g688 = DickForward41;
				float3 forward20_g690 = temp_output_3_0_g688;
				float3 DickUp39 = _DickUp;
				float3 temp_output_4_0_g688 = DickUp39;
				float3 up20_g690 = temp_output_4_0_g688;
				float3 localMyCustomExpression20_g690 = MyCustomExpression20_g690( bezierDerivitive20_g690 , forward20_g690 , up20_g690 );
				float3 normalizeResult27_g691 = normalize( localMyCustomExpression20_g690 );
				float3 normalizeResult31_g691 = normalize( cross( temp_output_7_0_g690 , localMyCustomExpression20_g690 ) );
				float3 normalizeResult29_g691 = normalize( cross( normalizeResult27_g691 , normalizeResult31_g691 ) );
				float3 temp_output_51_22_g688 = cross( normalizeResult29_g691 , normalizeResult27_g691 );
				float3 temp_output_2_0_g688 = temp_output_180_0;
				float3 DickRight44 = _DickRight;
				float3 temp_output_5_0_g688 = DickRight44;
				float dotResult15_g688 = dot( temp_output_2_0_g688 , temp_output_5_0_g688 );
				float3 temp_output_51_0_g688 = normalizeResult27_g691;
				float dotResult18_g688 = dot( temp_output_2_0_g688 , temp_output_4_0_g688 );
				float3 temp_output_184_0 = ( originalPosition291 - DickOrigin37 );
				float dotResult129 = dot( temp_output_184_0 , DickForward41 );
				float temp_output_168_0 = ( dotResult129 - VisibleLength32 );
				float temp_output_177_0 = ( temp_output_168_0 / OrifaceLength285 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch266 = min( temp_output_177_0 , 1.0 );
				#else
				float staticSwitch266 = temp_output_177_0;
				#endif
				float temp_output_42_0_g693 = staticSwitch266;
				float temp_output_26_0_g694 = temp_output_42_0_g693;
				float temp_output_19_0_g694 = ( 1.0 - temp_output_26_0_g694 );
				float3 temp_output_8_0_g693 = OrifacePosition80;
				float4 appendResult194 = (float4(_OrifaceOutWorldPosition1 , 1.0));
				float4 transform195 = mul(GetWorldToObjectMatrix(),appendResult194);
				float3 OrifaceOutPosition1151 = (transform195).xyz;
				float3 temp_output_9_0_g693 = OrifaceOutPosition1151;
				float4 appendResult197 = (float4(_OrifaceOutWorldPosition2 , 1.0));
				float4 transform198 = mul(GetWorldToObjectMatrix(),appendResult197);
				float3 OrifaceOutPosition2160 = (transform198).xyz;
				float3 temp_output_10_0_g693 = OrifaceOutPosition2160;
				float4 appendResult200 = (float4(_OrifaceOutWorldPosition3 , 1.0));
				float4 transform201 = mul(GetWorldToObjectMatrix(),appendResult200);
				float3 OrifaceOutPosition3165 = (transform201).xyz;
				float3 temp_output_11_0_g693 = OrifaceOutPosition3165;
				float temp_output_1_0_g697 = temp_output_42_0_g693;
				float temp_output_8_0_g697 = ( 1.0 - temp_output_1_0_g697 );
				float3 temp_output_3_0_g697 = temp_output_9_0_g693;
				float3 temp_output_4_0_g697 = temp_output_10_0_g693;
				float3 temp_output_7_0_g695 = ( ( 3.0 * temp_output_8_0_g697 * temp_output_8_0_g697 * ( temp_output_3_0_g697 - temp_output_8_0_g693 ) ) + ( 6.0 * temp_output_8_0_g697 * temp_output_1_0_g697 * ( temp_output_4_0_g697 - temp_output_3_0_g697 ) ) + ( 3.0 * temp_output_1_0_g697 * temp_output_1_0_g697 * ( temp_output_11_0_g693 - temp_output_4_0_g697 ) ) );
				float3 bezierDerivitive20_g695 = temp_output_7_0_g695;
				float3 temp_output_3_0_g693 = DickForward41;
				float3 forward20_g695 = temp_output_3_0_g693;
				float3 temp_output_4_0_g693 = DickUp39;
				float3 up20_g695 = temp_output_4_0_g693;
				float3 localMyCustomExpression20_g695 = MyCustomExpression20_g695( bezierDerivitive20_g695 , forward20_g695 , up20_g695 );
				float3 normalizeResult27_g696 = normalize( localMyCustomExpression20_g695 );
				float3 normalizeResult31_g696 = normalize( cross( temp_output_7_0_g695 , localMyCustomExpression20_g695 ) );
				float3 normalizeResult29_g696 = normalize( cross( normalizeResult27_g696 , normalizeResult31_g696 ) );
				float3 temp_output_51_22_g693 = cross( normalizeResult29_g696 , normalizeResult27_g696 );
				float3 temp_output_2_0_g693 = temp_output_184_0;
				float3 temp_output_5_0_g693 = DickRight44;
				float dotResult15_g693 = dot( temp_output_2_0_g693 , temp_output_5_0_g693 );
				float3 temp_output_51_0_g693 = normalizeResult27_g696;
				float dotResult18_g693 = dot( temp_output_2_0_g693 , temp_output_4_0_g693 );
				float temp_output_172_0 = saturate( sign( temp_output_168_0 ) );
				float3 lerpResult170 = lerp( ( ( ( temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_19_0_g689 * temp_output_8_0_g688 ) + ( temp_output_19_0_g689 * temp_output_19_0_g689 * 3.0 * temp_output_26_0_g689 * temp_output_9_0_g688 ) + ( 3.0 * temp_output_19_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_10_0_g688 ) + ( temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_26_0_g689 * temp_output_11_0_g688 ) ) + ( temp_output_51_22_g688 * dotResult15_g688 ) + ( temp_output_51_0_g688 * dotResult18_g688 ) ) , ( ( ( temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_19_0_g694 * temp_output_8_0_g693 ) + ( temp_output_19_0_g694 * temp_output_19_0_g694 * 3.0 * temp_output_26_0_g694 * temp_output_9_0_g693 ) + ( 3.0 * temp_output_19_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_10_0_g693 ) + ( temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_26_0_g694 * temp_output_11_0_g693 ) ) + ( temp_output_51_22_g693 * dotResult15_g693 ) + ( temp_output_51_0_g693 * dotResult18_g693 ) ) , temp_output_172_0);
				float3 temp_output_42_0_g703 = DickForward41;
				float NonVisibleLength31 = ( temp_output_258_0 * _DickLength );
				float3 temp_output_52_0_g703 = ( ( temp_output_42_0_g703 * ( ( NonVisibleLength31 - OrifaceLength285 ) - DickLength35 ) ) + ( originalPosition291 - DickOrigin37 ) );
				float dotResult53_g703 = dot( temp_output_42_0_g703 , temp_output_52_0_g703 );
				float temp_output_1_0_g706 = 1.0;
				float temp_output_8_0_g706 = ( 1.0 - temp_output_1_0_g706 );
				float3 temp_output_3_0_g706 = OrifaceOutPosition1151;
				float3 temp_output_4_0_g706 = OrifaceOutPosition2160;
				float3 temp_output_7_0_g704 = ( ( 3.0 * temp_output_8_0_g706 * temp_output_8_0_g706 * ( temp_output_3_0_g706 - OrifacePosition80 ) ) + ( 6.0 * temp_output_8_0_g706 * temp_output_1_0_g706 * ( temp_output_4_0_g706 - temp_output_3_0_g706 ) ) + ( 3.0 * temp_output_1_0_g706 * temp_output_1_0_g706 * ( OrifaceOutPosition3165 - temp_output_4_0_g706 ) ) );
				float3 bezierDerivitive20_g704 = temp_output_7_0_g704;
				float3 forward20_g704 = temp_output_42_0_g703;
				float3 temp_output_4_0_g703 = DickUp39;
				float3 up20_g704 = temp_output_4_0_g703;
				float3 localMyCustomExpression20_g704 = MyCustomExpression20_g704( bezierDerivitive20_g704 , forward20_g704 , up20_g704 );
				float3 normalizeResult27_g705 = normalize( localMyCustomExpression20_g704 );
				float3 normalizeResult31_g705 = normalize( cross( temp_output_7_0_g704 , localMyCustomExpression20_g704 ) );
				float3 normalizeResult29_g705 = normalize( cross( normalizeResult27_g705 , normalizeResult31_g705 ) );
				float3 temp_output_67_23_g703 = normalizeResult29_g705;
				float dotResult54_g703 = dot( temp_output_4_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_0_g703 = normalizeResult27_g705;
				float3 temp_output_43_0_g703 = DickRight44;
				float dotResult55_g703 = dot( temp_output_43_0_g703 , temp_output_52_0_g703 );
				float3 temp_output_67_22_g703 = cross( normalizeResult29_g705 , normalizeResult27_g705 );
				float temp_output_344_0 = saturate( sign( ( temp_output_177_0 - 1.0 ) ) );
				float3 lerpResult289 = lerp( lerpResult170 , ( ( ( dotResult53_g703 * temp_output_67_23_g703 ) + ( dotResult54_g703 * temp_output_67_0_g703 ) + ( dotResult55_g703 * temp_output_67_22_g703 ) ) + OrifaceOutPosition3165 ) , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch297 = lerpResult170;
				#else
				float3 staticSwitch297 = lerpResult289;
				#endif
				float temp_output_254_0 = saturate( -PenetrationDepth252 );
				float3 lerpResult250 = lerp( staticSwitch297 , v.vertex.xyz , temp_output_254_0);
				float3 ifLocalVar260 = 0;
				if( length( CumDelta79 ) <= 0.0 )
				ifLocalVar260 = originalPosition291;
				else
				ifLocalVar260 = lerpResult250;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch265 = lerpResult250;
				#else
				float3 staticSwitch265 = ifLocalVar260;
				#endif
				
				float3 temp_output_21_0_g688 = v.ase_normal;
				float dotResult55_g688 = dot( temp_output_21_0_g688 , temp_output_3_0_g688 );
				float dotResult56_g688 = dot( temp_output_21_0_g688 , temp_output_4_0_g688 );
				float dotResult57_g688 = dot( temp_output_21_0_g688 , temp_output_5_0_g688 );
				float3 normalizeResult31_g688 = normalize( ( ( dotResult55_g688 * normalizeResult29_g691 ) + ( dotResult56_g688 * temp_output_51_0_g688 ) + ( dotResult57_g688 * temp_output_51_22_g688 ) ) );
				float3 temp_output_21_0_g693 = v.ase_normal;
				float dotResult55_g693 = dot( temp_output_21_0_g693 , temp_output_3_0_g693 );
				float dotResult56_g693 = dot( temp_output_21_0_g693 , temp_output_4_0_g693 );
				float dotResult57_g693 = dot( temp_output_21_0_g693 , temp_output_5_0_g693 );
				float3 normalizeResult31_g693 = normalize( ( ( dotResult55_g693 * normalizeResult29_g696 ) + ( dotResult56_g693 * temp_output_51_0_g693 ) + ( dotResult57_g693 * temp_output_51_22_g693 ) ) );
				float3 lerpResult173 = lerp( normalizeResult31_g688 , normalizeResult31_g693 , temp_output_172_0);
				float3 temp_output_24_0_g703 = v.ase_normal;
				float dotResult61_g703 = dot( temp_output_42_0_g703 , temp_output_24_0_g703 );
				float dotResult62_g703 = dot( temp_output_4_0_g703 , temp_output_24_0_g703 );
				float dotResult60_g703 = dot( temp_output_43_0_g703 , temp_output_24_0_g703 );
				float3 normalizeResult33_g703 = normalize( ( ( dotResult61_g703 * temp_output_67_23_g703 ) + ( dotResult62_g703 * temp_output_67_0_g703 ) + ( dotResult60_g703 * temp_output_67_22_g703 ) ) );
				float3 lerpResult295 = lerp( lerpResult173 , normalizeResult33_g703 , temp_output_344_0);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch298 = lerpResult173;
				#else
				float3 staticSwitch298 = lerpResult295;
				#endif
				float3 lerpResult256 = lerp( staticSwitch298 , v.ase_normal , temp_output_254_0);
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch427 = InsideLerp523;
				#else
				float staticSwitch427 = 1.0;
				#endif
				float vertexToFrag515 = staticSwitch427;
				o.ase_texcoord7.z = vertexToFrag515;
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch265;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult256;

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

				float4 hsbc1_g702 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g702 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g702 = MyCustomExpression1_g702( hsbc1_g702 , startColor1_g702 );
				float2 texCoord103 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g702 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag515 = IN.ase_texcoord7.z;
				
				float3 Albedo = lerpResult105.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = vertexToFrag515;
				float AlphaClipThreshold = 0.01;
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
1916;181;1675;706;-1182.451;-838.5864;1.107931;True;False
Node;AmplifyShaderEditor.CommentaryNode;52;-4976.455,-3145.679;Inherit;False;1358.68;2563.85;Some variable setup;52;270;30;31;165;44;252;151;39;160;43;202;80;12;196;199;195;81;198;201;193;197;191;205;200;194;159;164;147;192;203;15;204;37;1;19;244;207;32;41;34;35;33;6;2;258;23;271;272;273;274;176;285;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-4962.455,-2286.527;Inherit;False;Property;_PenetrationDepth;PenetrationDepth;14;0;Create;True;0;0;0;False;0;False;0;-1;-1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;258;-4687.378,-2259.129;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-4850.07,-2398.98;Inherit;False;Property;_DickLength;DickLength;2;0;Create;True;0;0;0;False;0;False;1;1.675544;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;33;-4669.489,-2108.784;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;6;-4776.48,-2781.183;Inherit;False;Property;_DickForward;DickForward;1;0;Create;True;0;0;0;False;0;False;0,0,1;0.0003650414,0.06071361,0.9981552;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;-4433.521,-2785.505;Inherit;False;DickForward;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;54;-5345.401,-530.6367;Inherit;False;1717.116;1537.112;Get the blendshape deltas;25;63;58;62;73;79;78;77;76;75;74;72;71;70;68;67;66;69;65;64;61;60;59;57;56;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-4339.995,-2404.287;Inherit;False;DickLength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;243;-3453.025,-671.1805;Inherit;False;2031.068;1247.928;Localized Blendshapes;39;212;215;220;85;92;90;89;87;208;210;211;214;93;221;223;240;239;238;237;218;235;216;217;91;232;233;209;230;229;227;226;224;222;236;231;245;246;247;248;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-4498.969,-2102.772;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-3364.49,-185.5873;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;32;-4331.869,-2106.823;Inherit;False;VisibleLength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TangentVertexDataNode;55;-5242.061,-4.176445;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;231;-3427.975,477.4112;Inherit;False;Property;_CumProgress;CumProgress;19;0;Create;True;0;0;0;False;0;False;0;0;-1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;221;-3383.881,94.9912;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;232;-3403.025,378.2357;Inherit;False;35;DickLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-4921.194,-895.1556;Inherit;False;Property;_BulgePercentage;BulgePercentage;18;0;Create;True;0;0;0;False;0;False;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;223;-3376.085,269.0596;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;56;-5241.491,-180.2587;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;176;-4053.444,-2630.322;Inherit;False;Property;_OrifaceLength;OrifaceLength;17;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1;-4771.914,-3095.68;Inherit;False;Property;_DickOrigin;DickOrigin;0;0;Create;True;0;0;0;False;0;False;0,0,0;2.223419E-05,-0.2363962,-0.1551132;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;85;-3378.286,-356.6557;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;62;-4965.085,181.2963;Inherit;False;3;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;285;-3868.395,-2631.543;Inherit;False;OrifaceLength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;244;-4094.387,-887.5073;Inherit;False;BulgePercentage;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;224;-3038.613,148.3506;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;58;-5190.521,-423.6352;Inherit;False;2;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-4460.002,-3095.152;Inherit;False;DickOrigin;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;233;-3169.025,368.2357;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;73;-4969.691,449.0757;Inherit;False;1;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;57;-4965.229,-43.50259;Inherit;False;Create Orthogonal Vector;-1;;289;83358ef05db30f04ba825a1be5f469d8;0;2;25;FLOAT3;1,0,0;False;26;FLOAT3;0,1,0;False;3;FLOAT3;0;FLOAT3;1;FLOAT3;2
Node;AmplifyShaderEditor.DotProductOpNode;89;-3033.018,-303.2963;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-3364.812,-35.88542;Inherit;False;32;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;252;-4888.053,-2016.478;Inherit;False;PenetrationDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;489;-2771.091,-2005.969;Inherit;False;1722.249;733.2025;Dick Pinch when inside;22;523;513;511;510;509;507;508;504;505;506;502;501;500;498;497;496;495;490;491;493;531;534;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-4571.828,224.6032;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-4576.935,548.1751;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;92;-2880.361,-300.4567;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-4573.967,869.1758;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-4576.796,62.60213;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;222;-2885.956,151.1902;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;230;-2938.303,300.7181;Inherit;False;35;DickLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;490;-2755.367,-1467.66;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;491;-2752.621,-1626.685;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-4564.287,-145.6119;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;532;-2648.406,-2115.24;Inherit;False;285;OrifaceLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;245;-2934.01,-102.0246;Inherit;False;244;BulgePercentage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-4571.967,710.176;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-4573.828,383.6031;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;492;-2630.538,-2208.477;Inherit;False;Constant;_buffer;buffer;28;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;493;-2770.37,-1952.667;Inherit;False;252;PenetrationDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;495;-2711.924,-1743.335;Inherit;False;35;DickLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-4567.255,-466.612;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;209;-2938.708,-174.9288;Inherit;False;35;DickLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-4562.287,-304.6121;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;246;-2950.135,380.6296;Inherit;False;244;BulgePercentage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;498;-2536.632,-1853.247;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;497;-2559.819,-1566.431;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;93;-2702.69,-305.755;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;227;-2682.562,288.3647;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;220;-2708.285,145.8919;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;496;-2654.641,-1371.108;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;-4336.397,248.8253;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-4336.536,734.3989;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-5132.678,766.3522;Inherit;False;Property;_BlendshapeMultiplier;BlendshapeMultiplier;15;0;Create;True;0;0;0;False;0;False;1;2.985278;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-2676.967,-163.2822;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-4326.855,-280.3889;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;533;-2333.406,-2160.24;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;531;-2349.122,-1916.217;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-4160.593,-199.862;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;208;-2560.426,-303.0229;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-4181.591,718.89;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-4162.407,352.9512;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;226;-2566.021,148.624;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;500;-2399.641,-1499.108;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;236;-2499.202,-102.6926;Inherit;False;Property;_SquishPullAmount;SquishPullAmount;20;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;501;-2311.873,-1378.739;Inherit;False;285;OrifaceLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;229;-2415.466,149.1622;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;79;-4001.615,714.7198;Inherit;False;CumDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;502;-2225.235,-1722.575;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-3986.323,357.2032;Inherit;False;PullDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;238;-2309.829,-467.6244;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;211;-2409.871,-302.4847;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;69;-3993.38,-205.9108;Inherit;False;SquishDelta;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;217;-2194.327,-225.167;Inherit;False;72;PullDelta;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;91;-2217.01,-314.6847;Inherit;False;EaseOutCircular;-1;;303;8ef011c50e2d74145843b8825568a213;0;1;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;247;-2215.512,138.8171;Inherit;False;EaseOutCircular;-1;;304;8ef011c50e2d74145843b8825568a213;0;1;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;237;-2137.649,-93.93022;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;239;-2142.75,-486.7552;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;505;-2112.072,-1378.389;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;506;-2236.461,-1526.156;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;504;-2093.505,-1739.933;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;248;-2297.723,405.8108;Inherit;False;Property;_CumActive;CumActive;21;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;214;-2160.586,-401.8272;Inherit;False;69;SquishDelta;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;240;-2226.98,257.8603;Inherit;False;79;CumDelta;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;212;-1943.382,-621.1805;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;235;-1928.787,150.3386;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;216;-1905.106,-250.481;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;508;-1946.17,-1743.528;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;507;-1955.677,-1416.008;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;215;-1899.659,-418.2977;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;510;-1800.838,-1580.248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;509;-1800.496,-1727.622;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;218;-1574.957,-508.2743;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;537;-1952.297,-916.2717;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;539;-1959.904,-833.3502;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;536;-1756.749,-1013.043;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;511;-1641.58,-1691.237;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;538;-1584.251,-1019.086;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;541;-1599.904,-1095.35;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;534;-1584.977,-1542.346;Inherit;False;Property;_CLIP_DICK1;CLIP_DICK;23;0;Create;True;0;0;0;False;0;False;0;0;0;True;_CLIP_DICK_ON;Toggle;2;Key0;Key1;Reference;266;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;526;-1588.918,-767.1926;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;513;-1374.672,-1537.153;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;540;-1342.904,-1002.35;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;523;-1230.21,-1538.576;Inherit;False;InsideLerp;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;527;-1318.46,-775.3502;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;521;-1157.959,-538.5464;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;514;-857.5092,-1068.104;Inherit;False;Property;_INVISIBLE_WHEN_INSIDE;INVISIBLE_WHEN_INSIDE;25;0;Create;True;0;0;0;False;0;False;0;0;0;True;_INVISIBLE_WHEN_INSIDE;Toggle;2;Key0;Key1;Reference;427;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;19;-4930.7,-1625.755;Inherit;False;Property;_OrifaceWorldNormal;OrifaceWorldNormal;12;0;Create;True;0;0;0;False;0;False;0,0,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;124;-80.48791,-625.8585;Inherit;False;1512.968;1335.571;InsideBezierTransformation;25;169;168;129;144;167;126;166;134;139;143;127;141;177;171;184;113;114;115;112;111;241;259;286;266;425;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;291;-528.6437,-780.4668;Inherit;False;originalPosition;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;15;-4941.922,-1847.381;Inherit;False;Property;_OrifaceWorldPosition;OrifaceWorldPosition;9;0;Create;True;0;0;0;False;0;False;0,0,0;0.0006934633,0.01261455,0.0001472271;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;167;17.54336,-246.5188;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RelayNode;241;-3.006966,-359.6119;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;204;-4671.747,-1606.348;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;147;-4932.223,-1431.3;Inherit;False;Property;_OrifaceOutWorldPosition1;OrifaceOutWorldPosition1;7;0;Create;True;0;0;0;False;0;False;0,0,0;0,-0.33,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;184;252.3022,-378.8452;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;203;-4493.747,-1638.348;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;192;-4670.337,-1835.013;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;31.97204,-70.77341;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;164;-4932.94,-1062.879;Inherit;False;Property;_OrifaceOutWorldPosition3;OrifaceOutWorldPosition3;8;0;Create;True;0;0;0;False;0;False;0,0,0;0,-1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;159;-4932.051,-1240.919;Inherit;False;Property;_OrifaceOutWorldPosition2;OrifaceOutWorldPosition2;10;0;Create;True;0;0;0;False;0;False;0,0,0;0,-0.66,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;53;-77.66566,-2925.258;Inherit;False;1397.498;1696.416;OutsideBezierTransformation;22;5;7;14;24;18;26;36;38;42;40;45;46;47;48;50;51;82;83;180;186;242;424;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;315.7656,-145.9691;Inherit;False;32;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;194;-4658.243,-1424.08;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;205;-4265.747,-1635.348;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;129;449.0834,-393.9958;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;197;-4663.414,-1221.682;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;200;-4641.413,-1031.681;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;191;-4465.337,-1847.013;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;38;30.67948,-2576.457;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;201;-4436.413,-1043.681;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-4058.36,-1678.456;Inherit;False;OrifaceNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;195;-4453.243,-1436.08;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;168;595.0648,-393.869;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;193;-4219.337,-1855.013;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;198;-4458.414,-1233.682;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RelayNode;242;-27.80444,-2718.015;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-4495.479,-2253.633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;286;677.6079,-273.9864;Inherit;False;285;OrifaceLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;177;890.4041,-314.1482;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;43;-4781.68,-2616.343;Inherit;False;Property;_DickRight;DickRight;5;0;Create;True;0;0;0;False;0;False;1,0,0;0.9995936,0.02843111,-0.002094914;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;83;215.5567,-1683.971;Inherit;False;81;OrifaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;196;-4207.243,-1444.08;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;288;782.1151,875.407;Inherit;False;2042.808;1367.133;AllTheWayThrough;17;300;299;268;277;279;278;283;284;269;276;357;360;361;543;544;545;546;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-27.66577,-2096.24;Inherit;False;32;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;199;-4212.414,-1241.682;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;202;-4190.413,-1051.681;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-4337.489,-2257.578;Inherit;False;NonVisibleLength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;322.7352,-1344.839;Inherit;False;32;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;89.1147,-2462.619;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;107.2605,-1972.671;Inherit;False;Constant;_Float3;Float 3;7;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;12;-4772.305,-2938.479;Inherit;False;Property;_DickUp;DickUp;3;0;Create;True;0;0;0;False;0;False;0,1,0;0.02850586,-0.9977503,0.06067849;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;36;420.9843,-2482.511;Inherit;False;32;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;553.4703,-1577.856;Inherit;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-4042.033,-1961.266;Inherit;False;OrifacePosition;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;180;281.1804,-2697.167;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;160;-4010.543,-1249.784;Inherit;False;OrifaceOutPosition2;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;5;508.4149,-2699.109;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;259;887.8225,-143.5977;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;284;1450.587,1781.304;Inherit;False;285;OrifaceLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-4457.26,-2929.952;Inherit;False;DickUp;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-4458.265,-2608.829;Inherit;False;DickRight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;357;1344.591,1405.425;Inherit;False;31;NonVisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;757.4128,-1675.29;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;165;-4027.354,-1070.587;Inherit;False;OrifaceOutPosition3;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;186;644.2224,-2495.918;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;192.5567,-1818.971;Inherit;False;80;OrifacePosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;378.76,-2078.825;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;151;-4021.937,-1441.73;Inherit;False;OrifaceOutPosition1;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;342;1677.195,-187.8852;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;143;9.16564,126.3501;Inherit;False;44;DickRight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RelayNode;268;1074.491,1620.582;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;7;724.395,-2634.981;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;14;125.8223,-2874.257;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;126;11.40255,-155.178;Inherit;False;80;OrifacePosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;299;1076.739,1710.48;Inherit;False;37;DickOrigin;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;924.6042,-1778.301;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;51.05264,9.021725;Inherit;False;39;DickUp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;573.235,-2213.185;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;166;133.9714,364.2522;Inherit;False;160;OrifaceOutPosition2;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;266;1106.012,-239.4409;Inherit;False;Property;_CLIP_DICK;CLIP_DICK;23;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;114.4033,-2369.691;Inherit;False;39;DickUp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;129.2068,556.3395;Inherit;False;165;OrifaceOutPosition3;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;14.46523,211.7646;Inherit;False;151;OrifaceOutPosition1;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SignOpNode;171;900.9941,-429.722;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;116.5644,-2272.068;Inherit;False;44;DickRight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;283;1691.898,1428.133;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;361;1710.034,1765.321;Inherit;False;35;DickLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;144;27.20183,-549.9279;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;300;1285.5,1683.649;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SignOpNode;343;1982.038,-147.3198;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;1965.883,1806.224;Inherit;False;44;DickRight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;546;1917.836,1263.631;Inherit;False;165;OrifaceOutPosition3;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;545;1939.02,1176.402;Inherit;False;160;OrifaceOutPosition2;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;278;1932.883,1661.223;Inherit;False;41;DickForward;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;425;1101.096,-30.41792;Inherit;False;BezierSpaceTransform;-1;;693;d8cd7e255e788cb4f9cacb136d95dad5;0;10;42;FLOAT;0;False;21;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,1;False;4;FLOAT3;0,1,0;False;5;FLOAT3;1,0,0;False;8;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;2;FLOAT3;22;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;172;1679.071,-622.2599;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;290;3681.442,-1160.8;Inherit;False;872.8218;480.096;If we're not penetrating anything;6;251;253;254;255;250;256;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;544;1947.743,1092.911;Inherit;False;151;OrifaceOutPosition1;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;360;1905.186,1416.255;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;269;1556.062,981.478;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;543;1967.226,999.8442;Inherit;False;80;OrifacePosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;424;1018.832,-2498.435;Inherit;False;BezierSpaceTransform;-1;;688;d8cd7e255e788cb4f9cacb136d95dad5;0;10;42;FLOAT;0;False;21;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,1;False;4;FLOAT3;0,1,0;False;5;FLOAT3;1,0,0;False;8;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;2;FLOAT3;22;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;277;1479.708,1286.834;Inherit;False;165;OrifaceOutPosition3;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;276;1515.443,1172.921;Inherit;False;39;DickUp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;170;2122.402,-822.2975;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;547;2275.482,1228.26;Inherit;False;OrifaceSpaceTransform;-1;;703;a2cb6c5fdae31044587a631065a2df2f;0;11;68;FLOAT3;0,0,0;False;69;FLOAT3;0,0,0;False;70;FLOAT3;0,0,0;False;71;FLOAT3;0,0,0;False;24;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;42;FLOAT3;0,0,0;False;43;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;10;FLOAT;0;False;2;FLOAT3;34;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;344;2171.568,-175.8814;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;251;3731.442,-950.4623;Inherit;False;252;PenetrationDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;289;2584.067,-613.2349;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;264;4752.57,-1494.224;Inherit;False;892.3174;313.6557;Skip on tons of processing if we're the balls (or not part of the dick);5;265;260;263;262;292;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NegateNode;253;3991.37,-941.0542;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;525;6242.224,-2713.289;Inherit;False;794.332;385.2339;Clip dick alpha when inside;4;428;427;524;515;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;254;4177.57,-962.0543;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;206;6016.412,-2050.383;Inherit;False;1888.192;1147.05;FragmentShader;13;106;103;100;104;107;101;102;108;105;110;109;116;445;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;173;2112.694,-549.6835;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;4795.567,-1434.887;Inherit;False;79;CumDelta;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;255;4010.139,-1110.8;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;297;2814.938,-772.6146;Inherit;False;Property;_CLIP_DICK;CLIP_DICK;23;0;Create;True;0;0;0;False;0;False;0;0;0;True;_CLIP_DICK_ON;Toggle;2;Key0;Key1;Reference;266;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;250;4372.264,-1047.534;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;100;6070.291,-1564.565;Inherit;True;Property;_MainTex;MainTex;6;0;Create;True;0;0;0;False;0;False;-1;None;25159fabacb97444aa005a29bcd131a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;263;5002.189,-1424.88;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;103;6204.022,-1988.126;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;292;4819.718,-1333.824;Inherit;False;291;originalPosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;295;2683.241,-364.5937;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;428;6326.939,-2663.289;Inherit;False;Constant;_Float0;Float 0;28;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;106;6133.625,-1815.368;Inherit;False;Property;_HueBrightnessContrastSaturation;_HueBrightnessContrastSaturation;16;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.5,0.5,0.5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;524;6292.224,-2468.69;Inherit;False;523;InsideLerp;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;298;2940.157,-481.457;Inherit;False;Property;_CLIP_DICK;CLIP_DICK;23;0;Create;True;0;0;0;False;0;False;0;0;0;True;_CLIP_DICK_ON;Toggle;2;Key0;Key1;Reference;266;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ConditionalIfNode;260;5172.052,-1429.713;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;107;6529.273,-1778.095;Inherit;False;HueShift;-1;;702;1952e423258605d4aaa526c67ba2eb7c;0;2;2;FLOAT4;0,0.5,0.5,0.5;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;104;6584.06,-2000.383;Inherit;True;Property;_DecalColorMap;DecalColorMap;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;257;2018.033,-1069.033;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;427;6513.789,-2599.75;Inherit;False;Property;_INVISIBLE_WHEN_INSIDE;INVISIBLE_WHEN_INSIDE;25;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;515;6798.996,-2582.539;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;101;6066.412,-1349.593;Inherit;True;Property;_BumpMap;BumpMap;13;0;Create;True;0;0;0;False;0;False;-1;None;7a0a4dadf4ca8b846a2213c08fc385b0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;102;6070.819,-1133.333;Inherit;True;Property;_MaskMap;MaskMap;11;0;Create;True;0;0;0;False;0;False;-1;None;4ec4b7f4e3d05b54f9987bf565bd1410;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;256;4361.086,-839.7042;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;108;7017.784,-1185.667;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;445;7406.583,-1323.306;Inherit;False;Constant;_Float1;Float 1;28;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;270;-4949.412,-791.6831;Inherit;False;Property;_OrifaceOutWorldNormal;OrifaceOutWorldNormal;24;0;Create;True;0;0;0;False;0;False;0,0,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;274;-4066.556,-779.8602;Inherit;False;OrifaceOutNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;271;-4665.642,-746.7521;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;272;-4487.642,-778.7521;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;273;-4259.642,-775.7521;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;265;5369.051,-1332.521;Inherit;False;Property;_DEFORM_BALLS;DEFORM_BALLS;22;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;105;7151.767,-1775.192;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;114;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;113;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;109;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;110;7623.605,-1459.329;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Custom/Dick;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;0;  Early Z;0;Vertex Position,InvertActionOnDeselection;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;111;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;116;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;115;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;258;0;23;0
WireConnection;33;0;258;0
WireConnection;41;0;6;0
WireConnection;35;0;2;0
WireConnection;34;0;2;0
WireConnection;34;1;33;0
WireConnection;32;0;34;0
WireConnection;285;0;176;0
WireConnection;244;0;207;0
WireConnection;224;0;221;0
WireConnection;224;1;223;0
WireConnection;37;0;1;0
WireConnection;233;0;232;0
WireConnection;233;1;231;0
WireConnection;57;25;56;0
WireConnection;57;26;55;0
WireConnection;89;0;85;0
WireConnection;89;1;90;0
WireConnection;252;0;23;0
WireConnection;66;0;57;1
WireConnection;66;1;62;2
WireConnection;76;0;57;0
WireConnection;76;1;73;4
WireConnection;92;0;89;0
WireConnection;92;1;87;0
WireConnection;75;0;57;2
WireConnection;75;1;62;4
WireConnection;68;0;57;0
WireConnection;68;1;62;1
WireConnection;222;0;224;0
WireConnection;222;1;233;0
WireConnection;60;0;57;2
WireConnection;60;1;58;3
WireConnection;74;0;57;1
WireConnection;74;1;58;4
WireConnection;67;0;57;2
WireConnection;67;1;62;3
WireConnection;59;0;57;0
WireConnection;59;1;58;1
WireConnection;61;0;57;1
WireConnection;61;1;58;2
WireConnection;498;0;493;0
WireConnection;498;1;495;0
WireConnection;497;0;491;0
WireConnection;497;1;490;0
WireConnection;93;0;92;0
WireConnection;227;0;230;0
WireConnection;227;1;246;0
WireConnection;220;0;222;0
WireConnection;70;0;68;0
WireConnection;70;1;66;0
WireConnection;70;2;67;0
WireConnection;77;0;76;0
WireConnection;77;1;74;0
WireConnection;77;2;75;0
WireConnection;210;0;209;0
WireConnection;210;1;245;0
WireConnection;64;0;59;0
WireConnection;64;1;61;0
WireConnection;64;2;60;0
WireConnection;533;0;492;0
WireConnection;533;1;532;0
WireConnection;531;0;498;0
WireConnection;531;1;533;0
WireConnection;65;0;64;0
WireConnection;65;1;63;0
WireConnection;208;0;93;0
WireConnection;208;1;210;0
WireConnection;78;0;77;0
WireConnection;78;1;63;0
WireConnection;71;0;70;0
WireConnection;71;1;63;0
WireConnection;226;0;220;0
WireConnection;226;1;227;0
WireConnection;500;0;497;0
WireConnection;500;1;496;0
WireConnection;229;0;226;0
WireConnection;79;0;78;0
WireConnection;502;0;531;0
WireConnection;502;1;500;0
WireConnection;72;0;71;0
WireConnection;238;0;236;0
WireConnection;211;0;208;0
WireConnection;69;0;65;0
WireConnection;91;1;211;0
WireConnection;247;1;229;0
WireConnection;237;0;236;0
WireConnection;239;0;238;0
WireConnection;505;0;501;0
WireConnection;505;1;495;0
WireConnection;506;0;498;0
WireConnection;506;1;500;0
WireConnection;506;2;533;0
WireConnection;504;0;502;0
WireConnection;504;1;495;0
WireConnection;235;0;247;0
WireConnection;235;1;240;0
WireConnection;235;2;248;0
WireConnection;216;0;91;0
WireConnection;216;1;217;0
WireConnection;216;2;237;0
WireConnection;508;0;504;0
WireConnection;507;0;506;0
WireConnection;507;1;505;0
WireConnection;215;0;214;0
WireConnection;215;1;91;0
WireConnection;215;2;239;0
WireConnection;510;0;507;0
WireConnection;509;0;508;0
WireConnection;218;0;212;0
WireConnection;218;1;215;0
WireConnection;218;2;216;0
WireConnection;218;3;235;0
WireConnection;536;0;218;0
WireConnection;536;1;537;0
WireConnection;511;0;509;0
WireConnection;511;1;510;0
WireConnection;538;0;536;0
WireConnection;538;1;539;0
WireConnection;534;1;511;0
WireConnection;534;0;509;0
WireConnection;513;0;534;0
WireConnection;540;0;541;0
WireConnection;540;1;538;0
WireConnection;523;0;513;0
WireConnection;527;0;540;0
WireConnection;527;1;526;0
WireConnection;521;0;527;0
WireConnection;521;1;218;0
WireConnection;521;2;523;0
WireConnection;514;1;218;0
WireConnection;514;0;521;0
WireConnection;291;0;514;0
WireConnection;241;0;291;0
WireConnection;204;0;19;0
WireConnection;184;0;241;0
WireConnection;184;1;167;0
WireConnection;203;0;204;0
WireConnection;192;0;15;0
WireConnection;194;0;147;0
WireConnection;205;0;203;0
WireConnection;129;0;184;0
WireConnection;129;1;127;0
WireConnection;197;0;159;0
WireConnection;200;0;164;0
WireConnection;191;0;192;0
WireConnection;201;0;200;0
WireConnection;81;0;205;0
WireConnection;195;0;194;0
WireConnection;168;0;129;0
WireConnection;168;1;169;0
WireConnection;193;0;191;0
WireConnection;198;0;197;0
WireConnection;242;0;291;0
WireConnection;30;0;258;0
WireConnection;30;1;2;0
WireConnection;177;0;168;0
WireConnection;177;1;286;0
WireConnection;196;0;195;0
WireConnection;199;0;198;0
WireConnection;202;0;201;0
WireConnection;31;0;30;0
WireConnection;80;0;193;0
WireConnection;180;0;242;0
WireConnection;180;1;38;0
WireConnection;160;0;199;0
WireConnection;5;0;180;0
WireConnection;5;1;42;0
WireConnection;259;0;177;0
WireConnection;39;0;12;0
WireConnection;44;0;43;0
WireConnection;24;0;83;0
WireConnection;24;1;26;0
WireConnection;24;2;51;0
WireConnection;165;0;202;0
WireConnection;186;0;36;0
WireConnection;47;0;42;0
WireConnection;47;1;48;0
WireConnection;47;2;50;0
WireConnection;151;0;196;0
WireConnection;342;0;177;0
WireConnection;268;0;291;0
WireConnection;7;0;5;0
WireConnection;7;1;186;0
WireConnection;18;0;82;0
WireConnection;18;1;24;0
WireConnection;46;0;38;0
WireConnection;46;1;47;0
WireConnection;266;1;177;0
WireConnection;266;0;259;0
WireConnection;171;0;168;0
WireConnection;283;0;357;0
WireConnection;283;1;284;0
WireConnection;300;0;268;0
WireConnection;300;1;299;0
WireConnection;343;0;342;0
WireConnection;425;42;266;0
WireConnection;425;21;144;0
WireConnection;425;2;184;0
WireConnection;425;3;127;0
WireConnection;425;4;141;0
WireConnection;425;5;143;0
WireConnection;425;8;126;0
WireConnection;425;9;134;0
WireConnection;425;10;166;0
WireConnection;425;11;139;0
WireConnection;172;0;171;0
WireConnection;360;0;283;0
WireConnection;360;1;361;0
WireConnection;424;42;7;0
WireConnection;424;21;14;0
WireConnection;424;2;180;0
WireConnection;424;3;42;0
WireConnection;424;4;40;0
WireConnection;424;5;45;0
WireConnection;424;8;38;0
WireConnection;424;9;46;0
WireConnection;424;10;18;0
WireConnection;424;11;82;0
WireConnection;170;0;424;0
WireConnection;170;1;425;0
WireConnection;170;2;172;0
WireConnection;547;68;543;0
WireConnection;547;69;544;0
WireConnection;547;70;545;0
WireConnection;547;71;546;0
WireConnection;547;24;269;0
WireConnection;547;4;276;0
WireConnection;547;42;278;0
WireConnection;547;43;279;0
WireConnection;547;5;277;0
WireConnection;547;6;300;0
WireConnection;547;10;360;0
WireConnection;344;0;343;0
WireConnection;289;0;170;0
WireConnection;289;1;547;0
WireConnection;289;2;344;0
WireConnection;253;0;251;0
WireConnection;254;0;253;0
WireConnection;173;0;424;22
WireConnection;173;1;425;22
WireConnection;173;2;172;0
WireConnection;297;1;289;0
WireConnection;297;0;170;0
WireConnection;250;0;297;0
WireConnection;250;1;255;0
WireConnection;250;2;254;0
WireConnection;263;0;262;0
WireConnection;295;0;173;0
WireConnection;295;1;547;34
WireConnection;295;2;344;0
WireConnection;298;1;295;0
WireConnection;298;0;173;0
WireConnection;260;0;263;0
WireConnection;260;2;250;0
WireConnection;260;3;292;0
WireConnection;260;4;292;0
WireConnection;107;2;106;0
WireConnection;107;3;100;0
WireConnection;104;1;103;0
WireConnection;427;1;428;0
WireConnection;427;0;524;0
WireConnection;515;0;427;0
WireConnection;256;0;298;0
WireConnection;256;1;257;0
WireConnection;256;2;254;0
WireConnection;108;0;102;4
WireConnection;108;2;104;4
WireConnection;274;0;273;0
WireConnection;271;0;270;0
WireConnection;272;0;271;0
WireConnection;273;0;272;0
WireConnection;265;1;260;0
WireConnection;265;0;250;0
WireConnection;105;0;107;0
WireConnection;105;1;104;0
WireConnection;105;2;104;4
WireConnection;110;0;105;0
WireConnection;110;1;101;0
WireConnection;110;3;102;1
WireConnection;110;4;108;0
WireConnection;110;6;515;0
WireConnection;110;7;445;0
WireConnection;110;8;265;0
WireConnection;110;10;256;0
ASEEND*/
//CHKSM=96084EDA26D3ABC165FE39B727BE68857CC8642F