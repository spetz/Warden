using System;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class HooksConfiguration
    {
        public Action OnStart { get; protected set; }
        public Func<Task> OnStartAsync { get; protected set; }
        public Action<ISentryOutcome> OnSuccess { get; protected set; }
        public Func<ISentryOutcome, Task> OnSuccessAsync { get; protected set; }
        public Action<ISentryOutcome> OnFailure { get; protected set; }
        public Func<ISentryOutcome, Task> OnFailureAsync { get; protected set; }
        public Action<ISentryOutcome> OnCompleted { get; protected set; }
        public Func<ISentryOutcome,Task> OnCompletedAsync { get; protected set; }

        public static HooksConfiguration Empty => new HooksConfiguration();
        public static Builder Create() => new Builder();

        protected internal HooksConfiguration()
        {
            OnStart = () => { };
            OnStartAsync = () => Task.CompletedTask;
            OnSuccess = _ => { };
            OnSuccessAsync = _ => Task.CompletedTask;
            OnFailure = _ => { };
            OnFailureAsync = _ => Task.CompletedTask;
            OnCompleted = _ => { };
            OnCompletedAsync = _ => Task.CompletedTask;
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

            public Builder OnSuccess(Action<ISentryOutcome> hook)
            {
                _configuration.OnSuccess = hook;
                return this;
            }

            public Builder OnSuccessAsync(Func<ISentryOutcome, Task> hook)
            {
                _configuration.OnSuccessAsync = hook;
                return this;
            }

            public Builder OnFailure(Action<ISentryOutcome> hook)
            {
                _configuration.OnFailure = hook;
                return this;
            }

            public Builder OnFailureAsync(Func<ISentryOutcome, Task> hook)
            {
                _configuration.OnFailureAsync = hook;
                return this;
            }

            public Builder OnCompleted(Action<ISentryOutcome> hook)
            {
                _configuration.OnCompleted = hook;
                return this;
            }

            public Builder OnCompletedAsync(Func<ISentryOutcome, Task> hook)
            {
                _configuration.OnCompletedAsync = hook;
                return this;
            }

            public HooksConfiguration Build() => _configuration;
        }
    }
}