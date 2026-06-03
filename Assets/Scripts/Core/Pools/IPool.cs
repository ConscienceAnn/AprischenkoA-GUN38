using Zenject;

namespace Core.Pools
{
    /// <summary>
    /// Implement to create custom pool with uint keys
    /// </summary>
    /// <typeparam name="T">Poolable type</typeparam>
    public interface IPool<T>
    {
        void InitializePool(T pooledObject, int defaultCount);
        
        T GetNextObject();

        void ReturnObject(T pooledObject);
    }
}