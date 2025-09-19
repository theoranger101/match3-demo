using System.Collections.Generic;

namespace Utilities.Pooling
{
    public static class ListPool<T>
    {
        private static readonly Queue<List<T>> s_Queue = new(5);

        public static List<T> Get()
        {
            if (s_Queue.Count < 1)
            {
                s_Queue.Enqueue(new List<T>());
            }

            return s_Queue.Dequeue();
        }

        public static List<T> Get(T[] array)
        {
            var list = Get();
            list.AddRange(array);
            
            return list;
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            s_Queue.Enqueue(list);
        }
    }
}