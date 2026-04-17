using UnityEngine;
using Core.MessageSystem;
using Messages;

namespace Gameplay
{
    public class StarObject : SceneObjectBase
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<MainObject>(out _))
            {
                Collect();
            }
        }

        private void Collect()
        {
            gameObject.SetActive(false);
            Messenger.Send(new StarCollected());
            Debug.Log($"Star collected! Total stars: {GetCollectedCount()}");
        }

        private int GetCollectedCount()
        {
            // Можно заменить на нормальный счетчик
            return PlayerPrefs.GetInt("StarsCollected", 0);
        }
    }
}