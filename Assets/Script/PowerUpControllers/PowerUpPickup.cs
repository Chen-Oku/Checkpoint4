using UnityEngine;

// Attach this to the collectible pickup object (with a trigger collider).
// When the Player (tagged with 'Player') collides, this spawns the pickup VFX,
// calls the player's PlayerVFXController to activate the power-up and destroys the pickup.
public class PowerUpPickup : MonoBehaviour
{
    public PowerUpData powerUpData;

    [Tooltip("Player tag used to detect the player")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var controller = other.GetComponentInParent<PlayerVFXControllerPOW>();
        if (controller == null)
        {
            Debug.LogWarning("PlayerVFXControllerPOW not found on player root. Attach it to the player.");
            return;
        }

        // Play pickup VFX at this position
        if (powerUpData != null && powerUpData.pickupVFXPrefab != null)
        {
            var inst = Instantiate(powerUpData.pickupVFXPrefab, transform.position, Quaternion.identity);
            var ps = inst.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(inst, 5f);
        }

        // Play pickup sound
        if (powerUpData != null && powerUpData.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(powerUpData.pickupSound, transform.position);
        }

        // Activate the power-up on the player (this will replace any existing one)
        controller.ActivatePowerUp(powerUpData);

        // Remove the pickup object
        Destroy(gameObject);
    }
}
