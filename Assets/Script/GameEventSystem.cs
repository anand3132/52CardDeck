using System;
using System.Collections.Generic;

namespace RedGaint.Games.Core
{
   public static class GameEventSystem
    {
        private static Dictionary<Type, Delegate> _eventHandlers = new Dictionary<Type, Delegate>();

        // Subscribe to an event with data
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Combine(_eventHandlers[eventType], handler);
            }
            else
            {
                _eventHandlers[eventType] = handler;
            }
        }

        // Unsubscribe from an event
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Remove(_eventHandlers[eventType], handler);
            
                if (_eventHandlers[eventType] == null)
                {
                    _eventHandlers.Remove(eventType);
                }
            }
        }

        // Trigger an event with data
        public static void Trigger<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);
            if (_eventHandlers.TryGetValue(eventType, out Delegate delegates))
            {
                (delegates as Action<T>)?.Invoke(eventData);
            }
        }

        // Clear all subscriptions (call when changing scenes)
        public static void Clear()
        {
            _eventHandlers.Clear();
        }
    }
}