using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float _forceMultiplier = 500f;

    [SerializeField] private float _maxSpeed = 70f; // Максимальная скорость мяча

    private Rigidbody _rb;
    private Vector3 _startPoint;
    private bool _isDragging = false;
    private bool _isLaunched = false;
    private GameManager _gameManager;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_rb.velocity.magnitude > _maxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * _maxSpeed;
        }
    }

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }

    private void OnMouseDown()
    {
        if (_isLaunched || _gameManager == null)
            return;

        _isDragging = true;
        _startPoint = GetMouseWorldPos();
    }

    private void OnMouseUp()
    {
        if (!_isDragging || _isLaunched || _gameManager == null)
            return;

        _isDragging = false;
        _isLaunched = true;

        Vector3 endPoint = GetMouseWorldPos();
        Vector3 force = _startPoint - endPoint;
        force.y = 0;
        force.z = Mathf.Abs(force.z);

        _rb.isKinematic = false;
        _rb.AddForce(force * _forceMultiplier);

        _gameManager.OnBallThrown();
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}