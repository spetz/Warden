using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class 
        SentryHooksConfiguration
    {
        private readonly HashSet<Expression<Action>> _onStart = new HashSet<Expression<Action>>();
        private readonly HashSet<Expression<Func<Task>>> _onStartAsync = new HashSet<Expression<Func<Task>>>();
        private readonly HashSet<Expression<Action>> _onPause = new HashSet<Expression<Action>>();
        private readonly HashSet<Expression<Func<Task>>> _onPauseAsync = new HashSet<Expression<Func<Task>>>();
        private readonly HashSet<Expression<Action>> _onStop = new HashSet<Expression<Action>>();
        private readonly HashSet<Expression<Func<Task>>> _onStopAsync = new HashSet<Expression<Func<Task>>>();
        private readonly HashSet<Expression<Action<Exception>>> _onError = new HashSet<Expression<Action<Exception>>>();
        private readonly HashSet<Expression<Func<Exception, Task>>> _onErrorAsync = new HashSet<Expression<Func<Exception, Task>>>();
        private readonly HashSet<Expression<Action<long>>> _onIterationStart = new HashSet<Expression<Action<long>>>();
        private readonly HashSet<Expression<Func<long, Task>>> _onIterationStartAsync = new HashSet<Expression<Func<long, Task>>>();
        private readonly HashSet<Expression<Action<ISentryIteration>>> _onIterationCompleted = new HashSet<Expression<Action<ISentryIteration>>>();
        private readonly HashSet<Expression<Func<ISentryIteration, Task>>> _onIterationCompletedAsync = new HashSet<Expression<Func<ISentryIteration, Task>>>();

        public IEnumerable<Expression<Action>> OnStart => _onStart;
        public IEnumerable<Expression<Func<Task>>> OnStartAsync => _onStartAsync;
        public IEnumerable<Expression<Action>> OnPause => _onPause;
        public IEnumerable<Expression<Func<Task>>> OnPauseAsync => _onPauseAsync;
        public IEnumerable<Expression<Action>> OnStop => _onStop;
        public IEnumerable<Expression<Func<Task>>> OnStopAsync => _onStopAsync;
        public IEnumerable<Expression<Action<Exception>>> OnError => _onError;
        public IEnumerable<Expression<Func<Exception, Task>>> OnErrorAsync => _onErrorAsync;
        public IEnumerable<Expression<Action<long>>> OnIterationStart => _onIterationStart;
        public IEnumerable<Expression<Func<long, Task>>> OnIterationStartAsync => _onIterationStartAsync;
        public IEnumerable<Expression<Action<ISentryIteration>>> OnIterationCompleted => _onIterationCompleted;
        public IEnumerable<Expression<Func<ISentryIteration, Task>>> OnIterationCompletedAsync => _onIterationCompletedAsync;

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

            public Builder OnStart(params Expression<Action>[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            public Builder OnStartAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnPause(params Expression<Action>[] hooks)
            {
                _configuration._onPause.UnionWith(hooks);
                return this;
            }

            public Builder OnPauseAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onPauseAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnStop(params Expression<Action>[] hooks)
            {
                _configuration._onStop.UnionWith(hooks);
                return this;
            }

            public Builder OnStopAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onStopAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnError(params Expression<Action<Exception>>[] hooks)
            {
                _configuration._onError.UnionWith(hooks);
                return this;
            }

            public Builder OnErrorAsync(params Expression<Func<Exception, Task>>[] hooks)
            {
                _configuration._onErrorAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationStart(params Expression<Action<long>>[] hooks)
            {
                _configuration._onIterationStart.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationStartAsync(params Expression<Func<long, Task>>[] hooks)
            {
                _configuration._onIterationStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationCompleted(params Expression<Action<ISentryIteration>>[] hooks)
            {
                _configuration._onIterationCompleted.UnionWith(hooks);
                return this;
            }

            public Builder OnIterationCompletedAsync(params Expression<Func<ISentryIteration, Task>>[] hooks)
            {
                _configuration._onIterationCompletedAsync.UnionWith(hooks);
                return this;
            }

            public SentryHooksConfiguration Build() => _configuration;
        }
    }
}
