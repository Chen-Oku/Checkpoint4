// ToonHelpers.hlsl
// Helper functions to be used with Shader Graph Custom Function nodes.
// Matches behaviour from ToonShaderV2.shader: ToonQuant, rim, spec helpers.

#ifndef TOON_HELPERS_INCLUDED
#define TOON_HELPERS_INCLUDED

// Clamp helper
float Saturate(float x) { return clamp(x, 0.0, 1.0); }

// Toon quantization: maps NdotL [0..1] to discrete bands [0..1]
// steps: number of toon levels (>=1)
float ToonQuant(float NdotL, float steps)
{
    steps = max(1.0, steps);
    float s = round(steps);
    if (s <= 1.0)
        return 1.0;
    float v = Saturate(NdotL);
    float q = floor(v * s) / (s - 1.0);
    return Saturate(q);
}

// Specular approximation using half-vector
// normal, lightDir, viewDir are expected to be normalized.
float ToonSpecular(float3 normal, float3 lightDir, float3 viewDir, float glossiness)
{
    float3 halfVec = normalize(lightDir + viewDir);
    float NdotH = Saturate(dot(normal, halfVec));
    float specPow = pow(NdotH, glossiness);
    float specSmooth = smoothstep(0.0, 0.2, specPow);
    return specSmooth;
}

// Rim / Fresnel helper
// Returns rim factor in [0..1] based on view and light contributions
float ToonRim(float3 normal, float3 viewDir, float NdotL, float rimExponent, float rimThreshold, float rimSmooth, float rimMix)
{
    float viewDot = Saturate(dot(normal, viewDir));
    float fresnel = 1.0 - viewDot;
    float rimMask = smoothstep(rimThreshold - rimSmooth, rimThreshold + rimSmooth, fresnel);
    float rim_view = pow(rimMask, rimExponent);
    float rim_light = pow(Saturate(1.0 - NdotL), rimExponent);
    float rim = lerp(rim_view, rim_light, Saturate(rimMix));
    return rim;
}

#endif // TOON_HELPERS_INCLUDED
