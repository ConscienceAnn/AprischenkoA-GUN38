using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MessageSystem
{
    public static class Messenger
    {
        private static readonly Dictionary<Type, HashSet<IMessageListener>> _listeners = new();

        public static void Clear() => _listeners.Clear();

        public static void Send<TMessage>(TMessage message)
        {
            if (_listeners.TryGetValue(typeof(TMessage), out var listeners))
            {
                foreach (var listener in listeners)
                {
                    ((IMessageListener<TMessage>)listener).OnMessage(message);
                }
            }
            else
            {
                Debug.LogWarning($"No listener registered for {typeof(TMessage).Name}");
            }
        }

        public static void Subscribe<TMessage>(IMessageListener<TMessage> listener)
        {
            var type = typeof(TMessage);
            if (_listeners.TryGetValue(type, out var listeners))
            {
                if (listeners.Contains(listener))
                {
                    Debug.LogError($"{listener.GetType().Name} already subscribed to {type.Name}");
                    return;
                }
                listeners.Add(listener);
            }
            else
            {
                _listeners.Add(type, new HashSet<IMessageListener> { listener });
            }
        }

        public static void Unsubscribe<TMessage>(IMessageListener<TMessage> listener)
        {
            if (_listeners.TryGetValue(typeof(TMessage), out var listeners))
            {
                listeners.Remove(listener);
            }
        }
    }
}