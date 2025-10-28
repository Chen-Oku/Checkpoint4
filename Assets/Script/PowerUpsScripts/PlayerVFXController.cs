using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMaterialController), typeof(SpeedVFXController), typeof(ShieldVFXController))]
public class PlayerVFXController : MonoBehaviour
{
    [Header("Materials")]
    // Legacy serialized fields: kept so existing scenes/prefabs don't lose references.
    // Values are migrated to PlayerMaterialController in Awake and these fields are hidden from the Inspector.
    [SerializeField, HideInInspector]
    private Material legacyDefaultMaterial;
    [SerializeField, HideInInspector]
    private Material legacySpeedMaterial;
    [SerializeField, HideInInspector]
    private Material legacyCosmicMaterial;
    [SerializeField, HideInInspector]
    private Material legacyShieldMaterial;

    // Particle VFX prefabs removed — optional override GameObjects can still be
    // passed to ActivatePowerUp via the overrideVFX parameter.

    [Header("Dedicated Speed Systems")]
    [Tooltip("Looping particle system used while speed power-up is active (e.g. trails).")]
    [SerializeField, HideInInspector]
    private ParticleSystem legacySpeedParticleSystem;
    [Tooltip("One-shot explosion particle system played when speed power-up is acquired.")]
    [SerializeField, HideInInspector]
    private ParticleSystem legacySpeedExplosionParticleSystem;
    [SerializeField, HideInInspector]
    private ParticleSystem legacyCosmicParticleSystem;
    [SerializeField, HideInInspector]
    private ParticleSystem legacyCosmicExplosionParticleSystem;

    // Optional helper components created/used during Awake
    private PlayerMaterialController materialController;
    private SpeedVFXController speedVFXController;
    private CosmicVFXController cosmicVFXController;
    private ShieldVFXController shieldVFXController;

    [Header("Default VFX Prefabs")]
    [Tooltip("Optional default VFX prefab spawned on the player when a Speed power-up is picked up (used if the pickup doesn't provide an override).")]
    [SerializeField]
    private GameObject defaultSpeedVFXPrefab;
    [Tooltip("Optional default VFX prefab spawned on the player when a Cosmic power-up is picked up (used if the pickup doesn't provide an override).")]
    [SerializeField]
    private GameObject defaultCosmicVFXPrefab;
    [Tooltip("Optional default VFX prefab spawned on the player when a Shield power-up is picked up (used if the pickup doesn't provide an override).")]
    [SerializeField]
    private GameObject defaultShieldVFXPrefab;

    [Header("Timing")]
    [Tooltip("Delay before deactivating speed particle GameObjects to allow trails/particles to fade.")]
    [SerializeField, HideInInspector]
    private float legacyDeactivateDelay = 2f;
    [Tooltip("Temporary trail time used to quickly hide trails when clearing.")]
    [SerializeField, HideInInspector]
    private float legacyTrailHideTime = 0.01f;
    

