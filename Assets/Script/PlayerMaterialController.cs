using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMaterialController : MonoBehaviour
{
    [Header("Materials")]
    public Material defaultMaterial;
    public Material speedMaterial;
    public Material strengthMaterial;
    public Material shieldMaterial;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    void Awake()
    {
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

        if (defaultMaterial == null && renderers.Length > 0)
        {
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

    // Apply a material to all cached renderers (skips particle renderers)
    public void ApplyMaterial(Material mat, ParticleSystem skipSystem = null)
    {
        if (mat == null) return;
        foreach (var r in renderers)
        {
            if (skipSystem != null && IsChildOf(r.transform, skipSystem.gameObject.transform))
                continue;
            r.material = mat;
        }
    }

    public void RestoreDefault()
    {
        if (defaultMaterial != null)
            ApplyMaterial(defaultMaterial);
    }

    private bool IsChildOf(Transform t, Transform parent)
    {
        if (t == null || parent == null) return false;
        return t == parent || t.IsChildOf(parent);
    }
}
