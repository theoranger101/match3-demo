using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Events
{
    public class EventListenerCollection
    {
        private Dictionary<Type, List<object>> events;
        
        public EventListenerCollection()
        {
            events = new Dictionary<Type, List<object>>();
        }

        public List<object> GetListenersForType(Type type)
        {
            events.TryGetValue(type, out var list);
            return list;
        }
        
        public void AddListener<T>(EventListener<T> listener) where T : Event
        {
             var eventType = typeof(T);
            if (!events.TryGetValue(eventType, out var list))
            {
                list = new List<object>();
                events[eventType] = list;
            }
            list.Add(listener);
        }
        
        public void RemoveListener<T>(EventListener<T> listener) where T : Event
        {
            var eventType = typeof(T);
            
            if (!events.TryGetValue(eventType, out var list)) return;
            
            list.Remove(listener);
        }
        
        public void SendEvent<T>(T evt) where T : Event
        {
            var eventType = typeof(T);
            if (!events.TryGetValue(eventType, out var listeners)) return;

            for (var i = 0; i < listeners.Count; i++)
            {
                var listener = listeners[i] as EventListener<T>;

                if (listener == null)
                {
                    throw new Exception(
                        $"Event types do not match. Expected {eventType}, but found {listeners[i].GetType()}");
                }

                try
                {
                    listener(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (evt.IsConsumed)
                {
                    break;
                }
            }
        }
    }
}