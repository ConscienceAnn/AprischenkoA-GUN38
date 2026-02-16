using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Health health;

    public void OnRaycastHit(RaycastWeapon weapon, Vector3 direction) {

        Debug.Log($"HitBox on {gameObject.name} hit by {weapon.name}, damage: {weapon.damage}");

        if (health == null)
        {
            Debug.LogError($"HitBox on {gameObject.name} has no Health reference!");
            return;
        }

        health.TakeDamage(weapon.damage, direction);
    }
}

