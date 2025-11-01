using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMaterialController), typeof(SpeedVFXController), typeof(ShieldVFXController))]
public class PlayerVFXController : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material speedMaterial;
    [SerializeField]
    private Material cosmicMaterial;
    [SerializeField]
    private Material shieldMaterial;


    [Header("Dedicated Speed Systems")]
    [Tooltip("Sistema de partículas en bucle usado mientras el power-up de Velocidad está activo (p.ej. estelas).")]
    [SerializeField]
    private ParticleSystem speedParticleSystem;
    [Tooltip("Sistema de partículas de explosión de una sola vez reproducido cuando se obtiene el power-up de Velocidad.")]
    [SerializeField]
    private ParticleSystem speedExplosionParticleSystem;
    [SerializeField]
    private ParticleSystem cosmicParticleSystem;
    [Tooltip("Sistema de partículas de explosión de una sola vez reproducido cuando se obtiene el power-up Cósmico.")]
    [SerializeField]
    private ParticleSystem cosmicExplosionParticleSystem;

    // Optional helper creado/usado durante Awake
    private PlayerMaterialController materialController;
    private SpeedVFXController speedVFXController;
    private CosmicVFXController cosmicVFXController;
    private ShieldVFXController shieldVFXController;

    [Header("Default VFX Prefabs")]
    [Tooltip("Prefab VFX predeterminado opcional que se instancia en el jugador cuando se recoge un power-up de Velocidad (usado si el pickup no provee override).")]
    [SerializeField]
    private GameObject defaultSpeedVFXPrefab;
    [Tooltip("Prefab VFX predeterminado opcional que se instancia en el jugador cuando se recoge un power-up Cósmico (usado si el pickup no provee override).")]
    [SerializeField]
    private GameObject defaultCosmicVFXPrefab;
    [Tooltip("Prefab VFX predeterminado opcional que se instancia en el jugador cuando se recoge un power-up de Escudo (usado si el pickup no provee override).")]
    [SerializeField]
    private GameObject defaultShieldVFXPrefab;

    [Header("Timing")]
    [Tooltip("Retraso antes de desactivar GameObjects de partículas de Velocidad para permitir que las estelas/partículas se desvanezcan.")]
    [SerializeField]
    private float deactivateDelay = 2f;
    [Tooltip("Tiempo temporal de las estelas usado para ocultarlas rápidamente al limpiar.")]
    [SerializeField]
    private float trailHideTime = 0.01f;
    

    private GameObject activeVFXInstance;
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    void Awake()
    {
    // Asegurar que los componentes autorizados existan y obtener referencias
        materialController = GetComponent<PlayerMaterialController>();
        speedVFXController = GetComponent<SpeedVFXController>();

        if (materialController == null)
            materialController = gameObject.AddComponent<PlayerMaterialController>();
        if (speedVFXController == null)
            speedVFXController = gameObject.AddComponent<SpeedVFXController>();

    // Migración: si existen valores serializados en esta instancia, copiarlos a los componentes autorizados.
    // Esto preserva referencias desde escenas/prefabs.
        if (defaultMaterial != null)
            materialController.defaultMaterial = defaultMaterial;
        if (speedMaterial != null)
            materialController.speedMaterial = speedMaterial;
        if (cosmicMaterial != null)
            materialController.cosmicMaterial = cosmicMaterial;
        if (shieldMaterial != null)
            materialController.shieldMaterial = shieldMaterial;

        if (speedParticleSystem != null)
            speedVFXController.speedParticleSystem = speedParticleSystem;
        if (speedExplosionParticleSystem != null)
            speedVFXController.speedExplosionParticleSystem = speedExplosionParticleSystem;
        // migrar valores de timing si se han personalizado
        if (deactivateDelay > 0f)
            speedVFXController.deactivateDelay = deactivateDelay;
        if (trailHideTime > 0f)
            speedVFXController.trailHideTime = trailHideTime;

    // asegurar que el controlador Cosmic exista y migrar valores
        cosmicVFXController = GetComponent<CosmicVFXController>();
        if (cosmicVFXController == null)
            cosmicVFXController = gameObject.AddComponent<CosmicVFXController>();
        if (cosmicParticleSystem != null)
            cosmicVFXController.cosmicParticleSystem = cosmicParticleSystem;
        if (cosmicExplosionParticleSystem != null)
            cosmicVFXController.cosmicExplosionParticleSystem = cosmicExplosionParticleSystem;
        if (deactivateDelay > 0f)
            cosmicVFXController.deactivateDelay = deactivateDelay;

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

    // Asegurar que el controlador de Shield exista 
        shieldVFXController = GetComponent<ShieldVFXController>();
        if (shieldVFXController == null)
            shieldVFXController = gameObject.AddComponent<ShieldVFXController>();

    // Diagnóstico: avisar si no hay fuentes de VFX Cósmico asignadas para facilitar depuración
    bool hasCosmicSources = (defaultCosmicVFXPrefab != null) || (cosmicParticleSystem != null) || (cosmicExplosionParticleSystem != null) || (cosmicVFXController != null && (cosmicVFXController.cosmicParticleSystem != null || cosmicVFXController.cosmicExplosionParticleSystem != null || cosmicVFXController.cosmicEffectPrefab != null));
        if (!hasCosmicSources)
        {
            Debug.LogWarning("PlayerVFXController: No cosmic VFX prefab or particle systems assigned for Cosmic power-up. Assign either PowerUp.vfxPrefab on pickups or defaultCosmicVFXPrefab / CosmicVFXController particle systems on the player.", this);
        }
    }

    public void ActivatePowerUp(PowerUpType type, float duration = 5f, GameObject overrideVFX = null)
    {
        StopAllCoroutines();
    // limpiar VFX previo
        if (activeVFXInstance != null) Destroy(activeVFXInstance);

    // asegurar que solo el power-up activado tenga VFX activos
        DeactivateAllControllers(type);

        switch (type)
        {
            case PowerUpType.Speed:
                    // Aplicar material via controller 
                if (materialController != null) materialController.ApplyMaterial(materialController.speedMaterial, (speedVFXController != null) ? speedVFXController.speedParticleSystem : speedParticleSystem);
                if (speedVFXController != null) speedVFXController.Activate();
                // instanciar override si se proporciona, de lo contrario usar el prefab predeterminado para Velocidad
                var speedPrefabToSpawn = overrideVFX != null ? overrideVFX : defaultSpeedVFXPrefab;
                if (speedPrefabToSpawn != null) SpawnVFX(speedPrefabToSpawn);
                StartCoroutine(TimeoutRestore(duration, PowerUpType.Speed));
                // No destruir inmediatamente; TimeoutRestore manejará la limpieza para que el sistema
                // de partículas se reproduzca durante toda la duración del power-up.
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

                // instanciar override si se proporciona, de lo contrario usar el prefab predeterminado para Cósmico
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
                // instanciar override si se proporciona, de lo contrario usar el prefab predeterminado para Escudo
                var shieldPrefabToSpawn = overrideVFX != null ? overrideVFX : defaultShieldVFXPrefab;
                if (shieldPrefabToSpawn != null) SpawnVFX(shieldPrefabToSpawn);
                StartCoroutine(TimeoutRestore(duration, PowerUpType.Shield));
                break;
            default:
                break;
        }
    }

    private void DeactivateAllControllers(PowerUpType? exclude = null)
    {
    // Velocidad
        if (exclude != PowerUpType.Speed)
        {
            if (speedVFXController != null)
                speedVFXController.Deactivate();
            else
            {
                if (speedParticleSystem != null)
                {
                    speedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    speedParticleSystem.Clear(true);
                    StartCoroutine(DeactivateAfterDelay(speedParticleSystem.gameObject, deactivateDelay));
                }
                if (speedExplosionParticleSystem != null)
                {
                    speedExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    speedExplosionParticleSystem.Clear(true);
                    StartCoroutine(DeactivateAfterDelay(speedExplosionParticleSystem.gameObject, deactivateDelay));
                }
            }
        }

    // Cósmico
        if (exclude != PowerUpType.Cosmic)
        {
            if (cosmicVFXController != null)
                cosmicVFXController.Deactivate();
            else
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
        }

    // Escudo
        if (exclude != PowerUpType.Shield)
        {
            if (shieldVFXController != null)
                shieldVFXController.Deactivate();
        }
    }

    private void ApplyMaterial(Material mat)
    {
        if (mat == null) return;
        foreach (var r in renderers)
        {
            var sp = (speedVFXController != null) ? speedVFXController.speedParticleSystem : speedParticleSystem;
            if (sp != null && IsChildOf(r.transform, sp.gameObject.transform))
                continue;
            r.material = mat;
        }
    }

    private bool IsChildOf(Transform t, Transform parent)
    {
        if (t == null || parent == null) return false;
        return t == parent || t.IsChildOf(parent);
    }

    private void SpawnVFX(GameObject prefab)
    {
        if (prefab == null) return;
        // Instanciar el prefab VFX como hijo del jugador
        activeVFXInstance = Instantiate(prefab, transform.position, transform.rotation, transform);
        activeVFXInstance.transform.localPosition = Vector3.zero;
        activeVFXInstance.transform.localRotation = Quaternion.identity;
        // asegurar que los sistemas de partículas se reproduzcan
        var systems = activeVFXInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            ps.Play();
        }
    }

    private IEnumerator TimeoutRestore(float seconds, PowerUpType type)
    {
        if (seconds > 0)
            yield return new WaitForSeconds(seconds);

        // restore default
        if (materialController != null)
            materialController.RestoreDefault();
        else if (defaultMaterial != null)
            ApplyMaterial(defaultMaterial);

        // Diagnosticar: log material restore
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
        // Desactivar controladores dedicados correspondientes al power-up expirado
        switch (type)
        {
            case PowerUpType.Speed:
                if (speedVFXController != null)
                    speedVFXController.Deactivate();
                else
                {
                    if (speedParticleSystem != null)
                    {
                        speedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        speedParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(speedParticleSystem.gameObject, deactivateDelay));
                    }
                    if (speedExplosionParticleSystem != null)
                    {
                        speedExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        speedExplosionParticleSystem.Clear(true);
                        StartCoroutine(DeactivateAfterDelay(speedExplosionParticleSystem.gameObject, deactivateDelay));
                    }
                }
                break;
            case PowerUpType.Cosmic:
                if (cosmicVFXController != null)
                    cosmicVFXController.Deactivate();
                else
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
                break;
            case PowerUpType.Shield:
                // Asegurar que el material se restaure antes de desactivar el VFX del escudo
                if (materialController != null)
                {
                    materialController.RestoreDefault();
                }
                if (shieldVFXController != null)
                    shieldVFXController.Deactivate();
                break;
            default:
                break;
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
