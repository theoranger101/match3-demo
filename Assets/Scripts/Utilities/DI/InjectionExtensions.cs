using System;
using System.Reflection;

namespace Utilities.DI
{
    public static class InjectionExtensions
    {
        /// <summary>
        /// Takes the target object and injects values into any existing field and property with Inject attribute.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="target"></param>
        public static void InjectInto(this Container container, object target)
        {
            var type = target.GetType();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!Attribute.IsDefined(field, typeof(InjectAttribute)))
                {
                    continue;
                }

                var value = container.Get(field.FieldType);
                field.SetValue(target, value);
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                        BindingFlags.NonPublic))
            {
                if (!Attribute.IsDefined(property, typeof(InjectAttribute)))
                {
                    continue;
                }

                var value = container.Get(property.PropertyType);
                property.SetValue(target, value);
            }
        }
    }
}