using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SpeedVFXController : MonoBehaviour
{
    [Header("Dedicated Speed Systems")]
    [Tooltip("Sistema de partículas en bucle usado mientras el power-up de Velocidad está activo (p.ej. estela).")]
    public ParticleSystem speedParticleSystem;
    [Tooltip("Sistema de partículas de explosión de una sola vez reproducido cuando se obtiene el power-up de Velocidad.")]
    public ParticleSystem speedExplosionParticleSystem;

    [Header("Timing")]
    [Tooltip("Retraso antes de desactivar GameObjects de partículas de Velocidad para permitir que las estelas/partículas se desvanezcan.")]
    public float deactivateDelay = 2f;
    [Tooltip("Tiempo temporal de las estelas usado para ocultarlas rápidamente al limpiar.")]
    public float trailHideTime = 0.01f;

    private readonly List<TrailRenderer> speedTrails = new List<TrailRenderer>();
    private readonly List<TrailRenderer> explosionTrails = new List<TrailRenderer>();
    private readonly Dictionary<TrailRenderer, float> originalTrailTimes = new Dictionary<TrailRenderer, float>();

    void Awake()
    {
        InitDedicatedSpeedSystems();
    }

    private void InitDedicatedSpeedSystems()
    {
        if (speedParticleSystem != null)
        {
            speedParticleSystem.gameObject.SetActive(false);
            speedTrails.Clear();
            foreach (var tr in speedParticleSystem.GetComponentsInChildren<TrailRenderer>(true))
            {
                speedTrails.Add(tr);
                originalTrailTimes[tr] = tr.time;
                tr.enabled = false;
            }
        }

        if (speedExplosionParticleSystem != null)
        {
            speedExplosionParticleSystem.gameObject.SetActive(false);
            explosionTrails.Clear();
            foreach (var tr in speedExplosionParticleSystem.GetComponentsInChildren<TrailRenderer>(true))
            {
                explosionTrails.Add(tr);
                originalTrailTimes[tr] = tr.time;
                tr.enabled = false;
            }
        }
    }

    public void Activate()
    {
        if (speedParticleSystem != null)
        {
            speedParticleSystem.gameObject.SetActive(true);
            speedParticleSystem.Play();
            foreach (var tr in speedTrails)
            {
                if (tr == null) continue;
                if (originalTrailTimes.TryGetValue(tr, out var t))
                    tr.time = t;
                tr.enabled = true;
            }
        }

        if (speedExplosionParticleSystem != null)
        {
            speedExplosionParticleSystem.gameObject.SetActive(true);
            speedExplosionParticleSystem.Play();
            foreach (var tr in explosionTrails)
            {
                if (tr == null) continue;
                if (originalTrailTimes.TryGetValue(tr, out var t))
                    tr.time = t;
                tr.enabled = true;
            }
        }
    }

    public void Deactivate()
    {
        if (speedParticleSystem != null)
        {
            speedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            speedParticleSystem.Clear(true);
            ClearAndDisableTrails(speedTrails);
            StartCoroutine(DeactivateAfterDelay(speedParticleSystem.gameObject, deactivateDelay));
        }

        if (speedExplosionParticleSystem != null)
        {
            speedExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            speedExplosionParticleSystem.Clear(true);
            ClearAndDisableTrails(explosionTrails);
            StartCoroutine(DeactivateAfterDelay(speedExplosionParticleSystem.gameObject, deactivateDelay));
        }
    }

    private void ClearAndDisableTrails(List<TrailRenderer> trails)
    {
        foreach (var tr in trails)
        {
            if (tr == null) continue;
            tr.Clear();
            tr.time = trailHideTime;
            tr.enabled = false;
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
