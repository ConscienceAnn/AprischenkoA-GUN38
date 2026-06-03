using System.Collections.Generic;
using UnityEngine;

namespace Core.Pools
{
    public sealed class GameObjectPool : IPool<GameObject>
    {
        private readonly Queue<GameObject> _gameObjects = new();

        public void InitializePool(GameObject pooledObject, int defaultCount)
        {
            for (int i = 0; i < defaultCount; i++)
            {
                var newGameObject = Object.Instantiate(pooledObject);
                newGameObject.SetActive(false);
                _gameObjects.Enqueue(newGameObject);
            }
        }

        public GameObject GetNextObject()
        {
            var returnObject = _gameObjects.Dequeue();
            returnObject.SetActive(true);
            return returnObject;
        }

        public void ReturnObject(GameObject pooledObject)
        {
            pooledObject.SetActive(false);
            _gameObjects.Enqueue(pooledObject);
        }
    }
}