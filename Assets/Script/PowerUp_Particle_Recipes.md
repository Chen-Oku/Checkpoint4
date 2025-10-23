Power-Up Particle System Recipes
================================

This file describes step-by-step settings for creating 4 ParticleSystems for each Power-Up: Speed, Strength, Shield.
Follow these values in Unity's ParticleSystem inspector (these assume default units). Tweak to taste.

General tips
- Use local space for trails and core attachments where appropriate.
- Use small Start Lifetime for quick effects (0.2-1s) for sparks/trails and longer for mist.
- Use "Play On Awake" = true for continuous VFX; for bursts set Play On Awake = false and trigger Play() from script.
- For trails use Renderer -> Material with an additive particle shader and set Render Mode = Stretched Billboard.


1) Speed / Energía (Electric)
--------------------------------
Goal: sensación de energía moviéndose por el cuerpo. 4 systems: Sparks, SpeedTrails, CoreGlow, DustBurst.

- Sparks (small fast particles)
  - Duration: 0.6
  - Looping: true
  - Start Lifetime: 0.2 - 0.5
  - Start Speed: 2 - 5
  - Start Size: 0.03 - 0.08
  - Start Color: #AEE8FF (cyan-blue) with alpha gradient
  - Emission: Rate over Time = 60
  - Shape: Sphere (Radius = 0.1) or Cone (Angle = 20, Radius = 0.05)
  - Velocity over Lifetime: Radial-ish using X/Y limit or use randomize direction (0.6)
  - Color over Lifetime: Gradient from bright cyan to transparent
  - Noise: Strength = 0.1, Frequency = 0.5
  - Renderer: Render Mode: Billboard, Material: additive particle (tinted)

- SpeedTrails (stretched trail particles that follow movement)
  - Duration: 0.6
  - Looping: true
  - Start Lifetime: 0.15 - 0.35
  - Start Speed: 0 (use attached emission via script or set Shape to Mesh/SkinnedMeshRenderer with emissive points)
  - Start Size: 0.02 - 0.05 (width)
  - Emission: Rate over Time = 0; Use Emission bursts on Move events or set Rate = 40
  - Shape: Box thin around limbs or Skinned Mesh shape to spawn from bones
  - Renderer: Render Mode: Stretched Billboard; Speed Scale = 2.0; Length Scale = 3.0
  - Trails Module: enabled, set Ratio = 1, Lifetime = 0.3; Material: additive, thin soft texture

- CoreGlow (soft sphere near body)
  - Duration: 1.5
  - Looping: true
  - Start Lifetime: 0.8 - 1.2
  - Start Speed: 0
  - Start Size: 0.6 - 1.2 (scale with player size)
  - Start Color: Bright cyan with alpha 0.6
  - Emission: Rate over Time = 8
  - Shape: Sphere (Radius = 0.1) but use Renderer -> Render Mode: Billboard and set Soft Particle Factor if available
  - Color over Lifetime: subtle pulsing via gradient alpha (use Gradient with sin-like curve)

- DustBurst (initial burst on pickup)
  - Duration: 0.8
  - Looping: false
  - Start Lifetime: 0.6 - 1.0
  - Start Speed: 1 - 2
  - Start Size: 0.06 - 0.25
  - Emission: Bursts: time 0 count 40
  - Shape: Cone (Angle = 85, Radius = 0.2)
  - Color over Lifetime: light gray -> transparent
  - Renderer: use opaque/multiply to mix with ground


2) Strength / Furia (Fire Aura)
--------------------------------
Goal: personaje envuelto en fuego con borde brillante y humo. 4 systems: FlameWrap, SmokeLift, Sparks, Shockwave.

- FlameWrap (continuous ember/fires around body)
  - Duration: 2.0
  - Looping: true
  - Start Lifetime: 0.6 - 1.2
  - Start Speed: 0.2 - 0.8
  - Start Size: 0.2 - 0.6
  - Start Color: gradient orange->yellow
  - Emission: Rate over Time = 40
  - Shape: Mesh or Sphere with Radius = 0.5
  - Size over Lifetime: curve that grows slightly then fades
  - Color over Lifetime: orange -> transparent
  - Renderer: additive or alpha blended; light scattering material

