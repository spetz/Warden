using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Warden.Core
{
    /// <summary>
    /// Configuration of the hooks for the watcher.
    /// </summary>
    public class WatcherHooksConfiguration
    {
        private readonly ISet<Expression<Action<IWatcherCheck>>> _onStart = new HashSet<Expression<Action<IWatcherCheck>>>();
        private readonly ISet<Expression<Func<IWatcherCheck, Task>>> _onStartAsync = new HashSet<Expression<Func<IWatcherCheck, Task>>>();
        private readonly ISet<Expression<Action<IWardenCheckResult>>> _onSuccess = new HashSet<Expression<Action<IWardenCheckResult>>>();
        private readonly ISet<Expression<Func<IWardenCheckResult, Task>>> _onSuccessAsync = new HashSet<Expression<Func<IWardenCheckResult, Task>>>();
        private readonly ISet<Expression<Action<IWardenCheckResult>>> _onFirstSuccess = new HashSet<Expression<Action<IWardenCheckResult>>>();
        private readonly ISet<Expression<Func<IWardenCheckResult, Task>>> _onFirstSuccessAsync = new HashSet<Expression<Func<IWardenCheckResult, Task>>>();
        private readonly ISet<Expression<Action<IWardenCheckResult>>> _onFailure = new HashSet<Expression<Action<IWardenCheckResult>>>();
        private readonly ISet<Expression<Func<IWardenCheckResult, Task>>> _onFailureAsync = new HashSet<Expression<Func<IWardenCheckResult, Task>>>();
        private readonly ISet<Expression<Action<IWardenCheckResult>>> _onFirstFailure = new HashSet<Expression<Action<IWardenCheckResult>>>();
        private readonly ISet<Expression<Func<IWardenCheckResult, Task>>> _onFirstFailureAsync = new HashSet<Expression<Func<IWardenCheckResult, Task>>>();
        private readonly ISet<Expression<Action<IWardenCheckResult>>> _onCompleted = new HashSet<Expression<Action<IWardenCheckResult>>>();
        private readonly ISet<Expression<Func<IWardenCheckResult, Task>>> _onCompletedAsync = new HashSet<Expression<Func<IWardenCheckResult, Task>>>();
        private readonly ISet<Expression<Action<Exception>>> _onError = new HashSet<Expression<Action<Exception>>>();
        private readonly ISet<Expression<Func<Exception, Task>>> _onErrorAsync = new HashSet<Expression<Func<Exception, Task>>>();
        private readonly ISet<Expression<Action<Exception>>> _onFirstError = new HashSet<Expression<Action<Exception>>>();
        private readonly ISet<Expression<Func<Exception, Task>>> _onFirstErrorAsync = new HashSet<Expression<Func<Exception, Task>>>();

        /// <summary>
        /// Set of unique OnStart hooks for the watcher, invoked when the ExecuteAsync() is about to start.
        /// </summary>
        public IEnumerable<Expression<Action<IWatcherCheck>>> OnStart => _onStart;

        /// <summary>
        /// Set of unique OnStartAsync hooks for the watcher, invoked when the ExecuteAsync() is about to start.
        /// </summary>
        public IEnumerable<Expression<Func<IWatcherCheck, Task>>> OnStartAsync => _onStartAsync;

        /// <summary>
        /// Set of unique OnSuccess hooks for the watcher, invoked when the ExecuteAsync() succeeded.
        /// </summary>
        public IEnumerable<Expression<Action<IWardenCheckResult>>> OnSuccess => _onSuccess;

        /// <summary>
        /// Set of unique OnSuccessAsync hooks for the watcher, invoked when the ExecuteAsync() succeeded.
        /// </summary>
        public IEnumerable<Expression<Func<IWardenCheckResult, Task>>> OnSuccessAsync => _onSuccessAsync;

        /// <summary>
        /// Set of unique OnFirstSuccess hooks for the watcher, 
        /// invoked when the ExecuteAsync() succeeded for the first time after the previous check didn't succeed.
        /// </summary>
        public IEnumerable<Expression<Action<IWardenCheckResult>>> OnFirstSuccess => _onFirstSuccess;

        /// <summary>
        /// Set of unique OnFirstSuccessAsync hooks for the watcher, 
        /// invoked when the ExecuteAsync() succeeded for the first time after the previous check didn't succeed.
        /// </summary>
        public IEnumerable<Expression<Func<IWardenCheckResult, Task>>> OnFirstSuccessAsync => _onFirstSuccessAsync;

        /// <summary>
        /// Set of unique OnFailure hooks for the watcher, invoked when the ExecuteAsync() is failed.
        /// </summary>
        public IEnumerable<Expression<Action<IWardenCheckResult>>> OnFailure => _onFailure;

        /// <summary>
        /// Set of unique OnFailureAsync hooks for the watcher, invoked when the ExecuteAsync() is failed.
        /// </summary>
        public IEnumerable<Expression<Func<IWardenCheckResult, Task>>> OnFailureAsync => _onFailureAsync;

        /// <summary>
        /// Set of unique OnFirstFailure hooks for the watcher, 
        /// invoked when the ExecuteAsync() failed for the first time after the previous check did succeed.
        /// </summary>
        public IEnumerable<Expression<Action<IWardenCheckResult>>> OnFirstFailure => _onFirstFailure;

        /// <summary>
        /// Set of unique OnFirstFailureAsync hooks for the watcher, 
        /// invoked when the ExecuteAsync() failed for the first time after the previous check did succeed.
        /// </summary>
        public IEnumerable<Expression<Func<IWardenCheckResult, Task>>> OnFirstFailureAsync => _onFirstFailureAsync;

        /// <summary>
        /// Set of unique OnCompleted hooks for the watcher,
        /// invoked when the ExecuteAsync() completed, regardless it's succeeded or not.
        /// </summary>
        public IEnumerable<Expression<Action<IWardenCheckResult>>> OnCompleted => _onCompleted;

        /// <summary>
        /// Set of unique OnCompletedAsync hooks for the watcher, 
        /// invoked when the ExecuteAsync() completed, regardless it's succeeded or not.
        /// </summary>
        public IEnumerable<Expression<Func<IWardenCheckResult, Task>>> OnCompletedAsync => _onCompletedAsync;

        /// <summary>
        /// Set of unique OnError hooks for the watcher, invoked when the ExecuteAsync() threw an exception.
        /// </summary>
        public IEnumerable<Expression<Action<Exception>>> OnError => _onError;

        /// <summary>
        /// Set of unique OnErrorAsync hooks for the watcher, invoked when the ExecuteAsync() threw an exception.
        /// </summary>
        public IEnumerable<Expression<Func<Exception, Task>>> OnErrorAsync => _onErrorAsync;

        /// <summary>
        /// Set of unique OnFirstError hooks for the watcher, invoked when the ExecuteAsync()
        /// threw an exception for the first time after the previous check either succeeded or not.
        /// </summary>
        public IEnumerable<Expression<Action<Exception>>> OnFirstError => _onFirstError;

        /// <summary>
        /// Set of unique OnFirstErrorAsync hooks for the watcher, invoked when the ExecuteAsync()
        /// threw an exception for the first time after the previous check either succeeded or not.
        /// </summary>
        public IEnumerable<Expression<Func<Exception, Task>>> OnFirstErrorAsync => _onFirstErrorAsync;

        protected internal WatcherHooksConfiguration()
        {
        }

        /// <summary>
        /// An empty, default configuration of the watcher hooks, that has no hooks defined.
        /// </summary>
        public static WatcherHooksConfiguration Empty => new WatcherHooksConfiguration();

        /// <summary>
        /// Factory method for creating a new instance of fluent builder for the WatcherHooksConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
        public static Builder Create() => new Builder();

        public class Builder
        {
            private readonly WatcherHooksConfiguration _configuration = new WatcherHooksConfiguration();

            protected internal Builder()
            {
            }

            /// <summary>
            /// One or more unique OnStart hooks for the watcher, invoked when the ExecuteAsync() is about to start.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnStart(params Expression<Action<IWatcherCheck>>[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnStartAsync hooks for the watcher, invoked when the ExecuteAsync() is about to start.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnStartAsync(params Expression<Func<IWatcherCheck, Task>>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnSuccess hooks for the watcher, invoked when the ExecuteAsync() succeeded.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnSuccess(params Expression<Action<IWardenCheckResult>>[] hooks)
            {
                _configuration._onSuccess.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnSuccessAsync hooks for the watcher, invoked when the ExecuteAsync() succeeded.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnSuccessAsync(params Expression<Func<IWardenCheckResult, Task>>[] hooks)
            {
                _configuration._onSuccessAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstSuccess hooks for the watcher, 
            /// invoked when the ExecuteAsync() succeeded for the first time after the previous check didn't succeed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstSuccess(params Expression<Action<IWardenCheckResult>>[] hooks)
            {
                _configuration._onFirstSuccess.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstSuccessAsync hooks for the watcher, 
            /// invoked when the ExecuteAsync() succeeded for the first time after the previous check didn't succeed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstSuccessAsync(params Expression<Func<IWardenCheckResult, Task>>[] hooks)
            {
                _configuration._onFirstSuccessAsync.UnionWith(hooks);
                return this;
            }
            /// <summary>
            /// One or more unique OnFailure hooks for the watcher, invoked when the ExecuteAsync() is failed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFailure(params Expression<Action<IWardenCheckResult>>[] hooks)
            {
                _configuration._onFailure.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFailureAsync hooks for the watcher, invoked when the ExecuteAsync() is failed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFailureAsync(params Expression<Func<IWardenCheckResult, Task>>[] hooks)
            {
                _configuration._onFailureAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstFailure hooks for the watcher, 
            /// invoked when the ExecuteAsync() failed for the first time after the previous check did succeed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstFailure(params Expression<Action<IWardenCheckResult>>[] hooks)
            {
                _configuration._onFirstFailure.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstFailureAsync hooks for the watcher, 
            /// invoked when the ExecuteAsync() failed for the first time after the previous check did succeed.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstFailureAsync(params Expression<Func<IWardenCheckResult, Task>>[] hooks)
            {
                _configuration._onFirstFailureAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnCompleted hooks for the watcher,
            /// invoked when the ExecuteAsync() completed, regardless it's succeeded or not.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnCompleted(params Expression<Action<IWardenCheckResult>>[] hooks)
            {
                _configuration._onCompleted.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnCompletedAsync hooks for the watcher, 
            /// invoked when the ExecuteAsync() completed, regardless it's succeeded or not.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnCompletedAsync(params Expression<Func<IWardenCheckResult, Task>>[] hooks)
            {
                _configuration._onCompletedAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnError hooks for the watcher, invoked when the ExecuteAsync() threw an exception.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnError(params Expression<Action<Exception>>[] hooks)
            {
                _configuration._onError.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnErrorAsync hooks for the watcher, invoked when the ExecuteAsync() threw an exception.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnErrorAsync(params Expression<Func<Exception, Task>>[] hooks)
            {
                _configuration._onErrorAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstError hooks for the watcher, invoked when the ExecuteAsync()
            /// threw an exception for the first time after the previous check either succeeded or not.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstError(params Expression<Action<Exception>>[] hooks)
            {
                _configuration._onFirstError.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnFirstErrorAsync hooks for the watcher, invoked when the ExecuteAsync()
            /// threw an exception for the first time after the previous check either succeeded or not.
            /// </summary>
            /// <param name="hooks">One or more custom watcher hooks.</param>
            /// <returns>Instance of fluent builder for the WatcherHooksConfiguration.</returns>
            public Builder OnFirstErrorAsync(params Expression<Func<Exception, Task>>[] hooks)
            {
                _configuration._onFirstErrorAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// Builds the WatcherHooksConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of WatcherHooksConfiguration.</returns>
            public WatcherHooksConfiguration Build() => _configuration;
        }
    }
}