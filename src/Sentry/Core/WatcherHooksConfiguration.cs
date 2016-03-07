using System;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class WatcherHooksConfiguration
    {
        public Action<IWatcherCheck> OnStart { get; protected set; }
        public Func<IWatcherCheck, Task> OnStartAsync { get; protected set; }
        public Action<ISentryCheckResult> OnSuccess { get; protected set; }
        public Func<ISentryCheckResult, Task> OnSuccessAsync { get; protected set; }
        public Action<ISentryCheckResult> OnFailure { get; protected set; }
        public Func<ISentryCheckResult, Task> OnFailureAsync { get; protected set; }
        public Action<ISentryCheckResult> OnCompleted { get; protected set; }
        public Func<ISentryCheckResult,Task> OnCompletedAsync { get; protected set; }

        public static WatcherHooksConfiguration Empty => new WatcherHooksConfiguration();
        public static Builder Create() => new Builder();

        protected internal WatcherHooksConfiguration()
        {
            OnStart = _ => { };
            OnStartAsync = _ => Task.CompletedTask;
            OnSuccess = _ => { };
            OnSuccessAsync = _ => Task.CompletedTask;
            OnFailure = _ => { };
            OnFailureAsync = _ => Task.CompletedTask;
            OnCompleted = _ => { };
            OnCompletedAsync = _ => Task.CompletedTask;
        }

        public class Builder
        {
            private readonly WatcherHooksConfiguration _configuration = new WatcherHooksConfiguration();

            protected internal Builder()
            {
            }

            public Builder OnStart(Action<IWatcherCheck> hook)
            {
                _configuration.OnStart = hook;
                return this;
            }

            public Builder OnStartAsync(Func<IWatcherCheck, Task> hook)
            {
                _configuration.OnStartAsync = hook;
                return this;
            }

            public Builder OnSuccess(Action<ISentryCheckResult> hook)
            {
                _configuration.OnSuccess = hook;
                return this;
            }

            public Builder OnSuccessAsync(Func<ISentryCheckResult, Task> hook)
            {
                _configuration.OnSuccessAsync = hook;
                return this;
            }

            public Builder OnFailure(Action<ISentryCheckResult> hook)
            {
                _configuration.OnFailure = hook;
                return this;
            }

            public Builder OnFailureAsync(Func<ISentryCheckResult, Task> hook)
            {
                _configuration.OnFailureAsync = hook;
                return this;
            }

            public Builder OnCompleted(Action<ISentryCheckResult> hook)
            {
                _configuration.OnCompleted = hook;
                return this;
            }

            public Builder OnCompletedAsync(Func<ISentryCheckResult, Task> hook)
            {
                _configuration.OnCompletedAsync = hook;
                return this;
            }

            public WatcherHooksConfiguration Build() => _configuration;
        }
    }
}