    // active instance references
    private GameObject activeVFXInstance;
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    void Awake()
    {
        // Ensure authoritative components exist (RequireComponent makes them present in the Editor, but be defensive)
        materialController = GetComponent<PlayerMaterialController>();
        speedVFXController = GetComponent<SpeedVFXController>();

        if (materialController == null)
            materialController = gameObject.AddComponent<PlayerMaterialController>();
        if (speedVFXController == null)
            speedVFXController = gameObject.AddComponent<SpeedVFXController>();

        // Migration: if legacy serialized values exist on this instance, copy them into the authoritative components.
        // This preserves references from scenes/prefabs when we remove public fields later.
        if (legacyDefaultMaterial != null)
            materialController.defaultMaterial = legacyDefaultMaterial;
        if (legacySpeedMaterial != null)
            materialController.speedMaterial = legacySpeedMaterial;
        if (legacyCosmicMaterial != null)
            materialController.cosmicMaterial = legacyCosmicMaterial;
        if (legacyShieldMaterial != null)
            materialController.shieldMaterial = legacyShieldMaterial;

        if (legacySpeedParticleSystem != null)
            speedVFXController.speedParticleSystem = legacySpeedParticleSystem;
        if (legacySpeedExplosionParticleSystem != null)
            speedVFXController.speedExplosionParticleSystem = legacySpeedExplosionParticleSystem;
        // migrate timing values if they differ from defaults
        if (legacyDeactivateDelay > 0f)
            speedVFXController.deactivateDelay = legacyDeactivateDelay;
        if (legacyTrailHideTime > 0f)
            speedVFXController.trailHideTime = legacyTrailHideTime;

        // Ensure and migrate legacy cosmic systems into the authoritative cosmic controller
        cosmicVFXController = GetComponent<CosmicVFXController>();
        if (cosmicVFXController == null)
            cosmicVFXController = gameObject.AddComponent<CosmicVFXController>();
        if (legacyCosmicParticleSystem != null)
            cosmicVFXController.cosmicParticleSystem = legacyCosmicParticleSystem;
        if (legacyCosmicExplosionParticleSystem != null)
            cosmicVFXController.cosmicExplosionParticleSystem = legacyCosmicExplosionParticleSystem;
        if (legacyDeactivateDelay > 0f)
            cosmicVFXController.deactivateDelay = legacyDeactivateDelay;
        // if (legacyTrailHideTime > 0f)
        //     cosmicVFXController.trailHideTime = legacyTrailHideTime;

        // Cache renderers for fallback ApplyMaterial path and for SpawnVFX behavior
        var allRenderers = GetComponentsInChildren<Renderer>();
        var filtered = new List<Renderer>(allRenderers.Length);
        foreach (var r in allRenderers)
        {
            if (r is ParticleSystemRenderer)
                continue;
            filtered.Add(r);
        }
        renderers = filtered.ToArray();
        mpb = new MaterialPropertyBlock();

        // Ensure Shield controller exists (added defensively to match other controllers)
        shieldVFXController = GetComponent<ShieldVFXController>();
        if (shieldVFXController == null)
            shieldVFXController = gameObject.AddComponent<ShieldVFXController>();

        // Diagnostic: warn if no cosmic VFX sources are present so it's easier to debug
        bool hasCosmicSources = (defaultCosmicVFXPrefab != null) || (legacyCosmicParticleSystem != null) || (legacyCosmicExplosionParticleSystem != null) || (cosmicVFXController != null && (cosmicVFXController.cosmicParticleSystem != null || cosmicVFXController.cosmicExplosionParticleSystem != null || cosmicVFXController.cosmicEffectPrefab != null));
        if (!hasCosmicSources)
        {
            Debug.LogWarning("PlayerVFXController: No cosmic VFX prefab or particle systems assigned for Cosmic power-up. Assign either PowerUp.vfxPrefab on pickups or defaultCosmicVFXPrefab / CosmicVFXController particle systems on the player.", this);
        }
    }

    public void ActivatePowerUp(PowerUpType type, float duration = 5f, GameObject overrideVFX = null)
    {
        StopAllCoroutines();
        // clean previous VFX
        if (activeVFXInstance != null) Destroy(activeVFXInstance);

        switch (type)
        {
            case PowerUpType.Speed:
                // Apply material via controller (use authoritative component values)
                if (materialController != null) materialController.ApplyMaterial(materialController.speedMaterial, (speedVFXController != null) ? speedVFXController.speedParticleSystem : legacySpeedParticleSystem);
                if (speedVFXController != null) speedVFXController.Activate();
                // spawn override if provided, otherwise use default prefab for Speed
                var speedPrefabToSpawn = overrideVFX != null ? overrideVFX : defaultSpeedVFXPrefab;
                if (speedPrefabToSpawn != null) SpawnVFX(speedPrefabToSpawn);
                StartCoroutine(TimeoutRestore(duration, PowerUpType.Speed));
                // Do not destroy immediately; let TimeoutRestore handle cleanup so the particle
                // system plays for the full duration of the power-up.
                break;
            case PowerUpType.Cosmic:
                if (materialController != null) materialController.ApplyMaterial(materialController.cosmicMaterial);
                if (cosmicVFXController != null)
                {
                    cosmicVFXController.Activate();
                    Debug.Log("PlayerVFXController: Triggered CosmicVFXController.Activate()", this);
                }
                else
                {
                    Debug.LogWarning("PlayerVFXController: CosmicVFXController missing on player. Dedicated cosmic particle systems won't play.", this);
                }

                // spawn override if provided, otherwise use default prefab for Cosmic
                var cosmicPrefabToSpawn = overrideVFX != null ? overrideVFX : defaultCosmicVFXPrefab;
                if (cosmicPrefabToSpawn != null)
                {
                    Debug.Log("PlayerVFXController: Spawning Cosmic VFX prefab: " + cosmicPrefabToSpawn.name, this);
                    SpawnVFX(cosmicPrefabToSpawn);
                }
                else
                {
                    Debug.Log("PlayerVFXController: No Cosmic prefab to spawn (override VFX null and defaultCosmicVFXPrefab not assigned).", this);
                }
                StartCoroutine(TimeoutRestore(duration, PowerUpType.Cosmic));
                break;
            case PowerUpType.Shield:
                if (materialController != null)
                {
                    materialController.ApplyMaterial(materialController.shieldMaterial);
                    Debug.Log("PlayerVFXController: Applied shield material.", this);
                }
                // Activate dedicated shield VFX systems if present
                if (shieldVFXController != null)
                {
                    shieldVFXController.Activate();
                }
                else
                {
                    Debug.LogWarning("PlayerVFXController: ShieldVFXController missing on player. Shield particle systems won't play.", this);
                }
                // spawn override if provided, otherwise use default prefab for Shield
                var shieldPrefabToSpawn = overrideVFX != null ? overrideVFX : defaultShieldVFXPrefab;
                if (shieldPrefabToSpawn != null) SpawnVFX(shieldPrefabToSpawn);
                StartCoroutine(TimeoutRestore(duration, PowerUpType.Shield));
                break;
            default:
                break;
        }
    }

