using UnityEngine;

public class PinTrigger : MonoBehaviour
{
    private bool isKnockedDown = false;

    void OnCollisionEnter(Collision collision)
    {
        // Если кегля столкнулась с мячом и еще не считалась сбитой
        if (collision.gameObject.CompareTag("Ball") && !isKnockedDown)
        {
            isKnockedDown = true;
            GameManager.Instance.PinKnockedDown();
        }
    }
}