using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : Health
{
    public float dieForce = 15.0f;
    Ragdoll ragdoll;
    ActiveWeapon weapons;
    CharacterAiming aiming;
    VolumeProfile postProcessing;
    CameraManager cameraManager;

    protected override void OnStart()
    {
        Debug.Log("=== PlayerHealth OnStart ===");

        ragdoll = GetComponent<Ragdoll>();
        Debug.Log($"Ragdoll: {(ragdoll != null ? ragdoll.name : "NULL")}");

        weapons = GetComponent<ActiveWeapon>();
        Debug.Log($"ActiveWeapon: {(weapons != null ? weapons.name : "NULL")}");

        aiming = GetComponent<CharacterAiming>();
        Debug.Log($"CharacterAiming: {(aiming != null ? aiming.name : "NULL")}");

        Volume volume = FindObjectOfType<Volume>();
        postProcessing = volume != null ? volume.profile : null;
        Debug.Log($"Volume Profile: {(postProcessing != null ? "Found" : "NULL")}");

        cameraManager = FindObjectOfType<CameraManager>();
        Debug.Log($"CameraManager: {(cameraManager != null ? cameraManager.name : "NULL")}");

        UpdateVignette();
    }

    protected override void OnDeath(Vector3 direction)
    {
        Debug.Log("=== PlayerHealth OnDeath ===");
        Debug.Log($"Direction: {direction}");
        Debug.Log($"dieForce: {dieForce}");

        // Проверяем каждый компонент перед использованием
        if (ragdoll != null)
        {
            Debug.Log("Activating ragdoll...");
            ragdoll.ActivateRagdoll();
            direction.Normalize();
            direction.y = 1.0f;
            ragdoll.ApplyForce(direction * dieForce);
        }
        else
        {
            Debug.LogError("ragdoll is NULL in OnDeath!");
        }

        if (weapons != null)
        {
            Debug.Log("Dropping weapon...");
            weapons.DropWeapon();
        }
        else
        {
            Debug.LogError("weapons is NULL in OnDeath!");
        }

        if (aiming != null)
        {
            Debug.Log("Disabling aiming...");
            aiming.enabled = false;
        }
        else
        {
            Debug.LogError("aiming is NULL in OnDeath!");
        }

        // Исправленный блок для cameraManager
        if (cameraManager == null)
        {
            Debug.Log("CameraManager not found, creating automatically...");
            GameObject camManagerObj = new GameObject("CameraManager");
            cameraManager = camManagerObj.AddComponent<CameraManager>();
        }

        Debug.Log("Enabling kill cam...");
        cameraManager.EnableKillCam();

        Debug.Log("=== PlayerHealth OnDeath Complete ===");
    }

    protected override void OnDamage(Vector3 direction)
    {
        Debug.Log($"Player took damage, health: {currentHealth}");
        UpdateVignette();
    }

    protected override void OnHeal(float amount)
    {
        Debug.Log($"Player healed, health: {currentHealth}");
        UpdateVignette();
    }

    private void UpdateVignette()
    {
        if (postProcessing == null)
        {
            Debug.LogWarning("postProcessing is NULL, skipping vignette update");
            return;
        }

        Vignette vignette;
        if (postProcessing.TryGet(out vignette))
        {
            float percent = 1.0f - (currentHealth / maxHealth);
            vignette.intensity.value = percent * 0.6f;
            Debug.Log($"Vignette updated to {percent * 0.6f}");
        }
    }

    public void TakeDamageFromTrap(float damage)
    {
        Debug.Log($"ЛОВУШКА: наносим урон {damage}. Текущее здоровье: {currentHealth}");

        // Вызываем существующий метод TakeDamage из родительского класса
        TakeDamage(damage, Vector3.zero);

        Debug.Log($"После урона здоровье: {currentHealth}");
    }
}