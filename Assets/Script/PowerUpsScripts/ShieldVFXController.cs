using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShieldVFXController : MonoBehaviour
{
    [Header("Dedicated Shield Systems")]
    [Tooltip("Looping particle system used while shield power-up is active (e.g. aura).")]
    public ParticleSystem shieldParticleSystem;
    [Tooltip("One-shot explosion particle system played when shield power-up is acquired.")]
    public ParticleSystem shieldExplosionParticleSystem;

    [Header("Timing")]
    public float deactivateDelay = 2f;
    public float trailHideTime = 0.01f;

    private readonly List<TrailRenderer> shieldTrails = new List<TrailRenderer>();
    private readonly List<TrailRenderer> explosionTrails = new List<TrailRenderer>();
    private readonly Dictionary<TrailRenderer, float> originalTrailTimes = new Dictionary<TrailRenderer, float>();

    void Awake()
    {
        InitDedicatedShieldSystems();
    }

    private void InitDedicatedShieldSystems()
    {
        if (shieldParticleSystem != null)
        {
            shieldParticleSystem.gameObject.SetActive(false);
            shieldTrails.Clear();
            foreach (var tr in shieldParticleSystem.GetComponentsInChildren<TrailRenderer>(true))
            {
                shieldTrails.Add(tr);
                originalTrailTimes[tr] = tr.time;
                tr.enabled = false;
            }
        }

        if (shieldExplosionParticleSystem != null)
        {
            shieldExplosionParticleSystem.gameObject.SetActive(false);
            explosionTrails.Clear();
            foreach (var tr in shieldExplosionParticleSystem.GetComponentsInChildren<TrailRenderer>(true))
            {
                explosionTrails.Add(tr);
                originalTrailTimes[tr] = tr.time;
                tr.enabled = false;
            }
        }
    }

    public void Activate()
    {
        if (shieldParticleSystem != null)
        {
            shieldParticleSystem.gameObject.SetActive(true);
            shieldParticleSystem.Play();
            foreach (var tr in shieldTrails)
            {
                if (tr == null) continue;
                if (originalTrailTimes.TryGetValue(tr, out var t))
                    tr.time = t;
                tr.enabled = true;
            }
        }

        if (shieldExplosionParticleSystem != null)
        {
            shieldExplosionParticleSystem.gameObject.SetActive(true);
            shieldExplosionParticleSystem.Play();
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
        if (shieldParticleSystem != null)
        {
            shieldParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            shieldParticleSystem.Clear(true);
            ClearAndDisableTrails(shieldTrails);
            StartCoroutine(DeactivateAfterDelay(shieldParticleSystem.gameObject, deactivateDelay));
        }

        if (shieldExplosionParticleSystem != null)
        {
            shieldExplosionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            shieldExplosionParticleSystem.Clear(true);
            ClearAndDisableTrails(explosionTrails);
            StartCoroutine(DeactivateAfterDelay(shieldExplosionParticleSystem.gameObject, deactivateDelay));
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
