using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Zenject;
using UniRx;
using UniRx.Triggers;


public sealed class PlayerBall : PlayerBase
{
    [SerializeField] private Rigidbody _rigidbody;
    [Inject] private IAxisInput _input;
    private IDisposable _disposable;


    private void Start()
    {
        this.OnEnableAsObservable().Subscribe(_ => _disposable = _input.AxisInput.Subscribe(Move)).AddTo(this);
        this.OnDisableAsObservable().Subscribe(_ => _disposable.Dispose()).AddTo(this);
    }


    protected override void Move(Vector3 direction)
    {
        _rigidbody.AddForce(direction * Speed);
    }


}
