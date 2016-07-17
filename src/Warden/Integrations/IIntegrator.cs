using System;
using System.Collections.Concurrent;

namespace Warden.Integrations
{
    public interface IIntegrator
    {
        void Register<T>(T integration) where T : class, IIntegration;
        T Resolve<T>() where T : class, IIntegration;
    }

    public class Integrator : IIntegrator
    {
        protected readonly ConcurrentDictionary<Type, IIntegration> Integrations = new ConcurrentDictionary<Type, IIntegration>(); 

        public void Register<T>(T integration) where T : class, IIntegration
        {
            var key = GetKey<T>();

            if (!Integrations.TryAdd(key, integration))
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has been already registered.");
        }

        public T Resolve<T>() where T : class, IIntegration
        {
            var key = GetKey<T>();

            IIntegration integration;
            if (!Integrations.TryGetValue(key, out integration))
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has not been registered.");

            return integration as T;
        }

        private static Type GetKey<T>() => typeof (T);
    }
}