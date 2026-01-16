using UnityEngine;

public class BallController : MonoBehaviour
{
    public float forceMultiplier = 500f;
    private Rigidbody rb;
    private Vector3 startPoint;
    private bool isDragging = false;
    private bool isLaunched = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        if (spawnPoint) transform.position = spawnPoint.transform.position;
    }

    void OnMouseDown()
    {
        if (isLaunched || !GameManager.Instance) return;
        isDragging = true;
        startPoint = GetMouseWorldPos();
    }

    void OnMouseUp()
    {
        if (!isDragging || isLaunched || !GameManager.Instance) return;

        isDragging = false;
        isLaunched = true;

        Vector3 endPoint = GetMouseWorldPos();
        Vector3 force = startPoint - endPoint;
        force.y = 0;
        force.z = Mathf.Abs(force.z);

        rb.isKinematic = false;
        rb.AddForce(force * forceMultiplier);

        // Уведомляем GameManager о броске
        GameManager.Instance.OnBallThrown();
    }

    Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}