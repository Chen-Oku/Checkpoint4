Power-Up VFX - Setup & Usage
=================================

Files added:
- `PowerUp.cs` - component for pickup objects (type, duration, optional prefab)
- `ActivatePowerUp.cs` - attach to the player to detect pickups and trigger VFX controller
- `PlayerVFXController.cs` - attach to the player; holds references to materials and particle prefabs
- `ParticleSpawnController.cs` - helper to spawn particle prefabs at runtime
- `Shaders/SpeedPulse.shader`, `DissolveEdge.shader`, `ShieldFresnel.shader` - example ShaderLab shaders

Quick wiring in Unity Editor
1. Create Materials
   - Right-click in Project -> Create -> Material.
   - For Speed: assign `Shaders/SpeedPulse` shader; set Color, Noise texture, etc.
   - For Strength: assign `Shaders/DissolveEdge` shader; provide a noise texture and tweak threshold/edge width.
   - For Shield: assign `Shaders/ShieldFresnel` shader; tweak fresnel power and color.

2. Player setup
   - Add `PlayerVFXController` to the player GameObject.
   - Assign `defaultMaterial` (player's normal material) and the three materials created above.
   - Assign particle VFX prefabs (see next section).
   - Add `ActivatePowerUp` to the player (same object). Ensure player has a Collider marked as "Is Trigger" if pickups are triggers.

3. Power-Up objects
   - Create a new GameObject for each pickup, add a Collider (Is Trigger) and the `PowerUp` component.
   - Set the `type` to Speed/Strength/Shield. Set `duration` or leave default.
   - Optionally assign a custom VFX prefab to `vfxPrefab`.

4. Particle prefabs (example)
   - Create an empty GameObject and add multiple ParticleSystem children (>=4 different systems recommended: sparks, core glow, trails, burst).
   - Configure each particle system (shapes, lifetime, color). Save the GameObject as a Prefab in Project.
   - Assign that prefab to the `PlayerVFXController` fields (Speed/Strength/Shield VFX Prefab).

Notes & next steps
- The shader files are simple examples; for higher-fidelity results use Shader Graph (URP/HDRP) and custom post-processing.
- You can animate shader properties (dissolve threshold, emission intensity) from `PlayerVFXController` using MaterialPropertyBlock or by adjusting properties on the material instance.
- If you want, I can create example particle-system settings exported as a documented JSON or a Unity package. I can't reliably create binary .prefab files here but I can give step-by-step values for each particle system to reproduce them in the Editor.
