// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Kobold"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Toggle(_PENETRATION_DEFORMATION_DETAIL_ON)] _PENETRATION_DEFORMATION_DETAIL("_PENETRATION_DEFORMATION_DETAIL", Float) = 0
		[Toggle(_PENETRATION_DEFORMATION_ON)] _PENETRATION_DEFORMATION("_PENETRATION_DEFORMATION", Float) = 0
		_BaseColorMap("BaseColorMap", 2D) = "white" {}
		_DecalColorMap("DecalColorMap", 2D) = "black" {}
		_MaskMap("MaskMap", 2D) = "gray" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		[NoScaleOffset]_DetailNormalMap("DetailNormalMap", 2D) = "bump" {}
		_HueBrightnessContrastSaturation("_HueBrightnessContrastSaturation", Vector) = (0,0.5,0.5,0.5)
		_Head("Head", Range( 0 , 1)) = 1
		_ThicknessMap("ThicknessMap", 2D) = "gray" {}
		_SubsurfaceColor("SubsurfaceColor", Color) = (0.8396226,0.6059541,0.6059541,1)
		_BoobLerp("BoobLerp", Range( 0 , 1)) = 0
		_DetailTiling("DetailTiling", Range( 0 , 10)) = 3
		_DetailAlpha("DetailAlpha", Range( 0 , 1)) = 1
		_CompressibleDistance("CompressibleDistance", Range( 0 , 1)) = 0
		_DetailNormalScale("DetailNormalScale", Range( 0 , 3)) = 1
		_Smoothness("Smoothness", Range( 0 , 10)) = 0
		[ASEEnd][NoScaleOffset]_DetailMaskMap("DetailMaskMap", 2D) = "gray" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
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
			Tags { "LightMode"="UniversalForwardOnly" }
			
			Blend One Zero, One Zero
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110


#pragma multi_compile __ _SCREEN_SPACE_OCCLUSION
#pragma multi_compile __ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile __ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
#pragma multi_compile __ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile __ _SHADOWS_SOFT
#pragma multi_compile __ _MIXED_LIGHTING_SUBTRACTIVE
			
// Disabled by Shader Control: #pragma multi_compile __ LIGHTMAP_SHADOW_MIXING
#pragma multi_compile __ SHADOWS_SHADOWMASK

// Disabled by Shader Control: #pragma multi_compile __ DIRLIGHTMAP_COMBINED
// Disabled by Shader Control: #pragma multi_compile __ LIGHTMAP_ON
#pragma multi_compile __ DYNAMICLIGHTMAP_ON

#pragma multi_compile __ _REFLECTION_PROBE_BLENDING
#pragma multi_compile __ _REFLECTION_PROBE_BOX_PROJECTION
#pragma multi_compile __ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
#pragma multi_compile __ _LIGHT_LAYERS
			
