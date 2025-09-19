using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities.DI
{
    public sealed class Container
    {
        private readonly Dictionary<Type, object> m_Singletons = new();
        private readonly Dictionary<Type, Func<Container, object>> m_SingletonFactories = new();

        private readonly Dictionary<Type, Func<Container, object>> m_Transients = new();

        public Container AddSingleton<TService>(TService instance)
        {
            m_Singletons[typeof(TService)] = instance!;
            return this;
        }

        public Container AddSingleton<TService, TImplementation>() where TImplementation : TService
        {
            m_SingletonFactories[typeof(TService)] = container => container.Create(typeof(TImplementation));
            return this;
        }

        public Container AddSingleton<TService>(Func<TService> factory)
        {
            m_SingletonFactories[typeof(TService)] = container => factory();
            return this;
        }

        public Container AddTransient<TService, TImplementation>() where TImplementation : TService
        {
            m_Transients[typeof(TService)] = container => container.Create(typeof(TImplementation));
            return this;
        }

        /// <summary>
        /// Register transient type with creation details. (Factory)
        /// </summary>
        public Container AddTransient<TService>(Func<Container, TService> factory)
        {
            m_Transients[typeof(TService)] = container => factory(container);
            return this;
        }

        public T Get<T>() => (T)Get(typeof(T));

        /// <param name="type"> Requested object type. </param>
        /// <returns> - Existing singleton instance if registered, <br/>
        /// - New instance from a registered factory, <br/>
        /// - New instance of the type if not abstract/interface.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if type is not registered and can't be created. </exception>
        public object Get(Type type)
        {
            if (m_Singletons.TryGetValue(type, out var singleton))
            {
                return singleton;
            }

            if (m_SingletonFactories.TryGetValue(type, out var factory))
            {
                var newSingleton = factory(this);
                m_Singletons[type] = newSingleton;

                return newSingleton;
            }

            if (m_Transients.TryGetValue(type, out var transient))
            {
                return transient(this);
            }

            if (!type.IsAbstract || !type.IsInterface)
            {
                return Create(type);
            }

            throw new InvalidOperationException($"Type {type} is not registered.");
        }

        private object Create(Type type)
        {
            // find constructors for type which is public and not static
            // prefers options that have "Inject" attribute and have more parameters(greedy)
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            var injectCtors = ctors.Where(c => c.IsDefined(typeof(InjectAttribute))).ToArray();
            if (injectCtors.Length > 1)
            {
                throw new InvalidOperationException($"Type {type} has multiple [Inject] constructors.");
            }

            var ctor = injectCtors.Length == 1
                ? injectCtors[0]
                : ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (ctor == null)
            {
                throw new InvalidOperationException($"Type {type} has no public constructor.");
            }

            // for each parameter for the chosen constructor,
            // get a registered object or create a new per Get function.
            var args = ctor.GetParameters().Select(p => Get(p.ParameterType)).ToArray();

            return Activator.CreateInstance(type, args);
        }
    }
}