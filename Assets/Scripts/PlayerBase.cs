using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    public float Speed = 3.0f;

    protected abstract void Move(Vector3 direction);



}
