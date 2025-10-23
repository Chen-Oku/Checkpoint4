using UnityEngine;

// This script should be attached to the player GameObject. It detects PowerUp pickups and
// forwards the activation to a PlayerVFXController (on the same GameObject).
public class ActivatePowerUp : MonoBehaviour
{
    private PlayerVFXController vfxController;

    void Awake()
    {
        vfxController = GetComponent<PlayerVFXController>();
        if (vfxController == null)
        {
            Debug.LogWarning("PlayerVFXController not found on player. Add one to handle VFX.");
        }
    }

    // When the player touches a PowerUp object, activate the corresponding VFX and destroy the pickup.
    void OnTriggerEnter(Collider other)
    {
        var power = other.GetComponent<PowerUp>();
        if (power != null)
        {
            if (vfxController != null)
            {
                vfxController.ActivatePowerUp(power.type, power.duration, power.vfxPrefab);
            }
            else
            {
                Debug.Log("Picked up power-up: " + power.type + " but no VFX controller available.");
            }

            // Optionally destroy the power-up object so it can't be reused
            Destroy(other.gameObject);
        }
    }
}
