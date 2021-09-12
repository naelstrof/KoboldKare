// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Dick"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_PenetratorOrigin("PenetratorOrigin", Vector) = (0,0,0,0)
		_PenetratorForward("PenetratorForward", Vector) = (0,0,1,0)
		_PenetratorLength("PenetratorLength", Float) = 1
		_PenetratorUp("PenetratorUp", Vector) = (0,1,0,0)
		_PenetratorRight("PenetratorRight", Vector) = (1,0,0,0)
		_OrificeOutWorldPosition1("OrificeOutWorldPosition1", Vector) = (0,0.33,0,0)
		_OrificeOutWorldPosition3("OrificeOutWorldPosition3", Vector) = (0,1,0,0)
		_OrificeWorldPosition("OrificeWorldPosition", Vector) = (0,0,0,0)
		_OrificeOutWorldPosition2("OrificeOutWorldPosition2", Vector) = (0,0.66,0,0)
		_OrificeWorldNormal("OrificeWorldNormal", Vector) = (0,-1,0,0)
		_PenetrationDepth("PenetrationDepth", Range( -1 , 10)) = 0
		_PenetratorBlendshapeMultiplier("PenetratorBlendshapeMultiplier", Range( 0 , 100)) = 1
		_OrificeLength("OrificeLength", Float) = 1
		_PenetratorBulgePercentage("PenetratorBulgePercentage", Range( 0 , 1)) = 0
		_PenetratorCumProgress("PenetratorCumProgress", Range( -1 , 2)) = 0
		_PenetratorSquishPullAmount("PenetratorSquishPullAmount", Range( -1 , 1)) = 0
		_PenetratorCumActive("PenetratorCumActive", Range( 0 , 1)) = 0
		[Toggle(_DEFORM_BALLS_ON)] _DEFORM_BALLS("DEFORM_BALLS", Float) = 0
		[Toggle(_CLIP_DICK_ON)] _CLIP_DICK("CLIP_DICK", Float) = 0
		[Toggle(_NOBLENDSHAPES_ON)] _NOBLENDSHAPES("NOBLENDSHAPES", Float) = 0
		[Toggle(_INVISIBLE_WHEN_INSIDE_ON)] _INVISIBLE_WHEN_INSIDE("INVISIBLE_WHEN_INSIDE", Float) = 0
		_DecalColorMap("DecalColorMap", 2D) = "black" {}
		_MainTex("MainTex", 2D) = "white" {}
		_MaskMap("MaskMap", 2D) = "gray" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		[ASEEnd]_HueBrightnessContrastSaturation("_HueBrightnessContrastSaturation", Vector) = (0,0,0,0)
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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


			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g712( float4 hsbc, float4 startColor )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.texcoord1.xyzw.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord7.z = vertexToFrag250_g713;
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = ifLocalVar391_g713;

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

				float4 hsbc1_g712 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g712 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g712 = MyCustomExpression1_g712( hsbc1_g712 , startColor1_g712 );
				float2 texCoord103 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g712 , tex2DNode104 , tex2DNode104.a);
				
				float2 uv_BumpMap = IN.ase_texcoord7.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				
				float2 uv_MaskMap = IN.ase_texcoord7.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode102 = tex2D( _MaskMap, uv_MaskMap );
				
				float lerpResult108 = lerp( tex2DNode102.a , 0.9 , tex2DNode104.a);
				
				float vertexToFrag250_g713 = IN.ase_texcoord7.z;
				
				float3 Albedo = lerpResult105.rgb;
				float3 Normal = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), 1.0f );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = tex2DNode102.r;
				float Smoothness = lerpResult108;
				float Occlusion = 1;
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord1.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord2.x = vertexToFrag250_g713;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = ifLocalVar391_g713;

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

				float vertexToFrag250_g713 = IN.ase_texcoord2.x;
				
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord1.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord2.x = vertexToFrag250_g713;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = ifLocalVar391_g713;
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

				float vertexToFrag250_g713 = IN.ase_texcoord2.x;
				
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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


			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g712( float4 hsbc, float4 startColor )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.texcoord2.x ) + ( temp_output_35_1_g713 * v.texcoord2.y ) + ( temp_output_35_2_g713 * v.texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.texcoord1.w ) + ( temp_output_35_1_g713 * v.texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord2.z = vertexToFrag250_g713;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = ifLocalVar391_g713;

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

				float4 hsbc1_g712 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g712 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g712 = MyCustomExpression1_g712( hsbc1_g712 , startColor1_g712 );
				float2 texCoord103 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g712 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag250_g713 = IN.ase_texcoord2.z;
				
				
				float3 Albedo = lerpResult105.rgb;
				float3 Emission = 0;
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MaskMap_ST;
			float4 _MainTex_ST;
			float4 _HueBrightnessContrastSaturation;
			float4 _BumpMap_ST;
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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


			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g712( float4 hsbc, float4 startColor )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord1.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord2.z = vertexToFrag250_g713;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = ifLocalVar391_g713;

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

				float4 hsbc1_g712 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g712 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g712 = MyCustomExpression1_g712( hsbc1_g712 , startColor1_g712 );
				float2 texCoord103 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g712 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag250_g713 = IN.ase_texcoord2.z;
				
				
				float3 Albedo = lerpResult105.rgb;
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord1 : TEXCOORD1;
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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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
			

			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord1.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord3.x = vertexToFrag250_g713;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = ifLocalVar391_g713;
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

				float vertexToFrag250_g713 = IN.ase_texcoord3.x;
				
				float Alpha = vertexToFrag250_g713;
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
			#pragma multi_compile_local __ _NOBLENDSHAPES_ON


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
			float3 _PenetratorOrigin;
			float3 _OrificeWorldPosition;
			float3 _PenetratorUp;
			float3 _PenetratorForward;
			float3 _OrificeOutWorldPosition3;
			float3 _OrificeOutWorldPosition2;
			float3 _OrificeOutWorldPosition1;
			float3 _OrificeWorldNormal;
			float3 _PenetratorRight;
			float _PenetratorCumActive;
			float _PenetratorCumProgress;
			float _PenetratorSquishPullAmount;
			float _PenetratorBulgePercentage;
			float _PenetrationDepth;
			float _PenetratorLength;
			float _OrificeLength;
			float _PenetratorBlendshapeMultiplier;
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


			float3 MyCustomExpression20_g1010( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g998( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float3 MyCustomExpression20_g1003( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			
			float4 MyCustomExpression1_g712( float4 hsbc, float4 startColor )
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

				float3 VertexNormal259_g713 = v.ase_normal;
				float3 normalizeResult27_g1008 = normalize( VertexNormal259_g713 );
				float3 temp_output_35_0_g713 = normalizeResult27_g1008;
				float3 normalizeResult31_g1008 = normalize( v.ase_tangent.xyz );
				float3 normalizeResult29_g1008 = normalize( cross( normalizeResult27_g1008 , normalizeResult31_g1008 ) );
				float3 temp_output_35_1_g713 = cross( normalizeResult29_g1008 , normalizeResult27_g1008 );
				float3 temp_output_35_2_g713 = normalizeResult29_g1008;
				float3 SquishDelta85_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord2.x ) + ( temp_output_35_1_g713 * v.ase_texcoord2.y ) + ( temp_output_35_2_g713 * v.ase_texcoord2.z ) ) * _PenetratorBlendshapeMultiplier );
				float temp_output_234_0_g713 = length( SquishDelta85_g713 );
				float temp_output_11_0_g713 = max( _PenetrationDepth , 0.0 );
				float VisibleLength25_g713 = ( _PenetratorLength * ( 1.0 - temp_output_11_0_g713 ) );
				float3 DickOrigin16_g713 = _PenetratorOrigin;
				float4 appendResult132_g713 = (float4(_OrificeWorldPosition , 1.0));
				float4 transform140_g713 = mul(GetWorldToObjectMatrix(),appendResult132_g713);
				float3 OrifacePosition170_g713 = (transform140_g713).xyz;
				float DickLength19_g713 = _PenetratorLength;
				float3 DickUp172_g713 = _PenetratorUp;
				float3 VertexPosition254_g713 = v.vertex.xyz;
				float3 temp_output_27_0_g713 = ( VertexPosition254_g713 - DickOrigin16_g713 );
				float3 DickForward18_g713 = _PenetratorForward;
				float dotResult42_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float BulgePercentage37_g713 = _PenetratorBulgePercentage;
				float temp_output_1_0_g1006 = saturate( ( abs( ( dotResult42_g713 - VisibleLength25_g713 ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float temp_output_94_0_g713 = sqrt( ( 1.0 - ( temp_output_1_0_g1006 * temp_output_1_0_g1006 ) ) );
				float3 PullDelta91_g713 = ( ( ( temp_output_35_0_g713 * v.ase_texcoord3.x ) + ( temp_output_35_1_g713 * v.ase_texcoord3.y ) + ( temp_output_35_2_g713 * v.ase_texcoord3.z ) ) * _PenetratorBlendshapeMultiplier );
				float dotResult32_g713 = dot( temp_output_27_0_g713 , DickForward18_g713 );
				float temp_output_1_0_g1007 = saturate( ( abs( ( dotResult32_g713 - ( DickLength19_g713 * _PenetratorCumProgress ) ) ) / ( DickLength19_g713 * BulgePercentage37_g713 ) ) );
				float3 CumDelta90_g713 = ( ( ( temp_output_35_0_g713 * v.texcoord1.xyzw.w ) + ( temp_output_35_1_g713 * v.ase_texcoord2.w ) + ( temp_output_35_2_g713 * v.ase_texcoord3.w ) ) * _PenetratorBlendshapeMultiplier );
				#ifdef _NOBLENDSHAPES_ON
				float3 staticSwitch390_g713 = VertexPosition254_g713;
				#else
				float3 staticSwitch390_g713 = ( VertexPosition254_g713 + ( SquishDelta85_g713 * temp_output_94_0_g713 * saturate( -_PenetratorSquishPullAmount ) ) + ( temp_output_94_0_g713 * PullDelta91_g713 * saturate( _PenetratorSquishPullAmount ) ) + ( sqrt( ( 1.0 - ( temp_output_1_0_g1007 * temp_output_1_0_g1007 ) ) ) * CumDelta90_g713 * _PenetratorCumActive ) );
				#endif
				float dotResult118_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float PenetrationDepth39_g713 = _PenetrationDepth;
				float temp_output_65_0_g713 = ( PenetrationDepth39_g713 * DickLength19_g713 );
				float OrifaceLength34_g713 = _OrificeLength;
				float temp_output_73_0_g713 = ( 0.25 * OrifaceLength34_g713 );
				float dotResult80_g713 = dot( ( staticSwitch390_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_112_0_g713 = ( -( ( ( temp_output_65_0_g713 - temp_output_73_0_g713 ) + dotResult80_g713 ) - DickLength19_g713 ) * 10.0 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch117_g713 = temp_output_112_0_g713;
				#else
				float staticSwitch117_g713 = max( temp_output_112_0_g713 , ( ( ( temp_output_65_0_g713 + dotResult80_g713 + temp_output_73_0_g713 ) - ( OrifaceLength34_g713 + DickLength19_g713 ) ) * 10.0 ) );
				#endif
				float InsideLerp123_g713 = saturate( staticSwitch117_g713 );
				float3 lerpResult124_g713 = lerp( ( ( DickForward18_g713 * dotResult118_g713 ) + DickOrigin16_g713 ) , staticSwitch390_g713 , InsideLerp123_g713);
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float3 staticSwitch125_g713 = lerpResult124_g713;
				#else
				float3 staticSwitch125_g713 = staticSwitch390_g713;
				#endif
				float3 temp_output_354_0_g713 = ( staticSwitch125_g713 - DickOrigin16_g713 );
				float dotResult373_g713 = dot( DickUp172_g713 , temp_output_354_0_g713 );
				float3 DickRight184_g713 = _PenetratorRight;
				float dotResult374_g713 = dot( DickRight184_g713 , temp_output_354_0_g713 );
				float dotResult375_g713 = dot( temp_output_354_0_g713 , DickForward18_g713 );
				float3 lerpResult343_g713 = lerp( ( ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult373_g713 * DickUp172_g713 ) + ( ( saturate( ( ( VisibleLength25_g713 - distance( DickOrigin16_g713 , OrifacePosition170_g713 ) ) / DickLength19_g713 ) ) + 1.0 ) * dotResult374_g713 * DickRight184_g713 ) + ( DickForward18_g713 * dotResult375_g713 ) + DickOrigin16_g713 ) , staticSwitch125_g713 , saturate( PenetrationDepth39_g713 ));
				float3 originalPosition126_g713 = lerpResult343_g713;
				float dotResult177_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_178_0_g713 = max( VisibleLength25_g713 , 0.05 );
				float temp_output_42_0_g1009 = ( dotResult177_g713 / temp_output_178_0_g713 );
				float temp_output_26_0_g1013 = temp_output_42_0_g1009;
				float temp_output_19_0_g1013 = ( 1.0 - temp_output_26_0_g1013 );
				float3 temp_output_8_0_g1009 = DickOrigin16_g713;
				float temp_output_393_0_g713 = distance( DickOrigin16_g713 , OrifacePosition170_g713 );
				float temp_output_396_0_g713 = min( temp_output_178_0_g713 , temp_output_393_0_g713 );
				float3 temp_output_9_0_g1009 = ( DickOrigin16_g713 + ( DickForward18_g713 * temp_output_396_0_g713 * 0.25 ) );
				float4 appendResult130_g713 = (float4(_OrificeWorldNormal , 0.0));
				float4 transform135_g713 = mul(GetWorldToObjectMatrix(),appendResult130_g713);
				float3 OrifaceNormal155_g713 = (transform135_g713).xyz;
				float3 temp_output_10_0_g1009 = ( OrifacePosition170_g713 + ( OrifaceNormal155_g713 * 0.25 * temp_output_396_0_g713 ) );
				float3 temp_output_11_0_g1009 = OrifacePosition170_g713;
				float temp_output_1_0_g1011 = temp_output_42_0_g1009;
				float temp_output_8_0_g1011 = ( 1.0 - temp_output_1_0_g1011 );
				float3 temp_output_3_0_g1011 = temp_output_9_0_g1009;
				float3 temp_output_4_0_g1011 = temp_output_10_0_g1009;
				float3 temp_output_7_0_g1010 = ( ( 3.0 * temp_output_8_0_g1011 * temp_output_8_0_g1011 * ( temp_output_3_0_g1011 - temp_output_8_0_g1009 ) ) + ( 6.0 * temp_output_8_0_g1011 * temp_output_1_0_g1011 * ( temp_output_4_0_g1011 - temp_output_3_0_g1011 ) ) + ( 3.0 * temp_output_1_0_g1011 * temp_output_1_0_g1011 * ( temp_output_11_0_g1009 - temp_output_4_0_g1011 ) ) );
				float3 normalizeResult27_g1012 = normalize( temp_output_7_0_g1010 );
				float3 bezierDerivitive20_g1010 = temp_output_7_0_g1010;
				float3 temp_output_3_0_g1009 = DickForward18_g713;
				float3 forward20_g1010 = temp_output_3_0_g1009;
				float3 temp_output_4_0_g1009 = DickUp172_g713;
				float3 up20_g1010 = temp_output_4_0_g1009;
				float3 localMyCustomExpression20_g1010 = MyCustomExpression20_g1010( bezierDerivitive20_g1010 , forward20_g1010 , up20_g1010 );
				float3 normalizeResult31_g1012 = normalize( localMyCustomExpression20_g1010 );
				float3 normalizeResult29_g1012 = normalize( cross( normalizeResult27_g1012 , normalizeResult31_g1012 ) );
				float3 temp_output_65_22_g1009 = normalizeResult29_g1012;
				float3 temp_output_2_0_g1009 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g1009 = DickRight184_g713;
				float dotResult15_g1009 = dot( temp_output_2_0_g1009 , temp_output_5_0_g1009 );
				float3 temp_output_65_0_g1009 = cross( normalizeResult29_g1012 , normalizeResult27_g1012 );
				float dotResult18_g1009 = dot( temp_output_2_0_g1009 , temp_output_4_0_g1009 );
				float dotResult142_g713 = dot( ( originalPosition126_g713 - DickOrigin16_g713 ) , DickForward18_g713 );
				float temp_output_152_0_g713 = ( dotResult142_g713 - VisibleLength25_g713 );
				float temp_output_157_0_g713 = ( temp_output_152_0_g713 / OrifaceLength34_g713 );
				#ifdef _CLIP_DICK_ON
				float staticSwitch197_g713 = min( temp_output_157_0_g713 , 1.0 );
				#else
				float staticSwitch197_g713 = temp_output_157_0_g713;
				#endif
				float temp_output_42_0_g997 = staticSwitch197_g713;
				float temp_output_26_0_g1001 = temp_output_42_0_g997;
				float temp_output_19_0_g1001 = ( 1.0 - temp_output_26_0_g1001 );
				float3 temp_output_8_0_g997 = OrifacePosition170_g713;
				float4 appendResult145_g713 = (float4(_OrificeOutWorldPosition1 , 1.0));
				float4 transform151_g713 = mul(GetWorldToObjectMatrix(),appendResult145_g713);
				float3 OrifaceOutPosition1183_g713 = (transform151_g713).xyz;
				float3 temp_output_9_0_g997 = OrifaceOutPosition1183_g713;
				float4 appendResult144_g713 = (float4(_OrificeOutWorldPosition2 , 1.0));
				float4 transform154_g713 = mul(GetWorldToObjectMatrix(),appendResult144_g713);
				float3 OrifaceOutPosition2182_g713 = (transform154_g713).xyz;
				float3 temp_output_10_0_g997 = OrifaceOutPosition2182_g713;
				float4 appendResult143_g713 = (float4(_OrificeOutWorldPosition3 , 1.0));
				float4 transform147_g713 = mul(GetWorldToObjectMatrix(),appendResult143_g713);
				float3 OrifaceOutPosition3175_g713 = (transform147_g713).xyz;
				float3 temp_output_11_0_g997 = OrifaceOutPosition3175_g713;
				float temp_output_1_0_g999 = temp_output_42_0_g997;
				float temp_output_8_0_g999 = ( 1.0 - temp_output_1_0_g999 );
				float3 temp_output_3_0_g999 = temp_output_9_0_g997;
				float3 temp_output_4_0_g999 = temp_output_10_0_g997;
				float3 temp_output_7_0_g998 = ( ( 3.0 * temp_output_8_0_g999 * temp_output_8_0_g999 * ( temp_output_3_0_g999 - temp_output_8_0_g997 ) ) + ( 6.0 * temp_output_8_0_g999 * temp_output_1_0_g999 * ( temp_output_4_0_g999 - temp_output_3_0_g999 ) ) + ( 3.0 * temp_output_1_0_g999 * temp_output_1_0_g999 * ( temp_output_11_0_g997 - temp_output_4_0_g999 ) ) );
				float3 normalizeResult27_g1000 = normalize( temp_output_7_0_g998 );
				float3 bezierDerivitive20_g998 = temp_output_7_0_g998;
				float3 temp_output_3_0_g997 = DickForward18_g713;
				float3 forward20_g998 = temp_output_3_0_g997;
				float3 temp_output_4_0_g997 = DickUp172_g713;
				float3 up20_g998 = temp_output_4_0_g997;
				float3 localMyCustomExpression20_g998 = MyCustomExpression20_g998( bezierDerivitive20_g998 , forward20_g998 , up20_g998 );
				float3 normalizeResult31_g1000 = normalize( localMyCustomExpression20_g998 );
				float3 normalizeResult29_g1000 = normalize( cross( normalizeResult27_g1000 , normalizeResult31_g1000 ) );
				float3 temp_output_65_22_g997 = normalizeResult29_g1000;
				float3 temp_output_2_0_g997 = ( originalPosition126_g713 - DickOrigin16_g713 );
				float3 temp_output_5_0_g997 = DickRight184_g713;
				float dotResult15_g997 = dot( temp_output_2_0_g997 , temp_output_5_0_g997 );
				float3 temp_output_65_0_g997 = cross( normalizeResult29_g1000 , normalizeResult27_g1000 );
				float dotResult18_g997 = dot( temp_output_2_0_g997 , temp_output_4_0_g997 );
				float temp_output_208_0_g713 = saturate( sign( temp_output_152_0_g713 ) );
				float3 lerpResult221_g713 = lerp( ( ( ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_19_0_g1013 * temp_output_8_0_g1009 ) + ( temp_output_19_0_g1013 * temp_output_19_0_g1013 * 3.0 * temp_output_26_0_g1013 * temp_output_9_0_g1009 ) + ( 3.0 * temp_output_19_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_10_0_g1009 ) + ( temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_26_0_g1013 * temp_output_11_0_g1009 ) ) + ( temp_output_65_22_g1009 * dotResult15_g1009 ) + ( temp_output_65_0_g1009 * dotResult18_g1009 ) ) , ( ( ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_19_0_g1001 * temp_output_8_0_g997 ) + ( temp_output_19_0_g1001 * temp_output_19_0_g1001 * 3.0 * temp_output_26_0_g1001 * temp_output_9_0_g997 ) + ( 3.0 * temp_output_19_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_10_0_g997 ) + ( temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_26_0_g1001 * temp_output_11_0_g997 ) ) + ( temp_output_65_22_g997 * dotResult15_g997 ) + ( temp_output_65_0_g997 * dotResult18_g997 ) ) , temp_output_208_0_g713);
				float3 temp_output_42_0_g1002 = DickForward18_g713;
				float NonVisibleLength165_g713 = ( temp_output_11_0_g713 * _PenetratorLength );
				float3 temp_output_52_0_g1002 = ( ( temp_output_42_0_g1002 * ( ( NonVisibleLength165_g713 - OrifaceLength34_g713 ) - DickLength19_g713 ) ) + ( originalPosition126_g713 - DickOrigin16_g713 ) );
				float dotResult53_g1002 = dot( temp_output_42_0_g1002 , temp_output_52_0_g1002 );
				float temp_output_1_0_g1004 = 1.0;
				float temp_output_8_0_g1004 = ( 1.0 - temp_output_1_0_g1004 );
				float3 temp_output_3_0_g1004 = OrifaceOutPosition1183_g713;
				float3 temp_output_4_0_g1004 = OrifaceOutPosition2182_g713;
				float3 temp_output_7_0_g1003 = ( ( 3.0 * temp_output_8_0_g1004 * temp_output_8_0_g1004 * ( temp_output_3_0_g1004 - OrifacePosition170_g713 ) ) + ( 6.0 * temp_output_8_0_g1004 * temp_output_1_0_g1004 * ( temp_output_4_0_g1004 - temp_output_3_0_g1004 ) ) + ( 3.0 * temp_output_1_0_g1004 * temp_output_1_0_g1004 * ( OrifaceOutPosition3175_g713 - temp_output_4_0_g1004 ) ) );
				float3 normalizeResult27_g1005 = normalize( temp_output_7_0_g1003 );
				float3 temp_output_85_23_g1002 = normalizeResult27_g1005;
				float3 temp_output_4_0_g1002 = DickUp172_g713;
				float dotResult54_g1002 = dot( temp_output_4_0_g1002 , temp_output_52_0_g1002 );
				float3 bezierDerivitive20_g1003 = temp_output_7_0_g1003;
				float3 forward20_g1003 = temp_output_42_0_g1002;
				float3 up20_g1003 = temp_output_4_0_g1002;
				float3 localMyCustomExpression20_g1003 = MyCustomExpression20_g1003( bezierDerivitive20_g1003 , forward20_g1003 , up20_g1003 );
				float3 normalizeResult31_g1005 = normalize( localMyCustomExpression20_g1003 );
				float3 normalizeResult29_g1005 = normalize( cross( normalizeResult27_g1005 , normalizeResult31_g1005 ) );
				float3 temp_output_85_0_g1002 = cross( normalizeResult29_g1005 , normalizeResult27_g1005 );
				float3 temp_output_43_0_g1002 = DickRight184_g713;
				float dotResult55_g1002 = dot( temp_output_43_0_g1002 , temp_output_52_0_g1002 );
				float3 temp_output_85_22_g1002 = normalizeResult29_g1005;
				float temp_output_222_0_g713 = saturate( sign( ( temp_output_157_0_g713 - 1.0 ) ) );
				float3 lerpResult224_g713 = lerp( lerpResult221_g713 , ( ( ( dotResult53_g1002 * temp_output_85_23_g1002 ) + ( dotResult54_g1002 * temp_output_85_0_g1002 ) + ( dotResult55_g1002 * temp_output_85_22_g1002 ) ) + OrifaceOutPosition3175_g713 ) , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch229_g713 = lerpResult221_g713;
				#else
				float3 staticSwitch229_g713 = lerpResult224_g713;
				#endif
				float temp_output_226_0_g713 = saturate( -PenetrationDepth39_g713 );
				float3 lerpResult232_g713 = lerp( staticSwitch229_g713 , originalPosition126_g713 , temp_output_226_0_g713);
				float3 ifLocalVar237_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar237_g713 = originalPosition126_g713;
				else
				ifLocalVar237_g713 = lerpResult232_g713;
				#ifdef _DEFORM_BALLS_ON
				float3 staticSwitch239_g713 = lerpResult232_g713;
				#else
				float3 staticSwitch239_g713 = ifLocalVar237_g713;
				#endif
				
				float3 temp_output_21_0_g1009 = VertexNormal259_g713;
				float dotResult55_g1009 = dot( temp_output_21_0_g1009 , temp_output_3_0_g1009 );
				float dotResult56_g1009 = dot( temp_output_21_0_g1009 , temp_output_4_0_g1009 );
				float dotResult57_g1009 = dot( temp_output_21_0_g1009 , temp_output_5_0_g1009 );
				float3 normalizeResult31_g1009 = normalize( ( ( dotResult55_g1009 * normalizeResult27_g1012 ) + ( dotResult56_g1009 * temp_output_65_0_g1009 ) + ( dotResult57_g1009 * temp_output_65_22_g1009 ) ) );
				float3 temp_output_21_0_g997 = VertexNormal259_g713;
				float dotResult55_g997 = dot( temp_output_21_0_g997 , temp_output_3_0_g997 );
				float dotResult56_g997 = dot( temp_output_21_0_g997 , temp_output_4_0_g997 );
				float dotResult57_g997 = dot( temp_output_21_0_g997 , temp_output_5_0_g997 );
				float3 normalizeResult31_g997 = normalize( ( ( dotResult55_g997 * normalizeResult27_g1000 ) + ( dotResult56_g997 * temp_output_65_0_g997 ) + ( dotResult57_g997 * temp_output_65_22_g997 ) ) );
				float3 lerpResult227_g713 = lerp( normalizeResult31_g1009 , normalizeResult31_g997 , temp_output_208_0_g713);
				float3 temp_output_24_0_g1002 = VertexNormal259_g713;
				float dotResult61_g1002 = dot( temp_output_42_0_g1002 , temp_output_24_0_g1002 );
				float dotResult62_g1002 = dot( temp_output_4_0_g1002 , temp_output_24_0_g1002 );
				float dotResult60_g1002 = dot( temp_output_43_0_g1002 , temp_output_24_0_g1002 );
				float3 normalizeResult33_g1002 = normalize( ( ( dotResult61_g1002 * temp_output_85_23_g1002 ) + ( dotResult62_g1002 * temp_output_85_0_g1002 ) + ( dotResult60_g1002 * temp_output_85_22_g1002 ) ) );
				float3 lerpResult233_g713 = lerp( lerpResult227_g713 , normalizeResult33_g1002 , temp_output_222_0_g713);
				#ifdef _CLIP_DICK_ON
				float3 staticSwitch236_g713 = lerpResult227_g713;
				#else
				float3 staticSwitch236_g713 = lerpResult233_g713;
				#endif
				float3 lerpResult238_g713 = lerp( staticSwitch236_g713 , VertexNormal259_g713 , temp_output_226_0_g713);
				float3 ifLocalVar391_g713 = 0;
				if( temp_output_234_0_g713 <= 0.0 )
				ifLocalVar391_g713 = VertexNormal259_g713;
				else
				ifLocalVar391_g713 = lerpResult238_g713;
				
				#ifdef _INVISIBLE_WHEN_INSIDE_ON
				float staticSwitch249_g713 = InsideLerp123_g713;
				#else
				float staticSwitch249_g713 = 1.0;
				#endif
				float vertexToFrag250_g713 = staticSwitch249_g713;
				o.ase_texcoord7.z = vertexToFrag250_g713;
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				o.ase_texcoord8 = v.texcoord1.xyzw;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = staticSwitch239_g713;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = ifLocalVar391_g713;

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

				float4 hsbc1_g712 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g712 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g712 = MyCustomExpression1_g712( hsbc1_g712 , startColor1_g712 );
				float2 texCoord103 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g712 , tex2DNode104 , tex2DNode104.a);
				
				float vertexToFrag250_g713 = IN.ase_texcoord7.z;
				
				float3 Albedo = lerpResult105.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = vertexToFrag250_g713;
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
Version=18910
149;164;1675;699;-6478.553;1572.621;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;206;6016.412,-2050.383;Inherit;False;1888.192;1147.05;FragmentShader;14;106;103;100;104;107;101;102;108;105;110;109;116;445;552;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;103;6204.022,-1988.126;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;106;6133.625,-1815.368;Inherit;False;Property;_HueBrightnessContrastSaturation;_HueBrightnessContrastSaturation;26;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.5019608,0.5019608,0.5019608;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;100;6070.291,-1564.565;Inherit;True;Property;_MainTex;MainTex;23;0;Create;True;0;0;0;False;0;False;-1;None;c6a51a68e5768654f8e614a5d167aefd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;104;6584.06,-2000.383;Inherit;True;Property;_DecalColorMap;DecalColorMap;22;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;107;6529.273,-1778.095;Inherit;False;HueShift;-1;;712;1952e423258605d4aaa526c67ba2eb7c;0;2;2;FLOAT4;0,0.5,0.5,0.5;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;105;7151.767,-1775.192;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;102;6070.819,-1133.333;Inherit;True;Property;_MaskMap;MaskMap;24;0;Create;True;0;0;0;False;0;False;-1;None;aef0d52182fe29d48985b053faf59e23;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;101;6066.412,-1349.593;Inherit;True;Property;_BumpMap;BumpMap;25;0;Create;True;0;0;0;False;0;False;-1;None;9c44ea8cd9bad9a41b2e1c4b503546e2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;552;7033.459,-1040.183;Inherit;False;PenetrationTechDeformation;0;;713;cb4db099da64a8846a0c6877ff8e2b5f;0;3;253;FLOAT3;0,0,0;False;258;FLOAT3;0,0,0;False;265;FLOAT3;0,0,0;False;3;FLOAT3;0;FLOAT;251;FLOAT3;252
Node;AmplifyShaderEditor.LerpOp;108;6909.51,-1170.398;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;445;7295.528,-1194.208;Inherit;False;Constant;_Float1;Float 1;28;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;114;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;111;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;113;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;109;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;110;7623.605,-1459.329;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Custom/Dick;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;0;  Early Z;0;Vertex Position,InvertActionOnDeselection;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;116;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;115;425.999,-147.7006;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;104;1;103;0
WireConnection;107;2;106;0
WireConnection;107;3;100;0
WireConnection;105;0;107;0
WireConnection;105;1;104;0
WireConnection;105;2;104;4
WireConnection;108;0;102;4
WireConnection;108;2;104;4
WireConnection;110;0;105;0
WireConnection;110;1;101;0
WireConnection;110;3;102;1
WireConnection;110;4;108;0
WireConnection;110;6;552;251
WireConnection;110;7;445;0
WireConnection;110;8;552;0
WireConnection;110;10;552;252
ASEEND*/
//CHKSM=8B99B5EF8F75A341D5CBE4287C08FFD086E51B6C