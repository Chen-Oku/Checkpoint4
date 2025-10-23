using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMaterialController), typeof(SpeedVFXController))]
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
    private Material legacyStrengthMaterial;
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

    // Optional helper components created/used during Awake
    private PlayerMaterialController materialController;
    private SpeedVFXController speedVFXController;

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
        if (legacyStrengthMaterial != null)
            materialController.strengthMaterial = legacyStrengthMaterial;
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
                if (overrideVFX != null) SpawnVFX(overrideVFX);
                StartCoroutine(TimeoutRestore(duration));
                // Do not destroy immediately; let TimeoutRestore handle cleanup so the particle
                // system plays for the full duration of the power-up.
                break;
            case PowerUpType.Strength:
                if (materialController != null) materialController.ApplyMaterial(materialController.strengthMaterial);
                if (overrideVFX != null) SpawnVFX(overrideVFX);
                StartCoroutine(TimeoutRestore(duration));
                break;
            case PowerUpType.Shield:
                if (materialController != null) materialController.ApplyMaterial(materialController.shieldMaterial);
                if (overrideVFX != null) SpawnVFX(overrideVFX);
                StartCoroutine(TimeoutRestore(duration));
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
        activeVFXInstance = Instantiate(prefab, transform);
        // ensure particle systems play
        var systems = activeVFXInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            ps.Play();
        }

        // Also trigger the dedicated speed systems (use authoritative controller)
        if (speedVFXController != null)
            speedVFXController.Activate();
        else if (legacySpeedParticleSystem != null)
            legacySpeedParticleSystem.Play();
        // No optional VFX material overrides; particle systems use their prefab materials
    }

    private IEnumerator TimeoutRestore(float seconds)
    {
        if (seconds > 0)
            yield return new WaitForSeconds(seconds);

        // restore default
        if (materialController != null)
            materialController.RestoreDefault();
        else if (legacyDefaultMaterial != null)
            ApplyMaterial(legacyDefaultMaterial);

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
            if (speedVFXController != null) speedVFXController.Deactivate();
            else
            {
                // Fallback: stop legacy systems if present
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
