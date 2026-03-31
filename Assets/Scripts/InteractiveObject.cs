using System.Collections;
using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UniRx;


public abstract class InteractiveObject : MonoBehaviour
{

    private bool _isInteractable;

    protected bool IsInteractable
    {
        get => _isInteractable;
        private set
        {
            _isInteractable = value;
            GetComponent<Renderer>().enabled = _isInteractable;
            GetComponent<Collider>().enabled = _isInteractable;
        }
    }

    protected abstract void Interaction(GameObject otherGameObject);

    public abstract void Execute();

    private void Start()
    {
        IsInteractable = true;

        this.OnCollisionEnterAsObservable()
            .Where(other => IsInteractable && other.gameObject.CompareTag("Player"))
            .Subscribe(collision => Interaction(collision.gameObject))
            .AddTo(this);

        this.OnTriggerEnterAsObservable()
            .Where(other => IsInteractable && other.gameObject.CompareTag("Player"))
            .Subscribe(other => Interaction(other.gameObject))
            .AddTo(this);

        Observable.EveryUpdate().Subscribe(l => Execute()).AddTo(this);

    }

}
