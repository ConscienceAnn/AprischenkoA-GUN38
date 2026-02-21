using UnityEngine;

public class BombTracker : MonoBehaviour
{
    public BombSpawner spawner;

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnBombDestroyed();
        }
    }
}