- SmokeLift (ascending smoke)
  - Duration: 4.0
  - Looping: true
  - Start Lifetime: 1.8 - 3.0
  - Start Speed: 0.2 - 0.6
  - Start Size: 0.4 - 1.2
  - Start Color: dark gray with alpha 0.6
  - Emission: Rate over Time = 12
  - Shape: Cone (Angle = 25), Emit from bottom of flame area
  - Velocity over Lifetime: upward (Y = 0.6)
  - Noise: strength 0.4 for turbulent look

- Sparks (bright short sparks)
  - Duration: 0.8
  - Looping: true
  - Start Lifetime: 0.2 - 0.5
  - Start Speed: 3 - 6
  - Start Size: 0.03 - 0.08
  - Start Color: bright yellow/white
  - Emission: Rate over Time = 80
  - Shape: Cone small angle, random direction
  - Renderer: additive, small soft texture

- Shockwave (one-shot ring on activate)
  - Duration: 0.8
  - Looping: false
  - Start Lifetime: 0.6
  - Start Speed: 0.0 (expand via size over lifetime)
  - Start Size: 0.1
  - Emission: Burst time 0 count 1
  - Shape: Circle (use single particle and scale size over lifetime)
  - Size over Lifetime: curve from 0.1 to 2.0 quickly
  - Color over Lifetime: bright edge then transparent


3) Shield / Protección (Energy Bubble)
--------------------------------
Goal: burbuja protectora con fresnel y partículas flotando. 4 systems: ShieldCore, OrbitParticles, RingBurst, EnergyMist.

- ShieldCore (soft glow inside shield)
  - Duration: 3.5
  - Looping: true
  - Start Lifetime: 1.2 - 2.0
  - Start Speed: 0
  - Start Size: 0.8 - 1.6 (fit to player)
  - Start Color: cyan/blue (slightly desaturated)
  - Emission: Rate over Time = 6
  - Shape: Sphere, emit inside
  - Color over Lifetime: subtle alpha pulsation

- OrbitParticles (small points orbiting)
  - Duration: 3.5
  - Looping: true
  - Start Lifetime: 1.0 - 2.5
  - Start Speed: 0.2 - 0.5
  - Start Size: 0.02 - 0.06
  - Emission: Rate over Time = 20
  - Shape: Donut/Torus (use Mesh or Shape: Sphere with Radius and set Velocity over Lifetime to orbit)
  - Velocity over Lifetime: set orbital-like velocities via X/Y curves
  - Renderer: soft additive

- RingBurst (activation ring)
  - Duration: 1.0
  - Looping: false
  - Start Lifetime: 0.6
  - Start Speed: 0
  - Start Size: 0.05
  - Emission: Burst time 0 count 1
  - Shape: Circle emit outward
  - Size over Lifetime: expand to 1.8
  - Color over Lifetime: cyan -> transparent

- EnergyMist (continuous subtle haze)
  - Duration: 6.0
  - Looping: true
  - Start Lifetime: 2.0 - 4.0
  - Start Speed: 0.05 - 0.2
  - Start Size: 0.6 - 1.6
  - Emission: Rate over Time = 6
  - Shape: Sphere or Hemisphere
  - Color over Lifetime: light cyan fading to transparent
  - Noise: small strength 0.1


How to assemble a prefab
------------------------
1. Create an empty GameObject named e.g. "VFX_Speed_Prefab".
2. Add 4 child GameObjects named as the systems above and add a ParticleSystem component to each.
3. Configure each ParticleSystem with the values above.
4. In the parent, add a small anchor script (optional) to allow triggering bursts from `PlayerVFXController`.
5. Save the parent as a Prefab in Project and assign to `PlayerVFXController`.

If you want, puedo crear un archivo con valores JSON para cada system para que puedas parsearlo con un pequeño editor script en Unity que cree y configure estos ParticleSystems automáticamente. ¿Quieres que lo haga?
