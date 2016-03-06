using System;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class HooksConfiguration
    {
        public Action<Exception> OnFailure { get; protected set; }
        public Action OnSuccess { get; protected set; }
        public Action OnCompleted { get; protected set; }
        public Func<Exception, Task> OnFailureAsync { get; protected set; }
        public Func<Task> OnSuccessAsync { get; protected set; }
        public Func<Task> OnCompletedAsync { get; protected set; }

        public static HooksConfiguration Empty => new HooksConfiguration();
        public static Builder Configure() => new Builder();

        protected internal HooksConfiguration()
        {
        }

        public class Builder
        {
            private readonly HooksConfiguration _configuration = new HooksConfiguration();

            protected internal Builder()
            {
            }

            public Builder OnFailure(Action<Exception> action)
            {
                _configuration.OnFailure = action;
                return this;
            }

            public Builder OnSuccess(Action action)
            {
                _configuration.OnSuccess = action;
                return this;
            }

            public Builder OnCompleted(Action action)
            {
                _configuration.OnCompleted = action;
                return this;
            }

            public Builder OnFailureAsync(Func<Exception, Task> action)
            {
                _configuration.OnFailureAsync = action;
                return this;
            }

            public Builder OnSuccessAsync(Func<Task> action)
            {
                _configuration.OnSuccessAsync = action;
                return this;
            }

            public Builder OnCompletedAsync(Func<Task> action)
            {
                _configuration.OnCompletedAsync = action;
                return this;
            }

            public HooksConfiguration Build() => _configuration;
        }
    }
}