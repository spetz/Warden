using System;
using System.Collections.Generic;

namespace Sentry
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
            if (Integrations.ContainsKey(typeof (T)))
            {
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has been already registered.");
            }

            Integrations[typeof (T)] = integration;
        }

        public T Resolve<T>() where T : class, IIntegration
        {
            if (!Integrations.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"Integration: {typeof(T).Name} has not been registered.");
            }

            return Integrations[typeof (T)] as T;
        }
    }
}