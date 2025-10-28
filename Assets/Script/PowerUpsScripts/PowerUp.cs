using UnityEngine;

public enum PowerUpType { Speed, Cosmic, Shield }

public class PowerUp : MonoBehaviour
{
    public PowerUpType type = PowerUpType.Speed;
    [Tooltip("Duration in seconds for temporary power-ups. 0 = instant / one-shot.")]
    public float duration = 5f;
    [Tooltip("Optional VFX prefab to override the default for this power-up.")]
    public GameObject vfxPrefab;
}