#pragma multi_compile __ _LIGHT_COOKIES
#pragma multi_compile __ _CLUSTERED_RENDERING

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
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
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
			float4 _JiggleInfos[16];
			sampler2D _DetailMaskMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			sampler2D _DetailNormalMap;
			sampler2D _NormalMap;
			sampler2D _MaskMap;
			sampler2D _ThicknessMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _NormalMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MaskMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _ThicknessMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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

				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.texcoord2;
				texCoord3_g37.xy = v.texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_texcoord8.zw = v.texcoord1.xy;
				o.ase_color = v.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord8.xy * temp_cast_0 + float2( 0,0 );
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord8.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord8.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float fresnelNdotV16_g36 = dot( WorldNormal, WorldViewDirection );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				float4 appendResult72 = (float4(break76.r , break76.g , break76.b , temp_output_70_0));
				
				float lerpResult108 = lerp( 0.0 , _DetailNormalScale , tex2DNode113.a);
				float3 unpack101 = UnpackNormalScale( tex2D( _DetailNormalMap, texCoord104 ), lerpResult108 );
				unpack101.z = lerp( 1, unpack101.z, saturate(lerpResult108) );
				float4 _NormalMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_NormalMap_ST);
				float2 uv_NormalMap = IN.ase_texcoord8.xy * _NormalMap_ST_Instance.xy + _NormalMap_ST_Instance.zw;
				
				float4 _MaskMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_MaskMap_ST);
				float2 uv_MaskMap = IN.ase_texcoord8.xy * _MaskMap_ST_Instance.xy + _MaskMap_ST_Instance.zw;
				float4 tex2DNode16 = tex2D( _MaskMap, uv_MaskMap );
				float lerpResult49 = lerp( 1.0 , tex2DNode16.g , _BoobLerp);
				float4 appendResult73 = (float4(tex2DNode16.r , lerpResult49 , tex2DNode16.b , tex2DNode16.a));
				float4 break10_g36 = appendResult73;
				float4 appendResult11_g36 = (float4(break10_g36.r , break10_g36.g , break10_g36.b , break10_g36.a));
				float4 break75 = appendResult11_g36;
				
				float temp_output_99_20 = tex2DNode3_g36.a;
				float4 _ThicknessMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_ThicknessMap_ST);
				float2 uv_ThicknessMap = IN.ase_texcoord8.xy * _ThicknessMap_ST_Instance.xy + _ThicknessMap_ST_Instance.zw;
				float4 temp_output_97_0 = ( ( 1.0 - temp_output_99_20 ) * ( ( 1.0 - tex2D( _ThicknessMap, uv_ThicknessMap ) ) * _SubsurfaceColor ) );
				
				float3 Albedo = appendResult72.xyz;
				float3 Normal = BlendNormal( unpack101 , UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = break75.x;
				float Smoothness = break75.w;
				float Occlusion = 1;
				float Alpha = temp_output_70_0;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.0;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = temp_output_97_0.rgb;
				float3 Translucency = temp_output_97_0.rgb;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0.85;
				#endif
				
				#ifdef _CLEARCOAT
				float CoatMask = temp_output_99_20;
				float CoatSmoothness = 0.85;
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
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 120110

			
			#pragma vertex vert
			#pragma fragment frag

#pragma multi_compile __ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
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
			float4 _JiggleInfos[16];
			sampler2D _DetailMaskMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.ase_texcoord2;
				texCoord3_g37.xy = v.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord2.zw = v.ase_texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord2.xy * temp_cast_0 + float2( 0,0 );
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord2.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord2.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float fresnelNdotV16_g36 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				
				float Alpha = temp_output_70_0;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.0;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0.85;
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
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
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
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
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
			float4 _JiggleInfos[16];
			sampler2D _DetailMaskMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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

				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.ase_texcoord2;
				texCoord3_g37.xy = v.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord2.zw = v.ase_texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord2.xy * temp_cast_0 + float2( 0,0 );
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord2.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord2.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float fresnelNdotV16_g36 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				
				float Alpha = temp_output_70_0;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0.85;
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
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
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
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 worldTangent : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
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
			float4 _JiggleInfos[16];
			sampler2D _DetailNormalMap;
			sampler2D _DetailMaskMap;
			sampler2D _NormalMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _NormalMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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

				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.ase_texcoord2;
				texCoord3_g37.xy = v.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;
				o.ase_texcoord4.zw = v.ase_texcoord1.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				o.ase_tangent = v.ase_tangent;
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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
				
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord4.xy * temp_cast_0 + float2( 0,0 );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord4.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult108 = lerp( 0.0 , _DetailNormalScale , tex2DNode113.a);
				float3 unpack101 = UnpackNormalScale( tex2D( _DetailNormalMap, texCoord104 ), lerpResult108 );
				unpack101.z = lerp( 1, unpack101.z, saturate(lerpResult108) );
				float4 _NormalMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_NormalMap_ST);
				float2 uv_NormalMap = IN.ase_texcoord4.xy * _NormalMap_ST_Instance.xy + _NormalMap_ST_Instance.zw;
				
				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord4.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float fresnelNdotV16_g36 = dot( WorldNormal, ase_worldViewDir );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				
				float3 Normal = BlendNormal( unpack101 , UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f ) );
				float Alpha = temp_output_70_0;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0.85;
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
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
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
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float4 _JiggleInfos[16];
			sampler2D _DetailMaskMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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


				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.ase_texcoord2;
				texCoord3_g37.xy = v.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				o.ase_texcoord1.xyz = ase_worldPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				o.ase_texcoord.zw = v.ase_texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord.xy * temp_cast_0 + float2( 0,0 );
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = IN.ase_texcoord2.xyz;
				float fresnelNdotV16_g36 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				
				surfaceDescription.Alpha = temp_output_70_0;
				surfaceDescription.AlphaClipThreshold = 0.5;


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
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _CLEARCOAT 1
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
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_DETAIL_ON
#pragma shader_feature_local __ _PENETRATION_DEFORMATION_ON
			#pragma multi_compile_instancing
			#include "Packages/com.naelstrof.penetrationtech/Shaders/Penetration.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceColor;
			float _CompressibleDistance;
			float _Smoothness;
			float _DetailTiling;
			float _DetailAlpha;
			float _DetailNormalScale;
			float _BoobLerp;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float4 _JiggleInfos[16];
			sampler2D _DetailMaskMap;
			sampler2D _BaseColorMap;
			sampler2D _DecalColorMap;
			UNITY_INSTANCING_BUFFER_START(Kobold)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueBrightnessContrastSaturation)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColorMap_ST)
				UNITY_DEFINE_INSTANCED_PROP(float, _Head)
			UNITY_INSTANCING_BUFFER_END(Kobold)


			float3 GetSoftbodyOffset3_g38( float blend, float3 vertexPosition )
			{
				float3 vertexOffset = float3(0,0,0);
				for(int i=0;i<8;i++) {
				    float4 targetPosePositionRadius = _JiggleInfos[i*2];
				    float4 verletPositionBlend = _JiggleInfos[i*2+1];
				    float3 movement = (verletPositionBlend.xyz - targetPosePositionRadius.xyz);
				    float dist = distance(vertexPosition, targetPosePositionRadius.xyz);
				    float multi = 1-smoothstep(0,targetPosePositionRadius.w,dist);
				    vertexOffset += movement * multi * verletPositionBlend.w * blend;
				}
				return vertexOffset;
			}
			
			float4 MyCustomExpression1_g34( float4 hsbc, float4 startColor )
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


				float blend3_g38 = length( v.ase_color.r );
				float localGetDeformationFromPenetrators_float8_g37 = ( 0.0 );
				float4 appendResult17_g37 = (float4(v.vertex.xyz , 1.0));
				float4 transform16_g37 = mul(GetObjectToWorldMatrix(),appendResult17_g37);
				float3 worldPosition8_g37 = (transform16_g37).xyz;
				float4 texCoord3_g37 = v.ase_texcoord2;
				texCoord3_g37.xy = v.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float4 uv28_g37 = texCoord3_g37;
				float compressibleDistance8_g37 = _CompressibleDistance;
				float smoothness8_g37 = _Smoothness;
				float3 deformedPosition8_g37 = float3( 0,0,0 );
				{
				GetDeformationFromPenetrators_float(worldPosition8_g37,uv28_g37,compressibleDistance8_g37,smoothness8_g37,deformedPosition8_g37);
				}
				float4 appendResult21_g37 = (float4(deformedPosition8_g37 , 1.0));
				float4 transform19_g37 = mul(GetWorldToObjectMatrix(),appendResult21_g37);
				#ifdef _PENETRATION_DEFORMATION_ON
				float3 staticSwitch24_g37 = (transform19_g37).xyz;
				#else
				float3 staticSwitch24_g37 = v.vertex.xyz;
				#endif
				float3 lerpResult85 = lerp( v.vertex.xyz , staticSwitch24_g37 , v.ase_color.g);
				float3 vertexPosition3_g38 = lerpResult85;
				float3 localGetSoftbodyOffset3_g38 = GetSoftbodyOffset3_g38( blend3_g38 , vertexPosition3_g38 );
				
				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				o.ase_texcoord1.xyz = ase_worldPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				o.ase_texcoord.zw = v.ase_texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = ( localGetSoftbodyOffset3_g38 + lerpResult85 );
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
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
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float4 _HueBrightnessContrastSaturation_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_HueBrightnessContrastSaturation);
				float4 hsbc1_g34 = _HueBrightnessContrastSaturation_Instance;
				float2 temp_cast_0 = (_DetailTiling).xx;
				float2 texCoord104 = IN.ase_texcoord.xy * temp_cast_0 + float2( 0,0 );
				float4 break110 = tex2D( _DetailMaskMap, texCoord104 );
				float4 appendResult111 = (float4(break110.r , break110.g , break110.b , 1.0));
				float4 _BaseColorMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_BaseColorMap_ST);
				float2 uv_BaseColorMap = IN.ase_texcoord.xy * _BaseColorMap_ST_Instance.xy + _BaseColorMap_ST_Instance.zw;
				float4 blendOpSrc106 = appendResult111;
				float4 blendOpDest106 = tex2D( _BaseColorMap, uv_BaseColorMap );
				float2 uv_DetailMaskMap113 = IN.ase_texcoord.xy;
				float4 tex2DNode113 = tex2D( _DetailMaskMap, uv_DetailMaskMap113 );
				float lerpResult109 = lerp( 0.0 , _DetailAlpha , tex2DNode113.a);
				float4 lerpBlendMode106 = lerp(blendOpDest106,(( blendOpDest106 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest106 ) * ( 1.0 - blendOpSrc106 ) ) : ( 2.0 * blendOpDest106 * blendOpSrc106 ) ),lerpResult109);
				float4 startColor1_g34 = ( saturate( lerpBlendMode106 ));
				float4 localMyCustomExpression1_g34 = MyCustomExpression1_g34( hsbc1_g34 , startColor1_g34 );
				float2 texCoord2_g36 = IN.ase_texcoord.zw * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode3_g36 = tex2Dlod( _DecalColorMap, float4( texCoord2_g36, 0, 0.0) );
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = IN.ase_texcoord2.xyz;
				float fresnelNdotV16_g36 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode16_g36 = ( 0.6 + 1.0 * pow( max( 1.0 - fresnelNdotV16_g36 , 0.0001 ), 2.0 ) );
				float4 lerpResult7_g36 = lerp( localMyCustomExpression1_g34 , tex2DNode3_g36 , saturate( ( tex2DNode3_g36.a * fresnelNode16_g36 ) ));
				float4 break76 = lerpResult7_g36;
				float _Head_Instance = UNITY_ACCESS_INSTANCED_PROP(Kobold,_Head);
				float lerpResult44 = lerp( IN.ase_color.a , break76.a , _Head_Instance);
				float temp_output_70_0 = saturate( lerpResult44 );
				
				surfaceDescription.Alpha = temp_output_70_0;
				surfaceDescription.AlphaClipThreshold = 0.5;


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
Node;AmplifyShaderEditor.RangedFloatNode;103;-2106.743,-1071.583;Inherit;False;Property;_DetailTiling;DetailTiling;14;0;Create;True;0;0;0;False;0;False;3;3;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;115;-1791.284,-1505.856;Inherit;True;Property;_DetailMaskMap;DetailMaskMap;19;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;None;False;gray;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;104;-1793.436,-1093.945;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;114;-1441.121,-1513.814;Inherit;True;Property;_TextureSample1;Texture Sample 1;17;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;113;-1431.836,-1313.532;Inherit;True;Property;_TextureSample0;Texture Sample 0;17;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;110;-936.4202,-1010.37;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;105;-1659.686,-891.845;Inherit;False;Property;_DetailNormalScale;DetailNormalScale;17;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-1650.106,-774.2557;Inherit;False;Property;_DetailAlpha;DetailAlpha;15;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;111;-774.6392,-1020.496;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;16;-544.4865,-190.4665;Inherit;True;Property;_MaskMap;MaskMap;6;0;Create;True;0;0;0;False;0;False;-1;None;ba658213c23f3f044964ac264d664e2a;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-1274.608,-290.526;Inherit;True;Property;_BaseColorMap;BaseColorMap;3;0;Create;True;0;0;0;False;0;False;-1;None;ca5c93517dba7944da1b2fb875dd04e6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;-434.1562,203.4802;Inherit;False;Property;_BoobLerp;BoobLerp;13;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;108;-716.5261,-1284.627;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;109;-1029.526,-739.6274;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-68.73178,-952.5997;Inherit;True;Property;_NormalMap;NormalMap;7;0;Create;True;0;0;0;False;0;False;-1;None;3fa8181c8565718469155288933d2cba;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;19;-423.4813,-822.4724;Inherit;False;InstancedProperty;_HueBrightnessContrastSaturation;_HueBrightnessContrastSaturation;9;0;Create;True;0;0;0;False;0;False;0,0.5,0.5,0.5;0,0.5,0.5,0.5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;106;-691.9363,-660.3837;Inherit;False;Overlay;True;3;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;49;-58.92234,-50.32614;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;101;-567.3115,-1164.562;Inherit;True;Property;_DetailNormalMap;DetailNormalMap;8;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;73;413.1302,-257.8121;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BlendNormalsNode;102;317.6367,-1158.187;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;83;531.4572,827.8464;Inherit;False;Property;_CompressibleDistance;CompressibleDistance;16;0;Create;True;0;0;0;False;0;False;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;18;-312.1903,-453.5887;Inherit;False;HueShift;-1;;34;1952e423258605d4aaa526c67ba2eb7c;0;2;2;FLOAT4;0,0.5,0.5,0.5;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;84;542.8562,942.8711;Inherit;False;Property;_Smoothness;Smoothness;18;0;Create;True;0;0;0;False;0;False;0;3;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;78;555.9571,613.9916;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;99;730.9473,-426.799;Inherit;False;ApplyDecals;4;;36;d9b89e1202461fa45af2324780068fb2;0;3;4;COLOR;0,0,0,0;False;5;FLOAT3;0,0,0;False;6;COLOR;0,0,0,0;False;5;FLOAT;18;FLOAT;20;FLOAT3;14;FLOAT4;15;COLOR;13
Node;AmplifyShaderEditor.PosVertexDataNode;86;1170.772,363.0097;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;80;1019.166,691.4556;Inherit;False;PenetrableDeformation;0;;37;014b2db8766710a4c8429222ab5b0977;0;4;10;FLOAT3;0,0,0;False;11;FLOAT4;0,0,0,0;False;12;FLOAT;0;False;13;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;76;1448.69,-996.402;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.LerpOp;85;1496.326,394.0421;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LengthOpNode;79;752.6432,536.627;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;14;1125.933,-833.5338;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;36;910.418,-615.4736;Inherit;False;InstancedProperty;_Head;Head;10;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;77;1660.415,616.9894;Inherit;False;JigglePhysicsSoftbody;-1;;38;6ec46ef0369ac3449867136b98c25983;0;2;6;FLOAT3;0,0,0;False;10;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;44;1457.205,-762.8445;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;898.8593,121.2402;Inherit;False;Constant;_AlphaClip;AlphaClip;6;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;70;1674.173,-746.1651;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;1120.949,67.20074;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;68;900.7033,201.0084;Inherit;False;Constant;_Float0;Float 0;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;252.8287,578.3058;Inherit;False;Property;_SubsurfaceColor;SubsurfaceColor;12;0;Create;True;0;0;0;False;0;False;0.8396226,0.6059541,0.6059541,1;0.6037736,0.03078062,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;594.9385,324.4233;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;98;897.6778,-62.9167;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;2024.133,389.8536;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;41;312.6382,343.7343;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;40;-46.74971,347.6124;Inherit;True;Property;_ThicknessMap;ThicknessMap;11;0;Create;True;0;0;0;False;0;False;-1;None;55dc3839aea320b4d8e63b5b2d13b409;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;72;1738.825,-994.7527;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;75;1275.148,-128.7411;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;60;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;89;2152.365,-91.9964;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;65;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;63;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;62;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;67;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;64;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;66;925.674,-72.54388;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;88;2152.365,-91.9964;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;5;d3d11;glcore;gles;gles3;ps5;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;61;2152.365,-171.9964;Float;False;True;-1;2;UnityEditor.ShaderGraphLitGUI;0;12;Kobold;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;1;LightMode=UniversalForwardOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;1;0;  Transmission Shadow;1,False,;0;Translucency;1;0;  Translucency Strength;1,False,;0;  Normal Distortion;1,False,;0;  Scattering;2,False,;0;  Direct;1,False,;0;  Ambient;0.2,False,;0;  Shadow;1,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;1;0;Receive Shadows;1;0;GPU Instancing;0;0;LOD CrossFade;0;638123054158872191;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;0;638349143991051084;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;0;0;Debug Display;0;0;Clear Coat;1;637974989309817492;0;10;False;True;True;True;False;False;True;False;True;True;False;;False;0
WireConnection;104;0;103;0
WireConnection;114;0;115;0
WireConnection;114;1;104;0
WireConnection;113;0;115;0
WireConnection;110;0;114;0
WireConnection;111;0;110;0
WireConnection;111;1;110;1
WireConnection;111;2;110;2
WireConnection;108;1;105;0
WireConnection;108;2;113;4
WireConnection;109;1;107;0
WireConnection;109;2;113;4
WireConnection;106;0;111;0
WireConnection;106;1;15;0
WireConnection;106;2;109;0
WireConnection;49;1;16;2
WireConnection;49;2;48;0
WireConnection;101;1;104;0
WireConnection;101;5;108;0
WireConnection;73;0;16;1
WireConnection;73;1;49;0
WireConnection;73;2;16;3
WireConnection;73;3;16;4
WireConnection;102;0;101;0
WireConnection;102;1;17;0
WireConnection;18;2;19;0
WireConnection;18;3;106;0
WireConnection;99;4;18;0
WireConnection;99;5;102;0
WireConnection;99;6;73;0
WireConnection;80;12;83;0
WireConnection;80;13;84;0
WireConnection;76;0;99;13
WireConnection;85;0;86;0
WireConnection;85;1;80;0
WireConnection;85;2;78;2
WireConnection;79;0;78;1
WireConnection;77;6;85;0
WireConnection;77;10;79;0
WireConnection;44;0;14;4
WireConnection;44;1;76;3
WireConnection;44;2;36;0
WireConnection;70;0;44;0
WireConnection;97;0;98;0
WireConnection;97;1;43;0
WireConnection;43;0;41;0
WireConnection;43;1;42;0
WireConnection;98;0;99;20
WireConnection;87;0;77;0
WireConnection;87;1;85;0
WireConnection;41;0;40;0
WireConnection;72;0;76;0
WireConnection;72;1;76;1
WireConnection;72;2;76;2
WireConnection;72;3;70;0
WireConnection;75;0;99;15
WireConnection;61;0;72;0
WireConnection;61;1;99;14
WireConnection;61;3;75;0
WireConnection;61;4;75;3
WireConnection;61;6;70;0
WireConnection;61;7;39;0
WireConnection;61;16;68;0
WireConnection;61;14;97;0
WireConnection;61;15;97;0
WireConnection;61;18;99;20
WireConnection;61;19;99;18
WireConnection;61;8;87;0
ASEEND*/
//CHKSM=46C84B39D66775696B54BD86DCE117C9C144ED7D
