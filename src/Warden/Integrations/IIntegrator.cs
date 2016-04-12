using System;
using System.Collections.Generic;

namespace Warden.Integrations
{
    public interface IIntegrator
    {
        void Register<T>(T integration) where T : class, IIntegration;
        T Resolve<T>() where T : class, IIntegration;
    }

    public class Integrator : IIntegrator
    {
        protected readonly IDictionary<Type, IIntegration> Integrations = new Dictionary<Type, IIntegration>();

        public void Register<T>(T integration) where T : class, IIntegration
        {
            var key = GetKey<T>();
            if (Integrations.ContainsKey(key))
            {
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has been already registered.");
            }

            Integrations[key] = integration;
        }

        public T Resolve<T>() where T : class, IIntegration
        {
            var key = GetKey<T>();
            if (!Integrations.ContainsKey(key))
            {
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has not been registered.");
            }

            return Integrations[key] as T;
        }

        private static Type GetKey<T>() => typeof(T);
    }
}