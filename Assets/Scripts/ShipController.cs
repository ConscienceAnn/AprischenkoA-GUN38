using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Physics")]
    [SerializeField] private float drag = 1f;
    [SerializeField] private float angularDrag = 2f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupPhysics();
    }

    void SetupPhysics()
    {
        rb.gravityScale = 0; 
        rb.drag = drag; 
        rb.angularDrag = angularDrag; 
    }

    void Update()
    {
     
        moveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

     
        if (moveInput.magnitude > 0.1f)
        {
            RotateTowardsMovement(moveInput);
        }
    }

    void FixedUpdate()
    {
  
        if (moveInput.magnitude > 0.1f)
        {
            Vector2 force = moveInput.normalized * moveSpeed;
            rb.AddForce(force);
        }
    }

    void RotateTowardsMovement(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}