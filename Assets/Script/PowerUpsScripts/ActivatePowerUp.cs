using UnityEngine;

// Este script se pone en el jugador para manejar la activación de power-ups al entrar en contacto con ellos.
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

            // opcional: destruir el power-up después de recogerlo
            Destroy(other.gameObject);
        }
    }
}
