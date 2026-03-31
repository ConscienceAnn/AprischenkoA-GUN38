using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//musing System.Model;
using UniRx;
using Zenject;
using System.Runtime.CompilerServices;
using System;

public class InputPresenter : IInitializable, IDisposable
{
    private IDisposable _inputDisposable;
    private readonly KeyCode _savePlayer = KeyCode.C;
    private readonly KeyCode _loadPlayer = KeyCode.V;
    private IVectorSet _vectorSet;

    [Inject]
    private void Inject(IVectorSet vectorSet) => _vectorSet = vectorSet;

    public void Initialize()
    {
        _inputDisposable = Observable
            .EveryUpdate()
            .Where(t => Input.GetAxis("Horizontal") !=0 || Input.GetAxis("Vertical") != 0)
            .Subscribe(OnNext);
    }
    private void OnNext(long obj) => _vectorSet.SetVector(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));

    public void Dispose() => _inputDisposable.Dispose();

}
