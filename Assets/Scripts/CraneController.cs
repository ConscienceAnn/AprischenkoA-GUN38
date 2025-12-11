using UnityEngine;

public class CraneController : MonoBehaviour
{
    public float hookSpeed = 1f;
    public float moveSpeed = 3f;

    public Transform hook;
    public Transform ropeSprite;
    public Transform ropeMask;

    private void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            float moveY = vertical * hookSpeed * Time.deltaTime;
            hook.Translate(0, moveY, 0, Space.World);
            ropeSprite.Translate(0, moveY, 0, Space.World);
        }

        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0)
        {
            float moveX = horizontal * moveSpeed * Time.deltaTime;
            transform.Translate(moveX, 0, 0, Space.World);
        }
    }
}