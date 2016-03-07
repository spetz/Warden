using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class WatcherHooksConfiguration
    {
        private readonly HashSet<Action<IWatcherCheck>> _onStart = new HashSet<Action<IWatcherCheck>>();
        private readonly HashSet<Func<IWatcherCheck, Task>> _onStartAsync = new HashSet<Func<IWatcherCheck, Task>>();
        private readonly HashSet<Action<ISentryCheckResult>> _onSuccess = new HashSet<Action<ISentryCheckResult>>();
        private readonly HashSet<Func<ISentryCheckResult, Task>> _onSuccessAsync = new HashSet<Func<ISentryCheckResult, Task>>();
        private readonly HashSet<Action<ISentryCheckResult>> _onFailure = new HashSet<Action<ISentryCheckResult>>();
        private readonly HashSet<Func<ISentryCheckResult, Task>> _onFailureAsync = new HashSet<Func<ISentryCheckResult, Task>>();
        private readonly HashSet<Action<ISentryCheckResult>> _onCompleted = new HashSet<Action<ISentryCheckResult>>();
        private readonly HashSet<Func<ISentryCheckResult, Task>> _onCompletedAsync = new HashSet<Func<ISentryCheckResult, Task>>();
        private readonly HashSet<Action<Exception>> _onError = new HashSet<Action<Exception>>();
        private readonly HashSet<Func<Exception, Task>> _onErrorAsync = new HashSet<Func<Exception, Task>>();

        public IEnumerable<Action<IWatcherCheck>> OnStart => _onStart;
        public IEnumerable<Func<IWatcherCheck, Task>> OnStartAsync => _onStartAsync;
        public IEnumerable<Action<ISentryCheckResult>> OnSuccess => _onSuccess;
        public IEnumerable<Func<ISentryCheckResult, Task>> OnSuccessAsync => _onSuccessAsync;
        public IEnumerable<Action<ISentryCheckResult>> OnFailure => _onFailure;
        public IEnumerable<Func<ISentryCheckResult, Task>> OnFailureAsync => _onFailureAsync;
        public IEnumerable<Action<ISentryCheckResult>> OnCompleted => _onCompleted;
        public IEnumerable<Func<ISentryCheckResult, Task>> OnCompletedAsync => _onCompletedAsync;
        public IEnumerable<Action<Exception>> OnError => _onError;
        public IEnumerable<Func<Exception, Task>> OnErrorAsync => _onErrorAsync;

        protected internal WatcherHooksConfiguration()
        {
        }

        public static WatcherHooksConfiguration Empty => new WatcherHooksConfiguration();
        public static Builder Create() => new Builder();

        public class Builder
        {
            private readonly WatcherHooksConfiguration _configuration = new WatcherHooksConfiguration();

            protected internal Builder()
            {
            }

            public Builder OnStart(params Action<IWatcherCheck>[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            public Builder OnStartAsync(params Func<IWatcherCheck, Task>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnSuccess(params Action<ISentryCheckResult>[] hooks)
            {
                _configuration._onSuccess.UnionWith(hooks);
                return this;
            }

            public Builder OnSuccessAsync(params Func<ISentryCheckResult, Task>[] hooks)
            {
                _configuration._onSuccessAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnFailure(params Action<ISentryCheckResult>[] hooks)
            {
                _configuration._onFailure.UnionWith(hooks);
                return this;
            }

            public Builder OnFailureAsync(params Func<ISentryCheckResult, Task>[] hooks)
            {
                _configuration._onFailureAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnCompleted(params Action<ISentryCheckResult>[] hooks)
            {
                _configuration._onCompleted.UnionWith(hooks);
                return this;
            }

            public Builder OnCompletedAsync(params Func<ISentryCheckResult, Task>[] hooks)
            {
                _configuration._onCompletedAsync.UnionWith(hooks);
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

            public WatcherHooksConfiguration Build() => _configuration;
        }
    }
}