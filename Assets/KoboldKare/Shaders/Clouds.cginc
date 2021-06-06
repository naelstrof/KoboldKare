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
    return (1-g2) / (4*3.1415*pow(1+g2-2*g*(a), 1.5));
}

float phase(float a, float4 phaseParams) {
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

uniform float densityMultiplier;
uniform float densityOffset;
uniform float scale;
uniform float detailNoiseScale;
uniform float detailNoiseWeight;
uniform float3 detailWeights;
uniform float4 shapeNoiseWeights;
uniform float4 phaseParams;

// March settings
uniform int numStepsLight;
uniform float rayOffsetStrength;

uniform float3 boundsMin;
uniform float3 boundsMax;

uniform float3 shapeOffset;
uniform float3 detailOffset;

uniform float shadowMin;
uniform float shadowMax;

// Light settings
uniform float lightAbsorptionTowardSun;
uniform float lightAbsorptionThroughCloud;
uniform float darknessThreshold;
//float3 _MainLightPosition;
//float4 _MainLightColor;

// Animation settings
uniform float timeScale;
uniform float baseSpeed;
uniform float detailSpeed;

uniform sampler3D NoiseTex;
uniform sampler3D DetailNoiseTex;

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
    //float4 shapeNoise = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);
    float4 shapeNoise = tex3Dlod(NoiseTex, float4(shapeSamplePos, mipLevel));
    float4 normalizedShapeWeights = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
    float shapeFBM = dot(shapeNoise, normalizedShapeWeights);
    float baseShapeDensity = (shapeFBM + densityOffset * .1) * heightGradient;

    // Save sampling from detail tex if shape density <= 0
    if (baseShapeDensity > 0) {
        // Sample detail noise
        float3 detailSamplePos = uvw*detailNoiseScale + detailOffset * offsetSpeed + float3(time*.4,-time,time*0.1)*detailSpeed;
        //float4 detailNoise = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, detailSamplePos, mipLevel);
        float4 detailNoise = tex3Dlod(DetailNoiseTex, float4(detailSamplePos, mipLevel));
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
float lightmarch(float3 position, float3 lightDir) {
    float3 dirToLight = lightDir.xyz;
    float dstInsideBox = rayBoxDst(boundsMin, boundsMax, position, 1/dirToLight).y;
    
    float stepSize = dstInsideBox/numStepsLight;
    float totalDensity = 0;

    for (int step = 0; step < numStepsLight; step ++) {
        position += dirToLight * stepSize;
        totalDensity += max(0, sampleDensity(position) * stepSize);
    }

    float transmittance = exp(-totalDensity * lightAbsorptionTowardSun);
    return darknessThreshold + transmittance * (1-darknessThreshold);
}