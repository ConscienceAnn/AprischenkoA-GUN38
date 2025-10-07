using System.Collections;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private Vector3 _start = Vector3.zero;  
    [SerializeField] private Vector3 _end = new Vector3(2f, 0, 0);
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _delay = 1f;

    private Rigidbody _rb;
    private Vector3 _globalStart;
    private Vector3 _globalEnd;

    private IEnumerator Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Mover: нужен Rigidbody!");
            yield break;
        }
        _rb.isKinematic = true;

        _globalStart = transform.TransformPoint(_start);
        _globalEnd = transform.TransformPoint(_end);

        while (true)
        {
            yield return MoveBetween(_globalStart, _globalEnd);
            yield return new WaitForSeconds(_delay);

            yield return MoveBetween(_globalEnd, _globalStart);
            yield return new WaitForSeconds(_delay);
        }
    }

    private IEnumerator MoveBetween(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        if (distance < 0.001f) yield break;

        float duration = distance / _speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 pos = Vector3.Lerp(from, to, t);
            _rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        _rb.MovePosition(to);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 gStart = transform.TransformPoint(_start);
        Vector3 gEnd = transform.TransformPoint(_end);
        Gizmos.DrawSphere(gStart, 0.1f);
        Gizmos.DrawSphere(gEnd, 0.1f);
        Gizmos.DrawLine(gStart, gEnd);
    }
}