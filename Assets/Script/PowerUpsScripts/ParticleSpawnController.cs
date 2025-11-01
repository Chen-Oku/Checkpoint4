using UnityEngine;

public class ParticleSpawnController : MonoBehaviour
{
    public static GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null, float destroyAfter = 5f)
    {
        if (prefab == null) return null;
        var go = Instantiate(prefab, position, Quaternion.identity, parent);
        var systems = go.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems) ps.Play();
        if (destroyAfter > 0) Destroy(go, destroyAfter);
        return go;
    }
}
