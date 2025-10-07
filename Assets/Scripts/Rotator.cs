using System.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 _rotate = new Vector3(0f, 90f, 0f); 

    private Rigidbody _rb;

    private IEnumerator Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Rotator: нужен Rigidbody!");
            yield break;
        }
        _rb.isKinematic = true;

 
        while (true)
        {
            Quaternion deltaRotation = Quaternion.Euler(_rotate * Time.fixedDeltaTime);
            _rb.MoveRotation(_rb.rotation * deltaRotation);
            yield return new WaitForFixedUpdate();
        }
    }
}