using System;

namespace Sentry
{
    public interface IIntegrator
    {
        T Resolve<T>() where T : class, IIntegration;
    }

    public class Integrator : IIntegrator
    {
        public T Resolve<T>() where T : class, IIntegration
        {
            throw new NotImplementedException();
        }
    }
}