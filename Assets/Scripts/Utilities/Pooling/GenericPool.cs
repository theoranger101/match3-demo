using System.Collections.Generic;

namespace Utilities.Pooling
{
    public class GenericPool<T> where T : class, new()
    {
        private readonly Queue<T> m_Queue = new();

        public T Get()
        {
            return m_Queue.Count > 0 ? m_Queue.Dequeue() : new T();
        }
        
        public void Release(T item)
        {
            if (item == null) return;
            m_Queue.Enqueue(item);
        }
    }
}