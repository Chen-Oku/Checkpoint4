using UnityEngine;

public enum PowerUpType { Speed, Cosmic, Shield }

public class PowerUp : MonoBehaviour
{
    public PowerUpType type = PowerUpType.Speed;
    [Tooltip("Duración en segundos para power-ups temporales. 0 = instantáneo / de un solo uso.")]
    public float duration = 5f;
    [Tooltip("Prefab de VFX opcional para sobrescribir el predeterminado de este power-up.")]
    public GameObject vfxPrefab;
}
