using UnityEngine;

public class PinTrigger : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            // Не уничтожаем мяч сразу!
            // Пусть GameManager сам решает когда уничтожать
        }
    }
}