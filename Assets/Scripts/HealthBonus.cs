using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interface;
using static UnityEngine.Random;

public sealed class GoodBonus : InteractiveObject, IFlay
{

    private Material _material;
    private float _lengthFlay;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        _material.color = Color.green;
        _lengthFlay = Range(1.0f, 0.5f);
    }

    protected override void Interaction(GameObject otherGameObject)
    {
        Debug.LogError("HP RESTORED");
    }

    public override void Execute()
    {
        if (!IsInteractable) { return; }
        Flay();
    }

    public void Flay()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.PingPong(Time.time, _lengthFlay), transform.localPosition.z);
    }




}

