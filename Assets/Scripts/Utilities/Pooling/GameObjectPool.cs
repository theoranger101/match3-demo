using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pooling
{
    public class GameObjectPool<T> where T : MonoBehaviour
    {
        private T m_Prefab;
        private Transform m_Parent;
        private Queue<T> m_Pool = new();
        
        public GameObjectPool(T prefab, int initialSize = 1, Transform parent = null)
        {
            m_Prefab = prefab;
            m_Parent = parent;
            
            for(var i = 0; i < initialSize; i++)
            {
                var instance = Object.Instantiate(m_Prefab, m_Parent);
                instance.gameObject.SetActive(false);
                
                m_Pool.Enqueue(instance);
            }
        }

        public T Get(Transform parent = null)
        {
            T item = m_Pool.Count > 0 ? m_Pool.Dequeue() : Object.Instantiate(m_Prefab, parent ?? m_Parent);
            item.gameObject.SetActive(true);

            return item;
        }
        
        public void Release(T item)
        {
            if (item == null) return;

            item.gameObject.SetActive(false);
            item.transform.SetParent(m_Parent);
            
            m_Pool.Enqueue(item);
        }
    }
}