using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DamageObject : InteractiveObject
{
    public override void Execute()
    {
       
    }

    protected override void Interaction(GameObject otherGameObject)
    {
        Debug.LogError("Damage applied");
    }
}
