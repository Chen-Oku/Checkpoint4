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
    [Tooltip("Prefab VFX opcional que se instancia en el jugador cuando se recoge el power-up (opcional).")]
    public GameObject cosmicEffectPrefab;
    [Tooltip("Material opcional que se aplica al jugador mientras el power-up está activo.")]
    public Material cosmicPlayerMaterial;
    [Tooltip("Referencia opcional al renderer del jugador (puede asignarse en el Inspector).")]
    public Renderer playerRenderer;
    [Tooltip("Si es true, mantener el particle system principal Cósmico visible y reproduciéndose desde el inicio.")]
    public bool showCosmicAtStart = true;
    [Tooltip("Tiempo (segundos) que mantener el material del jugador antes de restaurarlo.")]
    public float materialDuration = 5f;
    private Material originalPlayerMaterial;

    void Awake()
    {
        InitDedicatedCosmicSystems();
        if (showCosmicAtStart && cosmicParticleSystem != null)
        {
            cosmicParticleSystem.gameObject.SetActive(true);
            cosmicParticleSystem.Play();
        }
    }

    public void TriggerPowerUp(GameObject player = null, Renderer targetRenderer = null)
    {
        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.gameObject.SetActive(true);
            cosmicExplosionParticleSystem.Play();
            // programar desactivación
            StartCoroutine(DeactivateAfterDelay(cosmicExplosionParticleSystem.gameObject, deactivateDelay));
        }

        if (cosmicEffectPrefab != null && player != null)
        {
            Instantiate(cosmicEffectPrefab, player.transform.position, player.transform.rotation, player.transform);
        }

        // Aplicar material temporalmente al renderer del jugador
        var rend = targetRenderer != null ? targetRenderer : playerRenderer;
        if (rend != null && cosmicPlayerMaterial != null)
        {
            // almacenar original y asignar nuevo
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
            // restarurar material original
            rend.material = original;
        }
    }

    private void InitDedicatedCosmicSystems()
    {
        if (cosmicParticleSystem != null)
        {
            if (IsOwnedByThis(cosmicParticleSystem))
                cosmicParticleSystem.gameObject.SetActive(false);
            else
            {
                cosmicParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                cosmicParticleSystem.Clear(true);
            }
        }

        if (cosmicExplosionParticleSystem != null)
        {
            if (IsOwnedByThis(cosmicExplosionParticleSystem))
                cosmicExplosionParticleSystem.gameObject.SetActive(false);
            else
            {
                cosmicExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                cosmicExplosionParticleSystem.Clear(true);
            }
        }
    }

    private bool IsOwnedByThis(ParticleSystem ps)
    {
        if (ps == null) return false;
        var go = ps.gameObject;
        return go == this.gameObject || go.transform.IsChildOf(this.transform);
    }

    public void Activate()
    {
        if (cosmicParticleSystem != null)
        {
            if (IsOwnedByThis(cosmicParticleSystem))
                cosmicParticleSystem.gameObject.SetActive(true);
            cosmicParticleSystem.Play();
        }

        if (cosmicExplosionParticleSystem != null)
        {
            if (IsOwnedByThis(cosmicExplosionParticleSystem))
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
            if (IsOwnedByThis(cosmicParticleSystem))
                StartCoroutine(DeactivateAfterDelay(cosmicParticleSystem.gameObject, deactivateDelay));
        }

        if (cosmicExplosionParticleSystem != null)
        {
            cosmicExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            cosmicExplosionParticleSystem.Clear(true);
            if (IsOwnedByThis(cosmicExplosionParticleSystem))
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
