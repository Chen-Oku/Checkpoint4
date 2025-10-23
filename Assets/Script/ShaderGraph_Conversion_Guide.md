Shader Graph Conversion Guide (URP/HDRP)
========================================

This guide explains how to port the provided ShaderLab shaders to Shader Graph (recommended for URP/HDRP). It lists the nodes and exposed properties you should create so `PlayerVFXController` can animate them at runtime.

General notes
- Use Universal Render Pipeline (URP) or HDRP Shader Graph templates.
- Create a new PBR Graph (URP) or Lit Graph (HDRP) for each shader. For forward-only unlit glowing effects, create an Unlit Graph and add Emission manually.
- Expose properties (exposed to material) for anything you want to change from script: _EmissionStrength, _DissolveThreshold, _EdgeWidth, _ScrollSpeed, _Color, _NoiseTex.


1) SpeedPulse -> Shader Graph (Unlit)
-------------------------------------------------
Properties to create (exposed):
- MainTex (Texture2D)
- NoiseTex (Texture2D)
- Color (Color)
- EmissionStrength (Float)
- ScrollSpeed (Float)

Node steps (URP Unlit Graph):
1. Sample Texture2D `MainTex` -> UV from UV node.
2. Sample Texture2D `NoiseTex` -> UV = UV + Time * Vector2(ScrollSpeed, ScrollSpeed*0.3). Use Multiply node to scale Time.
3. Use Noise.r (Split or Vector1) as blend factor: Lerp(MainTex, Color, Noise.r).
4. For pulsing emission: Use Time node -> Multiply by (some frequency) -> Sine -> Remap from [-1,1] to [0,1] -> Multiply by EmissionStrength.
5. Add the emission contribution to the final color output (for Unlit Graph, connect to Color slot; for PBR graph connect to Emission).
6. Expose properties so you can animate ScrollSpeed and EmissionStrength from script.

Hints:
- For better detail add a Tiling and Offset property for NoiseTex.
- For soft glow in URP, set material Surface Type = Transparent and use additive blending if needed.


2) DissolveEdge -> Shader Graph (Unlit or PBR)
-------------------------------------------------
Properties to create (exposed):
- MainTex (Texture2D)
- NoiseTex (Texture2D)
- DissolveThreshold (Float)
- EdgeColor (Color)
- EdgeWidth (Float)

Node steps:
1. Sample NoiseTex at UV.
2. Compare Noise.r and DissolveThreshold using Subtract and Divide by EdgeWidth to create a smoothstep-like factor: smooth = saturate((Noise.r - Threshold) / EdgeWidth).
3. Use Lerp(EdgeColor, Sampled MainTex, smooth) to blend edge color into base.
4. Use smooth as alpha for the final output; optionally multiply alpha by a base texture alpha.
5. For a soft border, feed the smooth into a power node or smoothstep node to shape falloff.
6. Expose DissolveThreshold and EdgeWidth so you can animate dissolve from script.

Hints:
- To animate dissolve gradually, use a coroutine to update material.SetFloat("_DissolveThreshold", value).
- Consider adding a Noise Tiling property and a secondary animated noise (UV scroll) for dynamic dissolution.


3) ShieldFresnel -> Shader Graph (PBR/Lit or Unlit)
-------------------------------------------------
Properties to create (exposed):
- MainTex (Texture2D)
- Color (Color)
- FresnelPower (Float)
- FresnelStrength (Float)

Node steps:
1. Get Normal Vector (Normal Vector node) and World Space View Direction (View Direction node set to World).
2. Compute dot = saturate(dot(viewDir, normal)).
3. fresnel = pow(1 - dot, FresnelPower) * FresnelStrength.
4. Sample MainTex and multiply by Color.
5. Add fresnel * Color to the base color (as emission contribution or add to Albedo for Unlit Graph).
6. Optionally use fresnel to drive Alpha for translucent rim.

Hints:
- For refraction-like effects, use grab pass or screen-space distortion nodes available in HDRP, or fake with refraction normal maps.
- In URP, use an Unlit Graph with additively blended emmissive rim for shield look.


Animating properties from `PlayerVFXController`
-----------------------------------------------
- Use exposed material properties and animate them via MaterialPropertyBlock or material instances.
- Example coroutines:
  - Pulse: material.SetFloat("_EmissionStrength", Mathf.Lerp(a,b, sin...))
  - Dissolve: material.SetFloat("_DissolveThreshold", t);
- For performance, prefer MaterialPropertyBlock when changing properties per-renderer rather than overriding shared material.


Notes about URP/HDRP differences
- HDRP offers more advanced refractive and distortion nodes; if you use HDRP, porting will be easier for heat distortion effects.
- Shader Graph versions should be saved under a folder like `Assets/Shaders/ShaderGraphs/` and Materials created from them.

Would you like that:
- cree un script opcional que anime automáticamente propiedades comunes (Pulse/Dissolve) y lo añada a `PlayerVFXController`?
- cree un pequeño Editor script que lea JSON (o un archivo .asset) y genere los ParticleSystems con los parámetros arriba indicados (esto automatiza la creación de prefabs)?
