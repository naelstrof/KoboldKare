// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Dick"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_DecalColorMap("DecalColorMap", 2D) = "black" {}
		[HideInInspector]_DickRootWorld("DickRootWorld", Vector) = (0,0,0,0)
		[HideInInspector]_DickForwardWorld("DickForwardWorld", Vector) = (0,0,0,0)
		[HideInInspector]_DickRightWorld("DickRightWorld", Vector) = (0,0,0,0)
		[HideInInspector]_DickUpWorld("DickUpWorld", Vector) = (0,0,0,0)
		[HideInInspector]_StartClip("_StartClip", Float) = 0
		[HideInInspector]_EndClip("_EndClip", Float) = 0
		[HideInInspector]_SquashStretchCorrection("_SquashStretchCorrection", Float) = 1
		[HideInInspector]_DistanceToHole("_DistanceToHole", Float) = 0
		[HideInInspector]_DickWorldLength("_DickWorldLength", Float) = 1
		_MainTex("MainTex", 2D) = "white" {}
		_MaskMap("MaskMap", 2D) = "gray" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		[ASEEnd]_HueBrightnessContrastSaturation("_HueBrightnessContrastSaturation", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
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
			#define ASE_NEEDS_VERT_TANGENT
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


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
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
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


			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
			}
			
			float4 MyCustomExpression1_g726( float4 hsbc, float4 startColor )
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

				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				float4 break79_g722 = worldTangentOUT56_g722;
				float4 appendResult77_g722 = (float4(break79_g722.x , break79_g722.y , break79_g722.z , 0.0));
				float3 normalizeResult80_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult77_g722 )).xyz );
				float4 appendResult83_g722 = (float4(normalizeResult80_g722 , break79_g722.w));
				float4 lerpResult581 = lerp( v.ase_tangent , appendResult83_g722 , v.ase_color.r);
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_texcoord8.zw = v.texcoord1.xy;
				o.ase_texcoord9 = v.vertex;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult578;
				v.ase_tangent = lerpResult581;

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

				float4 hsbc1_g726 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord8.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g726 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g726 = MyCustomExpression1_g726( hsbc1_g726 , startColor1_g726 );
				float2 texCoord103 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g726 , tex2DNode104 , tex2DNode104.a);
				
				float2 uv_BumpMap = IN.ase_texcoord8.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
				
				float2 uv_MaskMap = IN.ase_texcoord8.xy * _MaskMap_ST.xy + _MaskMap_ST.zw;
				float4 tex2DNode102 = tex2D( _MaskMap, uv_MaskMap );
				
				float lerpResult108 = lerp( tex2DNode102.a , 0.9 , tex2DNode104.a);
				
				float4 appendResult67_g722 = (float4(IN.ase_texcoord9.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				float3 Albedo = lerpResult105.rgb;
				float3 Normal = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), 1.0f );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = tex2DNode102.r;
				float Smoothness = lerpResult108;
				float Occlusion = 1;
				float Alpha = lerpResult583;
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
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
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
			

			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord2 = v.vertex;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult578;

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
				float4 ase_tangent : TANGENT;
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

				float4 appendResult67_g722 = (float4(IN.ase_texcoord2.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				float Alpha = lerpResult583;
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
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
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
			

			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord2 = v.vertex;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult578;
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
				float4 ase_tangent : TANGENT;
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

				float4 appendResult67_g722 = (float4(IN.ase_texcoord2.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				float Alpha = lerpResult583;
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
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
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
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
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


			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
			}
			
			float4 MyCustomExpression1_g726( float4 hsbc, float4 startColor )
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

				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord4.xy = v.texcoord0.xy;
				o.ase_texcoord4.zw = v.texcoord1.xy;
				o.ase_texcoord5 = v.vertex;
				o.ase_color = v.ase_color;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult578;

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
				float4 ase_tangent : TANGENT;
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
				o.ase_tangent = v.ase_tangent;
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
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float4 hsbc1_g726 = _HueBrightnessContrastSaturation;
				float2 uv_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 startColor1_g726 = tex2D( _MainTex, uv_MainTex );
				float4 localMyCustomExpression1_g726 = MyCustomExpression1_g726( hsbc1_g726 , startColor1_g726 );
				float2 texCoord103 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode104 = tex2Dlod( _DecalColorMap, float4( texCoord103, 0, 0.0) );
				float4 lerpResult105 = lerp( localMyCustomExpression1_g726 , tex2DNode104 , tex2DNode104.a);
				
				float4 appendResult67_g722 = (float4(IN.ase_texcoord5.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				
				float3 Albedo = lerpResult105.rgb;
				float3 Emission = 0;
				float Alpha = lerpResult583;
				float AlphaClipThreshold = 0.01;

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
			#define ASE_NEEDS_VERT_TANGENT
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


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
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
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
			sampler2D _BumpMap;


			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_texcoord5 = v.vertex;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult578;
				v.ase_tangent = v.ase_tangent;
				
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
				
				float4 appendResult67_g722 = (float4(IN.ase_texcoord5.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				float3 Normal = UnpackNormalScale( tex2D( _BumpMap, uv_BumpMap ), 1.0f );
				float Alpha = lerpResult583;
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
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
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


				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord = v.vertex;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult578;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
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
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 appendResult67_g722 = (float4(IN.ase_texcoord.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				surfaceDescription.Alpha = lerpResult583;
				surfaceDescription.AlphaClipThreshold = 0.01;


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
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _HueBrightnessContrastSaturation;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float4 _MaskMap_ST;
			float3 _DickRootWorld;
			float3 _DickForwardWorld;
			float3 _DickRightWorld;
			float3 _DickUpWorld;
			float _SquashStretchCorrection;
			float _DistanceToHole;
			float _DickWorldLength;
			float _StartClip;
			float _EndClip;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			float3x3 ChangeOfBasis9_g722( float3 right, float3 up, float3 forward )
			{
				float3x3 basisTransform = 0;
				    basisTransform[0][0] = right.x;
				    basisTransform[0][1] = right.y;
				    basisTransform[0][2] = right.z;
				    basisTransform[1][0] = up.x;
				    basisTransform[1][1] = up.y;
				    basisTransform[1][2] = up.z;
				    basisTransform[2][0] = forward.x;
				    basisTransform[2][1] = forward.y;
				    basisTransform[2][2] = forward.z;
				return basisTransform;
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


				float localToCatmullRomSpace_float56_g722 = ( 0.0 );
				float3 worldDickRootPos56_g722 = _DickRootWorld;
				float3 right9_g722 = _DickRightWorld;
				float3 up9_g722 = _DickUpWorld;
				float3 forward9_g722 = _DickForwardWorld;
				float3x3 localChangeOfBasis9_g722 = ChangeOfBasis9_g722( right9_g722 , up9_g722 , forward9_g722 );
				float4 appendResult67_g722 = (float4(v.vertex.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float3 temp_output_12_0_g722 = mul( localChangeOfBasis9_g722, ( temp_output_68_0_g722 - _DickRootWorld ) );
				float3 break15_g722 = temp_output_12_0_g722;
				float temp_output_18_0_g722 = ( break15_g722.z * _SquashStretchCorrection );
				float3 appendResult26_g722 = (float3(break15_g722.x , break15_g722.y , temp_output_18_0_g722));
				float3 appendResult25_g722 = (float3(( break15_g722.x / _SquashStretchCorrection ) , ( break15_g722.y / _SquashStretchCorrection ) , temp_output_18_0_g722));
				float temp_output_17_0_g722 = ( _DistanceToHole * 0.5 );
				float smoothstepResult23_g722 = smoothstep( 0.0 , temp_output_17_0_g722 , temp_output_18_0_g722);
				float smoothstepResult22_g722 = smoothstep( _DistanceToHole , temp_output_17_0_g722 , temp_output_18_0_g722);
				float3 lerpResult31_g722 = lerp( appendResult26_g722 , appendResult25_g722 , min( smoothstepResult23_g722 , smoothstepResult22_g722 ));
				float3 lerpResult32_g722 = lerp( lerpResult31_g722 , ( temp_output_12_0_g722 + ( ( _DistanceToHole - ( _DickWorldLength * ( _DistanceToHole / ( _SquashStretchCorrection * _DickWorldLength ) ) ) ) * float3(0,0,1) ) ) , step( _DistanceToHole , temp_output_18_0_g722 ));
				float3 temp_output_37_0_g722 = ( _DickRootWorld + mul( transpose( localChangeOfBasis9_g722 ), lerpResult32_g722 ) );
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float3 lerpResult106_g722 = lerp( ( _DickRootWorld + ( _DickForwardWorld * break15_g722.z ) ) , temp_output_37_0_g722 , temp_output_54_0_g722);
				float3 worldPosition56_g722 = lerpResult106_g722;
				float3 worldDickForward56_g722 = _DickForwardWorld;
				float3 worldDickUp56_g722 = _DickUpWorld;
				float3 worldDickRight56_g722 = _DickRightWorld;
				float4 appendResult86_g722 = (float4(v.ase_normal , 0.0));
				float3 normalizeResult87_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult86_g722 )).xyz );
				float3 worldNormal56_g722 = normalizeResult87_g722;
				float4 break93_g722 = v.ase_tangent;
				float4 appendResult89_g722 = (float4(break93_g722.x , break93_g722.y , break93_g722.z , 0.0));
				float3 normalizeResult91_g722 = normalize( (mul( GetObjectToWorldMatrix(), appendResult89_g722 )).xyz );
				float4 appendResult94_g722 = (float4(normalizeResult91_g722 , break93_g722.w));
				float4 worldTangent56_g722 = appendResult94_g722;
				float3 worldPositionOUT56_g722 = float3( 0,0,0 );
				float3 worldNormalOUT56_g722 = float3( 0,0,0 );
				float4 worldTangentOUT56_g722 = float4( 0,0,0,0 );
				{
				ToCatmullRomSpace_float(worldDickRootPos56_g722,worldPosition56_g722,worldDickForward56_g722,worldDickUp56_g722,worldDickRight56_g722,worldNormal56_g722,worldTangent56_g722,worldPositionOUT56_g722,worldNormalOUT56_g722,worldTangentOUT56_g722);
				}
				float4 appendResult73_g722 = (float4(worldPositionOUT56_g722 , 1.0));
				float4 transform72_g722 = mul(GetWorldToObjectMatrix(),appendResult73_g722);
				float3 lerpResult575 = lerp( v.vertex.xyz , (transform72_g722).xyz , v.ase_color.r);
				
				float4 appendResult75_g722 = (float4(worldNormalOUT56_g722 , 0.0));
				float3 normalizeResult76_g722 = normalize( (mul( GetWorldToObjectMatrix(), appendResult75_g722 )).xyz );
				float3 lerpResult578 = lerp( v.ase_normal , normalizeResult76_g722 , v.ase_color.r);
				
				o.ase_texcoord = v.vertex;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult575;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult578;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 appendResult67_g722 = (float4(IN.ase_texcoord.xyz , 1.0));
				float4 transform66_g722 = mul(GetObjectToWorldMatrix(),appendResult67_g722);
				float3 temp_output_68_0_g722 = (transform66_g722).xyz;
				float dotResult42_g722 = dot( _DickForwardWorld , ( temp_output_68_0_g722 - _DickRootWorld ) );
				float temp_output_54_0_g722 = ( 1.0 - ( saturate( ( -( _StartClip - dotResult42_g722 ) * 10.0 ) ) * saturate( ( -( dotResult42_g722 - _EndClip ) * 10.0 ) ) ) );
				float lerpResult583 = lerp( 1.0 , temp_output_54_0_g722 , IN.ase_color.r);
				
				surfaceDescription.Alpha = lerpResult583;
				surfaceDescription.AlphaClipThreshold = 0.01;


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
Node;AmplifyShaderEditor.CommentaryNode;206;6016.412,-2050.383;Inherit;False;1888.192;1147.05;FragmentShader;11;106;103;100;104;107;101;102;108;105;445;553;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;579;6421.92,-768.8914;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;574;6326.138,-217.7695;Inherit;False;PenetratorDeformationShrink;1;;722;ad4a380768980ef49a79fe23c545abef;0;3;64;FLOAT3;0,0,0;False;69;FLOAT3;0,0,0;False;71;FLOAT4;0,0,0,0;False;4;FLOAT3;61;FLOAT3;62;FLOAT4;63;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;577;6410.317,-918.1694;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;576;6424.282,-631.2347;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;553;6564.251,-1114.017;Inherit;False;PenetratorDeformation;11;;725;034c1604581464e459076bc562dc2e05;0;3;64;FLOAT3;0,0,0;False;69;FLOAT3;0,0,0;False;71;FLOAT4;0,0,0,0;False;4;FLOAT3;61;FLOAT3;62;FLOAT4;63;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;575;6971.538,-903.3983;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;445;7295.528,-1194.208;Inherit;False;Constant;_Float1;Float 1;28;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;103;6204.022,-1988.126;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;578;6960.717,-756.5696;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;581;6993.785,-586.9204;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;583;6985.66,-429.6824;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TangentVertexDataNode;582;6421.854,-466.7342;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;108;7117.321,-1484.313;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;104;6584.06,-2000.383;Inherit;True;Property;_DecalColorMap;DecalColorMap;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;107;6529.273,-1778.095;Inherit;False;HueShift;-1;;726;1952e423258605d4aaa526c67ba2eb7c;0;2;2;FLOAT4;0,0.5,0.5,0.5;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;101;6066.412,-1349.593;Inherit;True;Property;_BumpMap;BumpMap;23;0;Create;True;0;0;0;False;0;False;-1;None;9c44ea8cd9bad9a41b2e1c4b503546e2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;106;6133.625,-1815.368;Inherit;False;Property;_HueBrightnessContrastSaturation;_HueBrightnessContrastSaturation;24;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.5019608,0.5019608,0.5019608;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;100;6070.291,-1564.565;Inherit;True;Property;_MainTex;MainTex;21;0;Create;True;0;0;0;False;0;False;-1;None;c6a51a68e5768654f8e614a5d167aefd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;102;6070.819,-1133.333;Inherit;True;Property;_MaskMap;MaskMap;22;0;Create;True;0;0;0;False;0;False;-1;None;aef0d52182fe29d48985b053faf59e23;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;105;7151.767,-1775.192;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;569;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;GBuffer;0;7;GBuffer;1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;562;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;565;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;563;7623.605,-1459.329;Float;False;True;-1;2;UnityEditor.ShaderGraphLitGUI;0;14;Custom/Dick;5b1861a142b3d4e45ba1bb5742a4fa5f;True;Forward;0;1;Forward;20;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.5,False,;0;  Scattering;2,False,;0;  Direct;0.9,False,;0;  Ambient;0.1,False,;0;  Shadow;0.5,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;0;637951711729303007;LOD CrossFade;0;637951711720212949;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;0;637937904191780019;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;False;True;False;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;566;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;568;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;DepthNormals;0;6;DepthNormals;1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;571;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;570;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;567;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;564;7623.605,-1459.329;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;17;New Amplify Shader;5b1861a142b3d4e45ba1bb5742a4fa5f;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;575;0;577;0
WireConnection;575;1;574;61
WireConnection;575;2;576;1
WireConnection;578;0;579;0
WireConnection;578;1;574;62
WireConnection;578;2;576;1
WireConnection;581;0;582;0
WireConnection;581;1;574;63
WireConnection;581;2;576;1
WireConnection;583;1;574;0
WireConnection;583;2;576;1
WireConnection;108;0;102;4
WireConnection;108;2;104;4
WireConnection;104;1;103;0
WireConnection;107;2;106;0
WireConnection;107;3;100;0
WireConnection;105;0;107;0
WireConnection;105;1;104;0
WireConnection;105;2;104;4
WireConnection;563;0;105;0
WireConnection;563;1;101;0
WireConnection;563;3;102;1
WireConnection;563;4;108;0
WireConnection;563;6;583;0
WireConnection;563;7;445;0
WireConnection;563;8;575;0
WireConnection;563;10;578;0
WireConnection;563;20;581;0
ASEEND*/
//CHKSM=D3C95BE5E7C5B9FB8957A72AC330C0404B600D3A
