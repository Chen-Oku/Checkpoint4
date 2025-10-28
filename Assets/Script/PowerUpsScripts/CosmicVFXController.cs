using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CosmicVFXController : MonoBehaviour
{
    [Header("Dedicated Cosmic Systems")]
    public ParticleSystem cosmicParticleSystem;
    public ParticleSystem cosmicExplosionParticleSystem;

    [Header("Timing")]
    public float deactivateDelay = 2f;

    [Header("Power-up")]
    // Prefab to spawn on the player when power-up is collected (optional)
    public GameObject cosmicEffectPrefab;
    // Optional material to apply to the player while power-up is active
    public Material cosmicPlayerMaterial;
    // Optional renderer reference for the player (can be assigned in inspector)
    public Renderer playerRenderer;
    // If true, keep the main cosmicParticleSystem visible and playing from the start
    public bool showCosmicAtStart = true;
    // How long to keep the player material before restoring
    public float materialDuration = 5f;
    private Material originalPlayerMaterial;

    void Awake()
    {
        InitDedicatedCosmicSystems();
        // If configured, show the main cosmic particle system from the start
        if (showCosmicAtStart && cosmicParticleSystem != null)
        {
            cosmicParticleSystem.gameObject.SetActive(true);
            cosmicParticleSystem.Play();
        }
    }

    /// <summary>
    /// Call this when the player obtains the power-up.
    /// - plays the explosion particle system
    /// - instantiates the cosmicEffectPrefab on the player (if provided)
    /// - applies cosmicPlayerMaterial to the player's renderer (if provided) for materialDuration seconds
    /// </summary>
    /// <param name="player">Optional player GameObject to parent the spawned VFX under.</param>
    /// <param name="targetRenderer">Optional renderer to apply the material to. If null, uses playerRenderer field.</param>
    public void TriggerPowerUp(GameObject player = null, Renderer targetRenderer = null)
    {
        // Play explosion particle system
        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.gameObject.SetActive(true);
            cosmicExplosionParticleSystem.Play();
            // schedule deactivation
            StartCoroutine(DeactivateAfterDelay(cosmicExplosionParticleSystem.gameObject, deactivateDelay));
        }

        // Spawn the VFX prefab on the player (optional)
        if (cosmicEffectPrefab != null && player != null)
        {
            Instantiate(cosmicEffectPrefab, player.transform.position, player.transform.rotation, player.transform);
        }

        // Apply material to the player renderer temporarily (optional)
        var rend = targetRenderer != null ? targetRenderer : playerRenderer;
        if (rend != null && cosmicPlayerMaterial != null)
        {
            // store original and set new
            originalPlayerMaterial = rend.sharedMaterial;
            rend.material = cosmicPlayerMaterial;
            StartCoroutine(RestoreMaterialAfterDelay(rend, originalPlayerMaterial, materialDuration));
        }
    }

    private IEnumerator RestoreMaterialAfterDelay(Renderer rend, Material original, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        if (rend != null)
        {
            // restore the original shared material to avoid leaking instances
            rend.material = original;
        }
    }

    private void InitDedicatedCosmicSystems()
    {
        if (cosmicParticleSystem != null)
        {
            cosmicParticleSystem.gameObject.SetActive(false);
        }

        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.gameObject.SetActive(false);
        }
    }

    public void Activate()
    {
        if (cosmicParticleSystem != null)
        {
            cosmicParticleSystem.gameObject.SetActive(true);
            cosmicParticleSystem.Play();
        }

        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.gameObject.SetActive(true);
            cosmicExplosionParticleSystem.Play();
        }
    }

    public void Deactivate()
    {
        if (cosmicParticleSystem != null)
        {
            cosmicParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            cosmicParticleSystem.Clear(true);
            StartCoroutine(DeactivateAfterDelay(cosmicParticleSystem.gameObject, deactivateDelay));
        }

        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            cosmicExplosionParticleSystem.Clear(true);
            StartCoroutine(DeactivateAfterDelay(cosmicExplosionParticleSystem.gameObject, deactivateDelay));
        }
    }

    private IEnumerator DeactivateAfterDelay(GameObject go, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        if (go != null)
            go.SetActive(false);
    }
}

// CosmicVFXController: handles cosmic particle systems and trails
