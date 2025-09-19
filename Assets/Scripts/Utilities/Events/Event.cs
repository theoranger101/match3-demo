using System;
using System.Collections.Generic;

namespace Utilities.Events
{
    public delegate void EventListener<in T>(T evt) where T : Event;

    [Serializable]
    public class Event : IDisposable
    {
        public object target = null;
        
        public bool IsConsumed { get; protected set; }
        
        public Event(){}

        public void Consume()
        {
            IsConsumed = true;
        }

        public virtual void Dispose()
        {
            
        }
    }

    public abstract class Event<T> : Event where T : Event<T>, new()
    {
        private static readonly Stack<T> poolStack = new();

        public static T Get() => GetPooledInternal();

        protected static T GetPooledInternal()
        {
            if (poolStack.Count > 0)
            {
                var evt = poolStack.Pop();
                evt.Reset();

                return evt;
            }
            else
            {
                var evt = new T();
                return evt;
            }
        }

        private static void Return(T evt)
        {
            poolStack.Push(evt);
        }

        protected virtual void Reset()
        {
            target = null;
            IsConsumed = false;
        }

        public override void Dispose()
        {
            Return((T) this);
        }
    }
}