    private void ApplyMaterial(Material mat)
    {
        // This method is kept for backward compatibility; prefer using PlayerMaterialController.
        if (mat == null) return;
        foreach (var r in renderers)
        {
            // Use speedParticleSystem from the authoritative controller if available, else fall back to legacy field
            var sp = (speedVFXController != null) ? speedVFXController.speedParticleSystem : legacySpeedParticleSystem;
            if (sp != null && IsChildOf(r.transform, sp.gameObject.transform))
                continue;
            r.material = mat;
        }
    }

    // Helper to test if a transform is the same as or child of a parent transform
    private bool IsChildOf(Transform t, Transform parent)
    {
        if (t == null || parent == null) return false;
        return t == parent || t.IsChildOf(parent);
    }

    private void SpawnVFX(GameObject prefab)
    {
        if (prefab == null) return;
        // Instantiate at the player's world position/rotation and parent to keep it aligned.
        activeVFXInstance = Instantiate(prefab, transform.position, transform.rotation, transform);
        // Ensure local transform is reset so prefab appears at the expected point on the player
        activeVFXInstance.transform.localPosition = Vector3.zero;
        activeVFXInstance.transform.localRotation = Quaternion.identity;
        // ensure particle systems play
        var systems = activeVFXInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            ps.Play();
        }
        // NOTE: Do NOT trigger dedicated controllers here. Activation of the
        // speed/cosmic controllers should be performed by the caller
        // (ActivatePowerUp) so that spawning a VFX prefab for one power-up
        // doesn't unintentionally enable another power-up's systems.
        // No optional VFX material overrides; particle systems use their prefab materials
    }

    private IEnumerator TimeoutRestore(float seconds, PowerUpType type)
    {
        if (seconds > 0)
            yield return new WaitForSeconds(seconds);

        // restore default
        if (materialController != null)
            materialController.RestoreDefault();
        else if (legacyDefaultMaterial != null)
            ApplyMaterial(legacyDefaultMaterial);

        // Diagnostic: log material restore
        Debug.Log("PlayerVFXController: Restored default material after power-up timeout: " + type, this);

        if (activeVFXInstance != null)
        {
            var systems = activeVFXInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in systems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            Destroy(activeVFXInstance, 2f);
            activeVFXInstance = null;
        }
        // Deactivate only the controller corresponding to the expired power-up
        switch (type)
        {
            case PowerUpType.Speed:
                if (speedVFXController != null)
                    speedVFXController.Deactivate();
                else
                {
                    if (legacySpeedParticleSystem != null)
                    {
                        legacySpeedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        legacySpeedParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(legacySpeedParticleSystem.gameObject, legacyDeactivateDelay));
                    }
                    if (legacySpeedExplosionParticleSystem != null)
                    {
                        legacySpeedExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        legacySpeedExplosionParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(legacySpeedExplosionParticleSystem.gameObject, legacyDeactivateDelay));
                    }
                }
                break;
            case PowerUpType.Cosmic:
                if (cosmicVFXController != null)
                    cosmicVFXController.Deactivate();
                else
                {
                    if (legacyCosmicParticleSystem != null)
                    {
                        legacyCosmicParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        legacyCosmicParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(legacyCosmicParticleSystem.gameObject, legacyDeactivateDelay));
                    }
                    if (legacyCosmicExplosionParticleSystem != null)
                    {
                        legacyCosmicExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        legacyCosmicExplosionParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(legacyCosmicExplosionParticleSystem.gameObject, legacyDeactivateDelay));
                    }
                }
                break;
            case PowerUpType.Shield:
                // Ensure material is restored before deactivating shield VFX
                if (materialController != null)
                {
                    materialController.RestoreDefault();
                }
                if (shieldVFXController != null)
                    shieldVFXController.Deactivate();
                break;
            default:
                // Shield handled above; other types no-op
                break;
        }
    }
    // Helper: deactivate a GameObject after a delay (used to let trails finish)
    private IEnumerator DeactivateAfterDelay(GameObject go, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        if (go != null)
            go.SetActive(false);
    }
}
