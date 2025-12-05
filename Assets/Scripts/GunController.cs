using UnityEngine;

public class GunControllerPhysics : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        movement = new Vector2(horizontalInput * moveSpeed, 0);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(movement.x, 0);
    }
}