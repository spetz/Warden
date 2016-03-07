using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class SentryHooksConfiguration
    {
        private readonly HashSet<Action> _onStart = new HashSet<Action>();
        private readonly HashSet<Func<Task>> _onStartAsync = new HashSet<Func<Task>>();
        private readonly HashSet<Action> _onStop = new HashSet<Action>();
        private readonly HashSet<Func<Task>> _onStopAsync = new HashSet<Func<Task>>();
        private readonly HashSet<Action<Exception>> _onError = new HashSet<Action<Exception>>();
        private readonly HashSet<Func<Exception, Task>> _onErrorAsync = new HashSet<Func<Exception, Task>>();
        private readonly HashSet<Action<long>> _onIterationStart = new HashSet<Action<long>>();
        private readonly HashSet<Func<long, Task>> _onIterationStartAsync = new HashSet<Func<long, Task>>();
        private readonly HashSet<Action<ISentryIteration>> _onIterationCompleted = new HashSet<Action<ISentryIteration>>();
        private readonly HashSet<Func<ISentryIteration, Task>> _onIterationCompletedAsync = new HashSet<Func<ISentryIteration, Task>>();

        public IEnumerable<Action> OnStart => _onStart;
        public IEnumerable<Func<Task>> OnStartAsync => _onStartAsync;
        public IEnumerable<Action> OnStop => _onStop;
        public IEnumerable<Func<Task>> OnStopAsync => _onStopAsync;
        public IEnumerable<Action<Exception>> OnError => _onError;
        public IEnumerable<Func<Exception, Task>> OnErrorAsync => _onErrorAsync;
        public IEnumerable<Action<long>> OnIterationStart => _onIterationStart;
        public IEnumerable<Func<long, Task>> OnIterationStartAsync => _onIterationStartAsync;
        public IEnumerable<Action<ISentryIteration>> OnIterationCompleted => _onIterationCompleted;
        public IEnumerable<Func<ISentryIteration, Task>> OnIterationCompletedAsync => _onIterationCompletedAsync;

        protected internal SentryHooksConfiguration()
        {
        }

        public static SentryHooksConfiguration Empty => new SentryHooksConfiguration();
        public static Builder Create() => new Builder();

        public class Builder
        {
            private readonly SentryHooksConfiguration _configuration = new SentryHooksConfiguration();

            protected internal Builder()
            {
            }

            public Builder OnStart(params Action[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            public Builder OnStartAsync(params Func<Task>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnStop(params Action[] hooks)
            {
                _configuration._onStop.UnionWith(hooks);
                return this;
            }

            public Builder OnStopAsync(params Func<Task>[] hooks)
            {
                _configuration._onStopAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnError(params Action<Exception>[] hooks)
            {
                _configuration._onError.UnionWith(hooks);
                return this;
            }

            public Builder OnErrorAsync(params Func<Exception, Task>[] hooks)
            {
                _configuration._onErrorAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationStart(params Action<long>[] hooks)
            {
                _configuration._onIterationStart.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationStartAsync(params Func<long, Task>[] hooks)
            {
                _configuration._onIterationStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationCompleted(params Action<ISentryIteration>[] hooks)
            {
                _configuration._onIterationCompleted.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationCompletedAsync(params Func<ISentryIteration, Task>[] hooks)
            {
                _configuration._onIterationCompletedAsync.UnionWith(hooks);
                return this;
            }

            public SentryHooksConfiguration Build() => _configuration;
        }
    }
}
