using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages a single active power-up on the player.
// Attach to the Player root. It swaps materials (shader) on the player's renderers
// and instantiates any player VFX. Only one power-up can be active at a time.
public class PlayerVFXControllerPOW : MonoBehaviour
{
    // Current active power-up data
    private PowerUpData activePowerUp;
    private Coroutine activeRoutine;

    // Keep original materials per renderer to restore later
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    // The instantiated VFX parented to the player when a power-up is active
    private GameObject activePlayerVFXInstance;

    /// <summary>
    /// Activate a power-up on this player. Replaces any active power-up.
    /// </summary>
    public void ActivatePowerUp(PowerUpData data)
    {
        if (data == null) return;

        // If there's an active power-up, remove it first
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            DeactivateCurrentPowerUp();
        }

        activePowerUp = data;

        ApplyMaterialToRenderers(data.effectMaterial);

        if (data.playerVFXPrefab != null)
        {
            activePlayerVFXInstance = Instantiate(data.playerVFXPrefab, transform);
            // If there's a ParticleSystem on the prefab, ensure it plays
            var ps = activePlayerVFXInstance.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        // Start duration countdown
        activeRoutine = StartCoroutine(PowerUpTimer(data.duration));
    }

    private IEnumerator PowerUpTimer(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        DeactivateCurrentPowerUp();
        activeRoutine = null;
    }

    private void DeactivateCurrentPowerUp()
    {
        // Destroy player VFX
        if (activePlayerVFXInstance != null)
        {
            var ps = activePlayerVFXInstance.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }
            Destroy(activePlayerVFXInstance);
            activePlayerVFXInstance = null;
        }

        // Restore original materials
        RestoreOriginalMaterials();

        activePowerUp = null;
    }

    private void ApplyMaterialToRenderers(Material effectMaterial)
    {
        // If no material provided, nothing to apply
        if (effectMaterial == null) return;

        var renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        originalMaterials.Clear();

        foreach (var r in renderers)
        {
            // store a copy of original materials
            var orig = r.materials;
            originalMaterials[r] = orig;

            // create a new array with the effect material applied to all slots
            var newMats = new Material[orig.Length];
            for (int i = 0; i < newMats.Length; i++)
            {
                newMats[i] = effectMaterial;
            }
            r.materials = newMats;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kv in originalMaterials)
        {
            if (kv.Key != null)
            {
                kv.Key.materials = kv.Value;
            }
        }
        originalMaterials.Clear();
    }

    // Optional: expose a method to forcibly clear any active power-up (e.g., on death)
    public void ClearPowerUp()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        DeactivateCurrentPowerUp();
    }
}

