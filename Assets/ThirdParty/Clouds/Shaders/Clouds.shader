Shader "VolumetricClouds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        BlueNoise ("Blue Noise", 2D) = "white" {}
        lightIntensity ("Light Intensity", Range (0, 1)) = 1
        scale("Cloud Scale", Float) = 0.62
        densityMultiplier("Cloud Density Multiplier", Float) = 1
        densityOffset("Cloud Density Offset", Float) = 0
        lightAbsorptionThroughCloud("Light Absorption Through Cloud", Float) = 0.75
        lightAbsorptionTowardSun("Light Absorption Toward Sun", Float) = 1 
        darknessThreshold("Darkness Threshold", Float) = 0.15
        rayOffsetStrength("Ray Offset Strength", Float) = 10 
        shadowMin ("Shadows Min", Range (0, 1)) = 0
        shadowMax ("Shadows Max", Range (0, 1)) = 1
        detailNoiseScale ("Detail Noise Scale", Float) = 3
        detailNoiseWeight ("Detail Noise Weight", Float) = 3.42
        shapeOffset ("Shape Offset", Vector) = (0,0,0)
        detailOffset ("Detail Offset", Vector) = (0,0,0)
        detailWeights ("Detail Weights", Vector) = (1,0.5,0.5)
        shapeNoiseWeights ("Shape Noise Weights", Vector) = (1,0.48,0.15)
        phaseParams ("Forward Scattering, Back Scattering, Brightness, Phase Factor", Vector) = (.811,.33,1,.488)
        boundsMin ("Bounds Min", Vector) = (0,0,0)
        boundsMax ("Bounds Max", Vector) = (1,1,1)
        numStepsLight ("Num Steps Light", Int) = 8
        timeScale ("Time Scale", Float) = 1
        baseSpeed ("Base Speed", Float) = 1
        detailSpeed ("Detail Speed", Float) = 1
        colA ("Cloud Color", Color) = (1,1,1,1)
    }
    HLSLINCLUDE
        #pragma target 4.5

        #pragma multi_compile_local CLOUD_SHADOWS_OFF CLOUD_SHADOWS_ON


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

            //#include "Assets/Clouds/Scripts/Clouds/Shaders/CloudDebug.cginc"

            struct appdata {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4x4 inverseCameraProjectionMatrix;
            
            v2f vert (appdata v) {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.pos = GetFullScreenTriangleVertexPosition(v.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(v.vertexID);

                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float4 viewVector = mul(unity_CameraInvProjection, float4(output.uv * 2 - 1, 0, -1));
//UNITY_MATRIX_I_VP
//_InvProjMatrix
                //viewVector /= viewVector.w;
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector.xyz,0)).xyz;
                //output.viewVector = viewVector.xyz;
                return output;
            }

            // Textures
            Texture3D<float4> NoiseTex;
            Texture3D<float4> DetailNoiseTex;
            //Texture2D<float4> WeatherMap;
            Texture2D<float4> BlueNoise;
            
            SamplerState samplerNoiseTex;
            SamplerState samplerDetailNoiseTex;
            //SamplerState samplerWeatherMap;
            SamplerState samplerBlueNoise;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            //sampler2D _CameraDepthTexture;

            // Shape settings
            float densityMultiplier;
            float densityOffset;
            float scale;
            float detailNoiseScale;
            float detailNoiseWeight;
            float3 detailWeights;
            float4 shapeNoiseWeights;
            float4 phaseParams;

            // March settings
            int numStepsLight;
            float rayOffsetStrength;

            float3 boundsMin;
            float3 boundsMax;

            float3 shapeOffset;
            float3 detailOffset;

            float shadowMin;
            float shadowMax;

            // Light settings
            float lightAbsorptionTowardSun;
            float lightAbsorptionThroughCloud;
            float darknessThreshold;
            float lightIntensity;
            //float3 _MainLightPosition;
            //float4 _MainLightColor;

            // Animation settings
            float timeScale;
            float baseSpeed;
            float detailSpeed;

            // Debug settings:
            int debugViewMode; // 0 = off; 1 = shape tex; 2 = detail tex; 3 = weathermap
            int debugGreyscale;
            int debugShowAllChannels;
            float debugNoiseSliceDepth;
            float4 debugChannelWeight;
            float debugTileAmount;
            float viewerSize;

            float4 colA;
            
            float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
                return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
            }

            float2 squareUV(float2 uv) {
                float width = _ScreenParams.x;
                float height =_ScreenParams.y;
                //float minDim = min(width, height);
                float scale = 1000;
                float x = uv.x * width;
                float y = uv.y * height;
                return float2 (x/scale, y/scale);
            }

            // Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                // Adapted from: http://jcgt.org/published/0007/03/04/
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            // Henyey-Greenstein
            float hg(float a, float g) {
                float g2 = g*g;
                return (1-g2) / (4*3.1415*pow(abs(1+g2-2*g*(a)), 1.5));
            }

            float phase(float a) {
                float blend = .5;
                float hgBlend = hg(a,phaseParams.x) * (1-blend) + hg(a,-phaseParams.y) * blend;
                return phaseParams.z + hgBlend*phaseParams.w;
            }

            float beer(float d) {
                float beer = exp(-d);
                return beer;
            }

            float remap01(float v, float low, float high) {
                return (v-low)/(high-low);
            }

            float sampleDensity(float3 rayPos) {
                // Constants:
                const int mipLevel = 0;
                const float baseScale = 1/1000.0;
                const float offsetSpeed = 1/100.0;

                // Calculate texture sample positions
                float time = _Time.x * timeScale;
                float3 size = boundsMax - boundsMin;
                float3 boundsCentre = (boundsMin+boundsMax) * .5;
                float3 uvw = (size * .5 + rayPos) * baseScale * scale;
                float3 shapeSamplePos = uvw + shapeOffset * offsetSpeed + float3(time,time*0.1,time*0.2) * baseSpeed;

                // Calculate falloff at along x/z edges of the cloud container
                const float containerEdgeFadeDst = 50;
                float dstFromEdgeX = min(containerEdgeFadeDst, min(rayPos.x - boundsMin.x, boundsMax.x - rayPos.x));
                float dstFromEdgeZ = min(containerEdgeFadeDst, min(rayPos.z - boundsMin.z, boundsMax.z - rayPos.z));
                float edgeWeight = min(dstFromEdgeZ,dstFromEdgeX)/containerEdgeFadeDst;
                
                // Calculate height gradient from weather map
                //float2 weatherUV = (size.xz * .5 + (rayPos.xz-boundsCentre.xz)) / max(size.x,size.z);
                //float weatherMap = WeatherMap.SampleLevel(samplerWeatherMap, weatherUV, mipLevel).x;
                float gMin = .2;
                float gMax = .7;
                float heightPercent = (rayPos.y - boundsMin.y) / size.y;
                float heightGradient = saturate(remap(heightPercent, 0.0, gMin, 0, 1)) * saturate(remap(heightPercent, 1, gMax, 0, 1));
                heightGradient *= edgeWeight;

                // Calculate base shape density
                float4 shapeNoise = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);
                float4 normalizedShapeWeights = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBM = dot(shapeNoise, normalizedShapeWeights);
                float baseShapeDensity = (shapeFBM + densityOffset * .1) * heightGradient;

                // Save sampling from detail tex if shape density <= 0
                if (baseShapeDensity > 0) {
                    // Sample detail noise
                    float3 detailSamplePos = uvw*detailNoiseScale + detailOffset * offsetSpeed + float3(time*.4,-time,time*0.1)*detailSpeed;
                    float4 detailNoise = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, detailSamplePos, mipLevel);
                    float3 normalizedDetailWeights = detailWeights / dot(detailWeights, 1);
                    float detailFBM = dot(detailNoise.xyz, normalizedDetailWeights);

                    // Subtract detail noise from base shape (weighted by inverse density so that edges get eroded more than centre)
                    float oneMinusShape = 1 - shapeFBM;
                    float detailErodeWeight = oneMinusShape * oneMinusShape * oneMinusShape;
                    float cloudDensity = baseShapeDensity - (1-detailFBM) * detailErodeWeight * detailNoiseWeight;
    
                    return cloudDensity * densityMultiplier * 0.1;
                }
                return 0;
            }

            // Calculate proportion of light that reaches the given point from the lightsource
            float lightmarch(float3 position) {
                float3 dirToLight = _MainLightPosition.xyz;
                float dstInsideBox = rayBoxDst(boundsMin, boundsMax, position, 1/dirToLight).y;
                
                float stepSize = dstInsideBox/numStepsLight;
                float totalDensity = 0;

                for (int step = 0; step < numStepsLight; step ++) {
                    position += dirToLight * stepSize;
                    totalDensity += max(0, sampleDensity(position) * stepSize);
                }

                float transmittance = lerp(1,exp(-totalDensity * lightAbsorptionTowardSun), abs(_MainLightPosition.y));
                return darknessThreshold + transmittance * (1-darknessThreshold);
            }

            float4 debugDrawNoise(float2 uv) {

                float4 channels = 0;
                float3 samplePos = float3(uv.x,uv.y, debugNoiseSliceDepth);

                if (debugViewMode == 1) {
                    channels = NoiseTex.SampleLevel(samplerNoiseTex, samplePos, 0);
                }
                else if (debugViewMode == 2) {
                    channels = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, samplePos, 0);
                }
                else if (debugViewMode == 3) {
                    //channels = WeatherMap.SampleLevel(samplerWeatherMap, samplePos.xy, 0);
                }

                if (debugShowAllChannels) {
                    return channels;
                }
                else {
                    float4 maskedChannels = (channels*debugChannelWeight);
                    if (debugGreyscale || debugChannelWeight.w == 1) {
                        return dot(maskedChannels,1);
                    }
                    else {
                        return maskedChannels;
                    }
                }
            }

          
            float4 frag (v2f i) : SV_Target
            {
                //#if DEBUG_MODE == 1
                //if (debugViewMode != 0) {
                    //float width = _ScreenParams.x;
                    //float height =_ScreenParams.y;
                    //float minDim = min(width, height);
                    //float x = i.uv.x * width;
                    //float y = (1-i.uv.y) * height;
//
                    //if (x < minDim*viewerSize && y < minDim*viewerSize) {
                        //return debugDrawNoise(float2(x/(minDim*viewerSize)*debugTileAmount, y/(minDim*viewerSize)*debugTileAmount));
                    //}
                //}
                //#endif
                
                // Create ray
                float3 rayPos = _WorldSpaceCameraPos;
                float viewLength = length(i.viewVector);
                float3 rayDir = i.viewVector / viewLength;
                
                // Depth and cloud container intersection info:
                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
                //float nonlin_depth = tex2D(_CameraDepthTexture,i.uv);
                //PositionInputs posInput = GetPositionInput(i.pos.xy, _ScreenSize.zw, nonlin_depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
                //float depth = posInput.linearDepth * viewLength;
                float depth = LinearEyeDepth(nonlin_depth, _ZBufferParams) * viewLength;
                float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                // point of intersection with the cloud container
                float3 entryPoint = rayPos + rayDir * dstToBox;

                // random starting offset (makes low-res results noisy rather than jagged/glitchy, which is nicer)
                float randomOffset = BlueNoise.SampleLevel(samplerBlueNoise, squareUV(i.uv*3), 0);
                randomOffset *= rayOffsetStrength;
                
                // Phase function makes clouds brighter around sun
                float cosAngle = dot(rayDir, _MainLightPosition.xyz);
                float phaseVal = phase(cosAngle);

                float dstTravelled = randomOffset;
                float dstLimit = min(depth-dstToBox, dstInsideBox);
                
                
                
                const float stepSize = 11;

                // March through volume:
                float transmittance = 1;
                float3 lightEnergy = 0;

                while (dstTravelled < dstLimit) {
                    rayPos = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayPos);
                    
                    if (density > 0) {
                        float lightTransmittance = lightmarch(rayPos);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseVal;
                        transmittance *= exp(-density * stepSize * lightAbsorptionThroughCloud);
                    
                        // Exit early if T is close to zero as further samples won't affect the result much
                        if (transmittance < 0.01) {
                            break;
                        }
                    }
                    dstTravelled += stepSize;
                }


                // Shadows Calc ----------------------

                float hitTransmittance = 1;

                #ifdef CLOUD_SHADOWS_ON
                // get the world space hit position
                float3 hitPos = rayDir * depth + _WorldSpaceCameraPos;
                float3 hitNorm = _MainLightPosition.xyz;
                float2 hitRayToContainerInfo = rayBoxDst(boundsMin, boundsMax, hitPos, 1/hitNorm);
                float hitDstToBox = hitRayToContainerInfo.x;
                float hitDstInsideBox = hitRayToContainerInfo.y;

                // point of intersection with the cloud container
                float3 hitEntryPoint = hitPos + hitNorm * hitDstToBox;

                float hitDstTravelled = randomOffset;
                float hitDstLimit = hitDstInsideBox;

                // Make sure we're not drawing shadows on the clipping planes
                if (nonlin_depth < 1 && nonlin_depth > 0) {
                // March through volume: 
                    while (hitDstTravelled < hitDstLimit) {
                        rayPos = hitEntryPoint + hitNorm * hitDstTravelled;
                        float density = sampleDensity(rayPos);
                        if (density > 0) {
                            hitTransmittance *= exp(-density * stepSize * lightAbsorptionThroughCloud);
                            // Exit early if T is close to zero as further samples won't affect the result much
                            if (hitTransmittance < 0.01) {
                                break;
                            }
                        }
                        hitDstTravelled += stepSize;
                    }
                }
                hitTransmittance = lerp(1,remap( hitTransmittance, 0, 1, shadowMin, shadowMax),abs(_MainLightPosition.y));
                #endif
                // Shadows done

                // Add clouds to background
                float3 backgroundCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                float3 cloudCol = (saturate(_MainLightColor.xyz) * lightIntensity) + (colA.xyz * (1-lightIntensity));
                float3 col = (backgroundCol * transmittance) * hitTransmittance + cloudCol * lightEnergy;
                return float4(col,0);
            }

            ENDHLSL
        }
    }
}