using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFXController : MonoBehaviour
{
    [Header("Materials")]
    public Material defaultMaterial;
    public Material speedMaterial;
    public Material strengthMaterial;
    public Material shieldMaterial;

    [Header("Particle VFX Prefabs")]
    public GameObject speedVFXPrefab;    // should contain >=4 particle systems
    public GameObject strengthVFXPrefab; // should contain >=4 particle systems
    public GameObject shieldVFXPrefab;   // should contain >=4 particle systems
    
    [Header("Optional VFX Materials (applied only to spawned VFX particle renderers)")]
    public Material speedVFXMaterial;    // if set, overrides materials in spawned speed VFX
    public Material strengthVFXMaterial; // if set, overrides materials in spawned strength VFX
    public Material shieldVFXMaterial;   // if set, overrides materials in spawned shield VFX

    // active instance references
    private GameObject activeVFXInstance;
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    void Awake()
    {
        // Cache renderers but exclude particle system renderers so VFX are not affected
        var allRenderers = GetComponentsInChildren<Renderer>();
        var filtered = new List<Renderer>(allRenderers.Length);
        foreach (var r in allRenderers)
        {
            // ParticleSystemRenderer is a Renderer subclass; we do not want to change particle materials here
            if (r is ParticleSystemRenderer)
                continue;
            filtered.Add(r);
        }
        renderers = filtered.ToArray();
        mpb = new MaterialPropertyBlock();
        // cache default material if not set
        if (defaultMaterial == null && renderers.Length > 0)
        {
            // Prefer mesh/skinned renderers for the player's default material
            Material found = null;
            foreach (var r in renderers)
            {
                if (r is MeshRenderer || r is SkinnedMeshRenderer)
                {
                    found = r.sharedMaterial;
                    break;
                }
            }
            defaultMaterial = found ?? renderers[0].sharedMaterial;
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
                ApplyMaterial(speedMaterial);
                SpawnVFX(overrideVFX ?? speedVFXPrefab);
                StartCoroutine(TimeoutRestore(duration));
                break;
            case PowerUpType.Strength:
                ApplyMaterial(strengthMaterial);
                SpawnVFX(overrideVFX ?? strengthVFXPrefab);
                StartCoroutine(TimeoutRestore(duration));
                break;
            case PowerUpType.Shield:
                ApplyMaterial(shieldMaterial);
                SpawnVFX(overrideVFX ?? shieldVFXPrefab);
                StartCoroutine(TimeoutRestore(duration));
                break;
            default:
                break;
        }
    }

    private void ApplyMaterial(Material mat)
    {
        if (mat == null) return;
        foreach (var r in renderers)
        {
            // set shared material so outline/dissolve works; for per-instance properties use mpb
            r.material = mat;
        }
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
        // Optionally override particle renderers' material for this spawned VFX (if a VFX material was provided)
        // Determine which VFX material to use based on the prefab reference
        Material overrideVfxMat = null;
        if (prefab == speedVFXPrefab) overrideVfxMat = speedVFXMaterial;
        else if (prefab == strengthVFXPrefab) overrideVfxMat = strengthVFXMaterial;
        else if (prefab == shieldVFXPrefab) overrideVfxMat = shieldVFXMaterial;

        if (overrideVfxMat != null)
        {
            var psRenderers = activeVFXInstance.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var psr in psRenderers)
            {
                // set sharedMaterial so the particle renderer uses the desired appearance
                psr.sharedMaterial = overrideVfxMat;
            }
        }
    }

    private IEnumerator TimeoutRestore(float seconds)
    {
        if (seconds > 0)
            yield return new WaitForSeconds(seconds);

        // restore default
        if (defaultMaterial != null)
            ApplyMaterial(defaultMaterial);

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
    }
}
