using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sentry.Core
{
    /// <summary>
    /// Configuration of the hooks for the Sentry.
    /// </summary>
    public class SentryHooksConfiguration
    {
        private readonly ISet<Expression<Action>> _onStart = new HashSet<Expression<Action>>();
        private readonly ISet<Expression<Func<Task>>> _onStartAsync = new HashSet<Expression<Func<Task>>>();
        private readonly ISet<Expression<Action>> _onPause = new HashSet<Expression<Action>>();
        private readonly ISet<Expression<Func<Task>>> _onPauseAsync = new HashSet<Expression<Func<Task>>>();
        private readonly ISet<Expression<Action>> _onStop = new HashSet<Expression<Action>>();
        private readonly ISet<Expression<Func<Task>>> _onStopAsync = new HashSet<Expression<Func<Task>>>();
        private readonly ISet<Expression<Action<Exception>>> _onError = new HashSet<Expression<Action<Exception>>>();
        private readonly ISet<Expression<Func<Exception, Task>>> _onErrorAsync = new HashSet<Expression<Func<Exception, Task>>>();
        private readonly ISet<Expression<Action<long>>> _onIterationStart = new HashSet<Expression<Action<long>>>();
        private readonly ISet<Expression<Func<long, Task>>> _onIterationStartAsync = new HashSet<Expression<Func<long, Task>>>();
        private readonly ISet<Expression<Action<ISentryIteration>>> _onIterationCompleted = new HashSet<Expression<Action<ISentryIteration>>>();
        private readonly ISet<Expression<Func<ISentryIteration, Task>>> _onIterationCompletedAsync = new HashSet<Expression<Func<ISentryIteration, Task>>>();

        /// <summary>
        /// Set of unique OnStart hooks for the Sentry, invoked when the StartAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Action>> OnStart => _onStart;

        /// <summary>
        /// Set of unique OnStartAsync hooks for the Sentry, invoked when the StartAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Func<Task>>> OnStartAsync => _onStartAsync;

        /// <summary>
        /// Set of unique OnPause hooks for the Sentry, invoked when the PauseAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Action>> OnPause => _onPause;

        /// <summary>
        /// Set of unique OnPauseAsync hooks for the Sentry, invoked when the PauseAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Func<Task>>> OnPauseAsync => _onPauseAsync;

        /// <summary>
        /// Set of unique OnStop hooks for the Sentry, invoked when the StopAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Action>> OnStop => _onStop;

        /// <summary>
        /// Set of unique OnStopAsync hooks for the Sentry, invoked when the StopAsync() is executed.
        /// </summary>
        public IEnumerable<Expression<Func<Task>>> OnStopAsync => _onStopAsync;

        /// <summary>
        /// Set of unique OnError hooks for the Sentry, 
        /// invoked when the ExecuteAsync() responsible for processing the iteration threw an exception.
        /// </summary>
        public IEnumerable<Expression<Action<Exception>>> OnError => _onError;

        /// <summary>
        /// Set of unique OnErrorAsync hooks for the Sentry, 
        /// invoked when the ExecuteAsync() responsible for processing the iteration threw an exception.
        /// </summary>
        public IEnumerable<Expression<Func<Exception, Task>>> OnErrorAsync => _onErrorAsync;

        /// <summary>
        /// Set of unique OnIterationStart hooks for the Sentry, 
        /// invoked when the ExecuteAsync() is started by the IIterationProcessor.
        /// </summary>
        public IEnumerable<Expression<Action<long>>> OnIterationStart => _onIterationStart;

        /// <summary>
        /// Set of unique OnIterationStartAsync hooks for the Sentry,
        /// invoked when the ExecuteAsync() is started by the IIterationProcessor.
        /// </summary>
        public IEnumerable<Expression<Func<long, Task>>> OnIterationStartAsync => _onIterationStartAsync;

        /// <summary>
        /// Set of unique OnIterationCompleted hooks for the Sentry, invoked when the ExecuteAsync() has completed.
        /// </summary>
        public IEnumerable<Expression<Action<ISentryIteration>>> OnIterationCompleted => _onIterationCompleted;

        /// <summary>
        /// Set of unique OnIterationCompletedAsync hooks for the Sentry, invoked when the ExecuteAsync() has completed.
        /// </summary>
        public IEnumerable<Expression<Func<ISentryIteration, Task>>> OnIterationCompletedAsync => _onIterationCompletedAsync;

        protected internal SentryHooksConfiguration()
        {
        }

        public static SentryHooksConfiguration Empty => new SentryHooksConfiguration();
        public static Builder Create() => new Builder();

        /// <summary>
        /// Factory method for creating a new intance of fluent builder for the SentryHooksConfiguration.
        /// </summary>
        /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
        public class Builder
        {
            private readonly SentryHooksConfiguration _configuration = new SentryHooksConfiguration();

            protected internal Builder()
            {
            }

            /// <summary>
            /// One or more unique OnStart hooks for the Sentry, invoked when the StartAsync() is executed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnStart(params Expression<Action>[] hooks)
            {
                _configuration._onStart.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnStartAsync hooks for the Sentry, invoked when the StartAsync() is executed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnStartAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onStartAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnPause hooks for the Sentry, invoked when the PauseAsync() is executed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnPause(params Expression<Action>[] hooks)
            {
                _configuration._onPause.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnPauseAsync hooks for the Sentry, invoked when the PauseAsync() is executed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnPauseAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onPauseAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnStop hooks for the Sentry, invoked when the StopAsync() is executed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnStop(params Expression<Action>[] hooks)
            {
                _configuration._onStop.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnStart hooks for the Sentry, invoked when the XXXXXXXXX.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnStopAsync(params Expression<Func<Task>>[] hooks)
            {
                _configuration._onStopAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnError hooks for the Sentry, 
            /// invoked when the ExecuteAsync() responsible for processing the iteration threw an exception.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnError(params Expression<Action<Exception>>[] hooks)
            {
                _configuration._onError.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnErrorAsync hooks for the Sentry, 
            /// invoked when the ExecuteAsync() responsible for processing the iteration threw an exception.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnErrorAsync(params Expression<Func<Exception, Task>>[] hooks)
            {
                _configuration._onErrorAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnIterationStart hooks for the Sentry, 
            /// invoked when the ExecuteAsync() is started by the IIterationProcessor.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnIterationStart(params Expression<Action<long>>[] hooks)
            {
                _configuration._onIterationStart.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnIterationStartAsync hooks for the Sentry,
            /// invoked when the ExecuteAsync() is started by the IIterationProcessor.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnIterationStartAsync(params Expression<Func<long, Task>>[] hooks)
            {
                _configuration._onIterationStartAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnIterationCompleted hooks for the Sentry, invoked when the ExecuteAsync() has completed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnIterationCompleted(params Expression<Action<ISentryIteration>>[] hooks)
            {
                _configuration._onIterationCompleted.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// One or more unique OnIterationCompletedAsync hooks for the Sentry, invoked when the ExecuteAsync() has completed.
            /// </summary>
            /// <param name="hooks">One or more custom Sentry hooks.</param>
            /// <returns>Instance of fluent builder for the SentryHooksConfiguration.</returns>
            public Builder OnIterationCompletedAsync(params Expression<Func<ISentryIteration, Task>>[] hooks)
            {
                _configuration._onIterationCompletedAsync.UnionWith(hooks);
                return this;
            }

            /// <summary>
            /// Builds the SentryHooksConfiguration and return its instance.
            /// </summary>
            /// <returns>Instance of SentryHooksConfiguration.</returns>
            public SentryHooksConfiguration Build() => _configuration;
        }
    }
}
