using System.Collections.Generic;

namespace Utilities.Pooling
{
    public static class DictionaryPool<TKey, TValue>
    {
        private static readonly Queue<Dictionary<TKey, TValue>> s_Queue = new(5);
        
        
        public static Dictionary<TKey, TValue> Get()
        {
            if (s_Queue.Count < 1)
            {
                s_Queue.Enqueue(new Dictionary<TKey, TValue>());
            }

            return s_Queue.Dequeue();
        }

        public static void Release(Dictionary<TKey, TValue> dict)
        {
            dict.Clear();
            s_Queue.Enqueue(dict);
        }
    }
}