using UnityEngine;
using Utilities.DI;

namespace Core.Services
{
    /// <summary>
    /// Provides the root DI <see cref="Container"/> for the application.
    /// Ensures a single instance and exposes it via <see cref="Root"/>.
    /// </summary>
    [DefaultExecutionOrder((int)ExecutionOrders.ContainerProvider)]
    public sealed class ContainerProvider : MonoBehaviour
    {
        public static Container Root { get; private set; }

        private void Awake()
        {
            if (Root != null)
            {
                Destroy(gameObject);
                return;
            }

            Root = new Container();
        }
    }
}