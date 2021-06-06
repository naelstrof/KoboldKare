// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TentacleDeform"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_forward("forward", Vector) = (0,0,1,0)
		_up("up", Vector) = (0,1,0,0)
		_right("right", Vector) = (1,0,0,0)
		_OrifacePosition("OrifacePosition", Vector) = (0,0,0,0)
		_OrifaceNormal("OrifaceNormal", Vector) = (0,0,0,0)
		[ASEEnd]_Length("Length", Range( 0 , 10)) = 1

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
		#pragma target 2.0

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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult70;

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

				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult70;

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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult70;
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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult70;

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

				
				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float3 Emission = 0;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

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
			
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
			{
				float bezierUpness = dot( bezierDerivitive , up);
				float3 bezierUp = lerp( up , -forward , saturate( bezierUpness ));
				float bezierDownness = dot( bezierDerivitive , -up );
				return normalize( lerp( bezierUp , forward , saturate( bezierDownness )) );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult70;

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

				
				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = lerpResult70;
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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float3 _forward;
			float3 _OrifacePosition;
			float3 _OrifaceNormal;
			float3 _up;
			float3 _right;
			float _Length;
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
			

			float3 MyCustomExpression20_g45( float3 bezierDerivitive, float3 forward, float3 up )
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

				float3 temp_output_11_0_g48 = _forward;
				float3 worldToObj25 = mul( GetWorldToObjectMatrix(), float4( _OrifacePosition, 1 ) ).xyz;
				float temp_output_48_0 = length( worldToObj25 );
				float PenetratedDepth29 = saturate( ( _Length - temp_output_48_0 ) );
				float3 temp_output_14_0_g48 = ( ( -temp_output_11_0_g48 * ( 1.0 - PenetratedDepth29 ) ) + v.vertex.xyz );
				float dotResult22_g48 = dot( temp_output_11_0_g48 , temp_output_14_0_g48 );
				float3 worldToObjDir24 = mul( GetWorldToObjectMatrix(), float4( _OrifaceNormal, 0 ) ).xyz;
				float3 normalizeResult27_g49 = normalize( -worldToObjDir24 );
				float3 temp_output_7_0_g48 = normalizeResult27_g49;
				float3 temp_output_4_0_g48 = _up;
				float dotResult23_g48 = dot( temp_output_4_0_g48 , temp_output_14_0_g48 );
				float3 normalizeResult31_g49 = normalize( temp_output_4_0_g48 );
				float3 normalizeResult29_g49 = normalize( cross( normalizeResult27_g49 , normalizeResult31_g49 ) );
				float3 temp_output_7_1_g48 = cross( normalizeResult29_g49 , normalizeResult27_g49 );
				float3 temp_output_20_0_g48 = _right;
				float dotResult21_g48 = dot( temp_output_20_0_g48 , temp_output_14_0_g48 );
				float3 temp_output_7_2_g48 = normalizeResult29_g49;
				float3 temp_output_2_0_g43 = v.vertex.xyz;
				float3 temp_output_3_0_g43 = _forward;
				float dotResult6_g43 = dot( temp_output_2_0_g43 , temp_output_3_0_g43 );
				float VisibleLength40 = ( _Length - PenetratedDepth29 );
				float temp_output_20_0_g43 = ( dotResult6_g43 / VisibleLength40 );
				float temp_output_26_0_g44 = temp_output_20_0_g43;
				float temp_output_19_0_g44 = ( 1.0 - temp_output_26_0_g44 );
				float3 temp_output_8_0_g43 = float3( 0,0,0 );
				float3 temp_output_9_0_g43 = ( _forward * VisibleLength40 * 0.333 );
				float3 temp_output_10_0_g43 = ( worldToObj25 + ( worldToObjDir24 * VisibleLength40 * 0.333 ) );
				float3 temp_output_11_0_g43 = worldToObj25;
				float temp_output_1_0_g46 = temp_output_20_0_g43;
				float temp_output_8_0_g46 = ( 1.0 - temp_output_1_0_g46 );
				float3 temp_output_3_0_g46 = temp_output_9_0_g43;
				float3 temp_output_4_0_g46 = temp_output_10_0_g43;
				float3 temp_output_7_0_g45 = ( ( 3.0 * temp_output_8_0_g46 * temp_output_8_0_g46 * ( temp_output_3_0_g46 - temp_output_8_0_g43 ) ) + ( 6.0 * temp_output_8_0_g46 * temp_output_1_0_g46 * ( temp_output_4_0_g46 - temp_output_3_0_g46 ) ) + ( 3.0 * temp_output_1_0_g46 * temp_output_1_0_g46 * ( temp_output_11_0_g43 - temp_output_4_0_g46 ) ) );
				float3 bezierDerivitive20_g45 = temp_output_7_0_g45;
				float3 forward20_g45 = temp_output_3_0_g43;
				float3 temp_output_4_0_g43 = _up;
				float3 up20_g45 = temp_output_4_0_g43;
				float3 localMyCustomExpression20_g45 = MyCustomExpression20_g45( bezierDerivitive20_g45 , forward20_g45 , up20_g45 );
				float3 normalizeResult27_g47 = normalize( localMyCustomExpression20_g45 );
				float3 normalizeResult24_g45 = normalize( cross( temp_output_7_0_g45 , localMyCustomExpression20_g45 ) );
				float3 normalizeResult31_g47 = normalize( normalizeResult24_g45 );
				float3 normalizeResult29_g47 = normalize( cross( normalizeResult27_g47 , normalizeResult31_g47 ) );
				float3 temp_output_41_22_g43 = cross( normalizeResult29_g47 , normalizeResult27_g47 );
				float3 temp_output_5_0_g43 = _right;
				float dotResult15_g43 = dot( temp_output_2_0_g43 , temp_output_5_0_g43 );
				float3 temp_output_41_0_g43 = normalizeResult27_g47;
				float dotResult18_g43 = dot( temp_output_2_0_g43 , temp_output_4_0_g43 );
				float dotResult36 = dot( v.vertex.xyz , _forward );
				float temp_output_38_0 = saturate( sign( ( VisibleLength40 - dotResult36 ) ) );
				float3 lerpResult54 = lerp( ( ( dotResult22_g48 * temp_output_7_0_g48 ) + ( dotResult23_g48 * temp_output_7_1_g48 ) + ( dotResult21_g48 * temp_output_7_2_g48 ) + worldToObj25 ) , ( ( ( temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_19_0_g44 * temp_output_8_0_g43 ) + ( temp_output_19_0_g44 * temp_output_19_0_g44 * 3.0 * temp_output_26_0_g44 * temp_output_9_0_g43 ) + ( 3.0 * temp_output_19_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_10_0_g43 ) + ( temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_26_0_g44 * temp_output_11_0_g43 ) ) + ( temp_output_41_22_g43 * dotResult15_g43 ) + ( temp_output_41_0_g43 * dotResult18_g43 ) ) , temp_output_38_0);
				float temp_output_71_0 = saturate( ( ( temp_output_48_0 - _Length ) * 8.0 ) );
				float3 lerpResult67 = lerp( lerpResult54 , v.vertex.xyz , temp_output_71_0);
				
				float3 temp_output_24_0_g48 = v.ase_normal;
				float dotResult25_g48 = dot( temp_output_11_0_g48 , temp_output_24_0_g48 );
				float dotResult26_g48 = dot( temp_output_4_0_g48 , temp_output_24_0_g48 );
				float dotResult27_g48 = dot( temp_output_20_0_g48 , temp_output_24_0_g48 );
				float3 normalizeResult33_g48 = normalize( ( ( dotResult25_g48 * temp_output_7_0_g48 ) + ( dotResult26_g48 * temp_output_7_1_g48 ) + ( dotResult27_g48 * temp_output_7_2_g48 ) ) );
				float3 temp_output_21_0_g43 = v.ase_normal;
				float dotResult23_g43 = dot( temp_output_21_0_g43 , temp_output_3_0_g43 );
				float dotResult24_g43 = dot( temp_output_21_0_g43 , temp_output_4_0_g43 );
				float dotResult25_g43 = dot( temp_output_21_0_g43 , temp_output_5_0_g43 );
				float3 normalizeResult31_g43 = normalize( ( ( normalizeResult29_g47 * dotResult23_g43 ) + ( temp_output_41_0_g43 * dotResult24_g43 ) + ( temp_output_41_22_g43 * dotResult25_g43 ) ) );
				float3 lerpResult64 = lerp( normalizeResult33_g48 , normalizeResult31_g43 , temp_output_38_0);
				float3 lerpResult70 = lerp( lerpResult64 , v.ase_normal , temp_output_71_0);
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = lerpResult67;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = lerpResult70;

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

				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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
7;169;1675;823;1075.539;-14.53859;1.075239;True;False
Node;AmplifyShaderEditor.Vector3Node;21;-1203.107,635.8024;Inherit;False;Property;_OrifacePosition;OrifacePosition;3;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,10;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;25;-934.3206,664.3275;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LengthOpNode;48;-482.2173,584.0245;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1237.421,-746.223;Inherit;False;Property;_Length;Length;5;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;-252.7359,574.6752;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;31;-56.63159,561.7845;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;199.2906,571.8292;Inherit;False;PenetratedDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-1206.149,-622.1014;Inherit;False;29;PenetratedDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;32;-768.9017,-634.5361;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;15;-1177.729,-373.3927;Inherit;False;Property;_forward;forward;0;0;Create;True;0;0;0;False;0;False;0,0,1;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;22;-1217.95,854.765;Inherit;False;Property;_OrifaceNormal;OrifaceNormal;4;0;Create;True;0;0;0;False;0;False;0,0,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-605.9235,-394.1053;Inherit;False;VisibleLength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;1;-1196.059,-525.7519;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;24;-981.0281,849.497;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;44;-1484.062,485.2912;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0.333;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-932.548,1020.346;Inherit;False;40;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;36;-465.6913,-535.0052;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;37;-219.2686,-526.8465;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-1091.918,552.8815;Inherit;False;29;PenetratedDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-1285.317,135.9577;Inherit;False;40;VisibleLength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-646.7068,1027.786;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;52;-1064.701,380.2873;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-965.4761,138.0826;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;17;-1193.1,-27.07427;Inherit;False;Property;_right;right;2;0;Create;True;0;0;0;False;0;False;1,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;58;-848.6926,517.2569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-629.6446,803.4425;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SignOpNode;61;-63.97924,-501.5865;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;68;-49.49989,363.2491;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;16;-1199.732,-202.7614;Inherit;False;Property;_up;up;1;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;63;-1376.15,-391.5664;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;38;55.98738,-473.6723;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;178.1881,360.7655;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;65;-584.6359,-110.0326;Inherit;False;BeizerSpaceTransform;-1;;43;d8cd7e255e788cb4f9cacb136d95dad5;0;10;21;FLOAT3;0,0,0;False;19;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,1;False;4;FLOAT3;0,1,0;False;5;FLOAT3;1,0,0;False;8;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;2;FLOAT3;22;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;66;-548.1432,239.403;Inherit;False;OrifaceSpaceTransform;-1;;48;a2cb6c5fdae31044587a631065a2df2f;0;8;24;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;10;FLOAT;0;False;11;FLOAT3;0,0,0;False;20;FLOAT3;0,0,0;False;2;FLOAT3;34;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;54;-122.3116,-80.5788;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;64;-130.3704,145.3796;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;71;472.5205,406.8019;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;67;308.8506,-132.0099;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;70;351.1631,71.45693;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;10;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;9;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;838.4768,-197.5135;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;TentacleDeform;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;0;  Early Z;0;Vertex Position,InvertActionOnDeselection;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
WireConnection;25;0;21;0
WireConnection;48;0;25;0
WireConnection;28;0;19;0
WireConnection;28;1;48;0
WireConnection;31;0;28;0
WireConnection;29;0;31;0
WireConnection;32;0;19;0
WireConnection;32;1;33;0
WireConnection;40;0;32;0
WireConnection;24;0;22;0
WireConnection;36;0;1;0
WireConnection;36;1;15;0
WireConnection;37;0;40;0
WireConnection;37;1;36;0
WireConnection;42;0;24;0
WireConnection;42;1;41;0
WireConnection;42;2;44;0
WireConnection;45;0;15;0
WireConnection;45;1;46;0
WireConnection;45;2;44;0
WireConnection;58;0;53;0
WireConnection;26;0;25;0
WireConnection;26;1;42;0
WireConnection;61;0;37;0
WireConnection;68;0;48;0
WireConnection;68;1;19;0
WireConnection;38;0;61;0
WireConnection;72;0;68;0
WireConnection;65;21;63;0
WireConnection;65;19;40;0
WireConnection;65;2;1;0
WireConnection;65;3;15;0
WireConnection;65;4;16;0
WireConnection;65;5;17;0
WireConnection;65;9;45;0
WireConnection;65;10;26;0
WireConnection;65;11;25;0
WireConnection;66;24;63;0
WireConnection;66;1;24;0
WireConnection;66;4;16;0
WireConnection;66;5;25;0
WireConnection;66;6;52;0
WireConnection;66;10;58;0
WireConnection;66;11;15;0
WireConnection;66;20;17;0
WireConnection;54;0;66;0
WireConnection;54;1;65;0
WireConnection;54;2;38;0
WireConnection;64;0;66;34
WireConnection;64;1;65;22
WireConnection;64;2;38;0
WireConnection;71;0;72;0
WireConnection;67;0;54;0
WireConnection;67;1;1;0
WireConnection;67;2;71;0
WireConnection;70;0;64;0
WireConnection;70;1;63;0
WireConnection;70;2;71;0
WireConnection;4;8;67;0
WireConnection;4;10;70;0
ASEEND*/
//CHKSM=5E9736843BAA988A1F7D27DB65A234DD5A91591F