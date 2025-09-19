using System;
using System.Collections.Generic;

namespace Utilities.Promises
{
    public class Promise<T> : IDisposable
    {
        private static int s_AvailableId = 0;
        private static Dictionary<int, Action<bool, T>> s_ResultCallbacks = new();

        public static Promise<T> Create()
        {
            var promise = new Promise<T>()
            {
                m_Id = s_AvailableId++
            };
            
            s_ResultCallbacks.Add(promise.m_Id, delegate {});
            
            return promise;
        }

        public static bool IsValid(Promise<T> promise)
        {
            return s_ResultCallbacks.ContainsKey(promise.m_Id);
        }

        private int m_Id;
        private bool m_IsDone;
        private bool m_IsSuccessful;
        
        private T m_Result;
        
        public event Action<bool, T> OnResult
        {
            add
            {
                if (m_IsDone)
                {
                    value(m_IsSuccessful, m_Result);
                }
                
                s_ResultCallbacks[m_Id] += value;
            }

            remove => s_ResultCallbacks[m_Id] -= value;
        }

        public void Complete(T result)
        {
            if (m_IsDone)
            {
                return;
            }
            
            m_IsDone = true;
            m_IsSuccessful = true;
            m_Result = result;
            
            s_ResultCallbacks[m_Id].Invoke(true, m_Result);
        }

        public void Fail()
        {
            if (m_IsDone)
            {
                return;
            }
            
            m_IsDone = true;
            m_IsSuccessful = false;
            
            s_ResultCallbacks[m_Id].Invoke(false, default);
        }

        private void Release()
        {
            s_ResultCallbacks[m_Id] = null;
            s_ResultCallbacks.Remove(m_Id);
        }

        public void Dispose()
        {
            Release();
        }
    }
}