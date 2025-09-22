using System.Collections.Generic;

namespace Utilities.Pooling
{
    public static class QueuePool<T>
    {
        private static readonly Queue<Queue<T>> s_Queue = new(5);

        public static Queue<T> Get()
        {
            if (s_Queue.Count < 1)
            {
                s_Queue.Enqueue(new Queue<T>());
            }

            return s_Queue.Dequeue();
        }

        public static Queue<T> Get(List<T> list)
        {
            var queue = Get();
            
            using (IEnumerator<T> en = list.GetEnumerator()) 
            {
                while(en.MoveNext()) 
                {
                    queue.Enqueue(en.Current);
                }
            }   
            
            return queue;
        }
        
        public static void Release(Queue<T> queue)
        {
            queue.Clear();
            s_Queue.Enqueue(queue);
        }
    }
}