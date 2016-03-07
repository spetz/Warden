using System;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class SentryHooksConfiguration
    {
        public Action OnStart { get; protected set; }
        public Func<Task> OnStartAsync { get; protected set; }
        public Action OnStop { get; protected set; }
        public Func<Task> OnStopAsync { get; protected set; }
        public Action<Exception> OnError { get; protected set; }
        public Func<Exception, Task> OnErrorAsync { get; protected set; }
        public Action<long> OnIterationStart { get; protected set; }
        public Func<long, Task> OnIterationStartAsync { get; protected set; }
        public Action<ISentryIteration> OnIterationCompleted { get; protected set; }
        public Func<ISentryIteration, Task> OnIterationCompletedAsync { get; protected set; }


        public static SentryHooksConfiguration Empty => new SentryHooksConfiguration();
        public static Builder Create() => new Builder();

        protected internal SentryHooksConfiguration()
        {
            OnStart = () => { };
            OnStartAsync = () => Task.CompletedTask;
            OnStop = () => { };
            OnStopAsync = () => Task.CompletedTask;
            OnError = _ => { };
            OnErrorAsync = _ => Task.CompletedTask;
            OnIterationStart = _ => { };
            OnIterationStartAsync = _ => Task.CompletedTask;
            OnIterationCompleted = _ => { };
            OnIterationCompletedAsync = _ => Task.CompletedTask;
        }

        public class Builder
        {
            private readonly SentryHooksConfiguration _configuration = new SentryHooksConfiguration();

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

            public Builder OnStop(Action hook)
            {
                _configuration.OnStop = hook;
                return this;
            }

            public Builder OnStopAsync(Func<Task> hook)
            {
                _configuration.OnStopAsync = hook;
                return this;
            }

            public Builder OnError(Action<Exception> hook)
            {
                _configuration.OnError = hook;
                return this;
            }

            public Builder OnErrorAsync(Func<Exception, Task> hook)
            {
                _configuration.OnErrorAsync = hook;
                return this;
            }

            public Builder OnIterationStart(Action<long> hook)
            {
                _configuration.OnIterationStart = hook;
                return this;
            }

            public Builder OnIterationStartAsync(Func<long, Task> hook)
            {
                _configuration.OnIterationStartAsync = hook;
                return this;
            }

            public Builder OnIterationCompleted(Action<ISentryIteration> hook)
            {
                _configuration.OnIterationCompleted = hook;
                return this;
            }

            public Builder OnIterationCompletedAsync(Func<ISentryIteration, Task> hook)
            {
                _configuration.OnIterationCompletedAsync = hook;
                return this;
            }

            public SentryHooksConfiguration Build() => _configuration;
        }
    }
}
