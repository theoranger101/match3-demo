using System.Collections.Generic;

namespace Utilities.Pooling
{
    public static class HashSetPool<T>
    {
        private static readonly Queue<HashSet<T>> s_Queue = new(5);

        public static HashSet<T> Get()
        {
            if (s_Queue.Count < 1)
            {
                s_Queue.Enqueue(new HashSet<T>());
            }

            return s_Queue.Dequeue();
        }

        public static void Release(HashSet<T> list)
        {
            list.Clear();
            s_Queue.Enqueue(list);
        }
    }
}