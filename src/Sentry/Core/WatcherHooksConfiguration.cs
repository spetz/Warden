using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public class WatcherHooksConfiguration
    {
        private readonly HashSet<Expression<Action<IWatcherCheck>>> _onStart = new HashSet<Expression<Action<IWatcherCheck>>>();
        private readonly HashSet<Expression<Func<IWatcherCheck, Task>>> _onStartAsync = new HashSet<Expression<Func<IWatcherCheck, Task>>>();
        private readonly HashSet<Expression<Action<ISentryCheckResult>>> _onSuccess = new HashSet<Expression<Action<ISentryCheckResult>>>();
        private readonly HashSet<Expression<Func<ISentryCheckResult, Task>>> _onSuccessAsync = new HashSet<Expression<Func<ISentryCheckResult, Task>>>();
        private readonly HashSet<Expression<Action<ISentryCheckResult>>> _onFailure = new HashSet<Expression<Action<ISentryCheckResult>>>();
        private readonly HashSet<Expression<Func<ISentryCheckResult, Task>>> _onFailureAsync = new HashSet<Expression<Func<ISentryCheckResult, Task>>>();
        private readonly HashSet<Expression<Action<ISentryCheckResult>>> _onCompleted = new HashSet<Expression<Action<ISentryCheckResult>>>();
        private readonly HashSet<Expression<Func<ISentryCheckResult, Task>>> _onCompletedAsync = new HashSet<Expression<Func<ISentryCheckResult, Task>>>();
        private readonly HashSet<Expression<Action<Exception>>> _onError = new HashSet<Expression<Action<Exception>>>();
        private readonly HashSet<Expression<Func<Exception, Task>>> _onErrorAsync = new HashSet<Expression<Func<Exception, Task>>>();

        public IEnumerable<Expression<Action<IWatcherCheck>>> OnStart => _onStart;
        public IEnumerable<Expression<Func<IWatcherCheck, Task>>> OnStartAsync => _onStartAsync;
        public IEnumerable<Expression<Action<ISentryCheckResult>>> OnSuccess => _onSuccess;
        public IEnumerable<Expression<Func<ISentryCheckResult, Task>>> OnSuccessAsync => _onSuccessAsync;
        public IEnumerable<Expression<Action<ISentryCheckResult>>> OnFailure => _onFailure;
        public IEnumerable<Expression<Func<ISentryCheckResult, Task>>> OnFailureAsync => _onFailureAsync;
        public IEnumerable<Expression<Action<ISentryCheckResult>>> OnCompleted => _onCompleted;
        public IEnumerable<Expression<Func<ISentryCheckResult, Task>>> OnCompletedAsync => _onCompletedAsync;
        public IEnumerable<Expression<Action<Exception>>> OnError => _onError;
        public IEnumerable<Expression<Func<Exception, Task>>> OnErrorAsync => _onErrorAsync;

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

            public Builder OnStart(params Expression<Action<IWatcherCheck>>[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            public Builder OnStartAsync(params Expression<Func<IWatcherCheck, Task>>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnSuccess(params Expression<Action<ISentryCheckResult>>[] hooks)
            {
                _configuration._onSuccess.UnionWith(hooks);
                return this;
            }

            public Builder OnSuccessAsync(params Expression<Func<ISentryCheckResult, Task>>[] hooks)
            {
                _configuration._onSuccessAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnFailure(params Expression<Action<ISentryCheckResult>>[] hooks)
            {
                _configuration._onFailure.UnionWith(hooks);
                return this;
            }

            public Builder OnFailureAsync(params Expression<Func<ISentryCheckResult, Task>>[] hooks)
            {
                _configuration._onFailureAsync.UnionWith(hooks);
                return this;
            }

            public Builder OnCompleted(params Expression<Action<ISentryCheckResult>>[] hooks)
            {
                _configuration._onCompleted.UnionWith(hooks);
                return this;
            }

            public Builder OnCompletedAsync(params Expression<Func<ISentryCheckResult, Task>>[] hooks)
            {
                _configuration._onCompletedAsync.UnionWith(hooks);
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

            public WatcherHooksConfiguration Build() => _configuration;
        }
    }
}