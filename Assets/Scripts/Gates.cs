using UnityEngine;

public class Gates : MonoBehaviour
{
    private int _score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Ball>(out _))
        {
            _score++;
            Destroy(other.gameObject); 
            Debug.Log("Ñ÷¸ò: " + _score);
        }
    }
}