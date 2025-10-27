using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/PowerUpData", fileName = "NewPowerUp")]
public class PowerUpData : ScriptableObject
{
    [Tooltip("Display name for the power-up")]
    public string powerUpName = "NewPowerUp";

    [Tooltip("Duration in seconds the power-up stays active on the player")]
    public float duration = 5f;

    [Tooltip("Material (shader) to apply to the player while the power-up is active")]
    public Material effectMaterial;

    [Tooltip("Particle VFX prefab to attach to the player while the power-up is active")]
    public GameObject playerVFXPrefab;

    [Tooltip("Pickup/explosion VFX prefab played when the collectible is picked up")]
    public GameObject pickupVFXPrefab;

    [Tooltip("Optional sound to play on pickup")]
    public AudioClip pickupSound;
}
