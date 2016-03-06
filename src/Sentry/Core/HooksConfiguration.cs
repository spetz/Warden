using System;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class HooksConfiguration
    {
        public Action OnStart { get; protected set; }
        public Func<Task> OnStartAsync { get; protected set; }
        public Action OnSuccess { get; protected set; }
        public Func<Task> OnSuccessAsync { get; protected set; }
        public Action<Exception> OnFailure { get; protected set; }
        public Func<Exception, Task> OnFailureAsync { get; protected set; }
        public Action OnCompleted { get; protected set; }
        public Func<Task> OnCompletedAsync { get; protected set; }

        public static HooksConfiguration Empty => new HooksConfiguration();
        public static Builder Configure() => new Builder();

        protected internal HooksConfiguration()
        {
            OnStart = () => { };
            OnStartAsync = () => Task.CompletedTask;
            OnSuccess = () => { };
            OnSuccessAsync = () => Task.CompletedTask;
            OnFailure = ex => { };
            OnFailureAsync = ex => Task.CompletedTask;
            OnCompleted = () => { };
            OnCompletedAsync = () => Task.CompletedTask;
        }

        public class Builder
        {
            private readonly HooksConfiguration _configuration = new HooksConfiguration();

            protected internal Builder()
            {
            }

            public Builder OnStart(Action hook)
            {
                _configuration.OnStart = hook;
                return this;
            }

            public Builder OnStartAsync(Func<Task> hook)
            {
                _configuration.OnStartAsync = hook;
                return this;
            }

            public Builder OnSuccess(Action hook)
            {
                _configuration.OnSuccess = hook;
                return this;
            }

            public Builder OnSuccessAsync(Func<Task> hook)
            {
                _configuration.OnSuccessAsync = hook;
                return this;
            }

            public Builder OnFailure(Action<Exception> hook)
            {
                _configuration.OnFailure = hook;
                return this;
            }

            public Builder OnFailureAsync(Func<Exception, Task> hook)
            {
                _configuration.OnFailureAsync = hook;
                return this;
            }

            public Builder OnCompleted(Action hook)
            {
                _configuration.OnCompleted = hook;
                return this;
            }

            public Builder OnCompletedAsync(Func<Task> hook)
            {
                _configuration.OnCompletedAsync = hook;
                return this;
            }

            public HooksConfiguration Build() => _configuration;
        }
    }
}