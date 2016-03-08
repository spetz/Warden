using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.Core
{
    public static class Extensions
    {
        public static void Execute(this IEnumerable<Action> actions)
        {
            actions.ToList().ForEach(x => x());
        }

        public static void Execute<T>(this IEnumerable<Action<T>> actions, T value)
        {
            actions.ToList().ForEach(x => x(value));
        }

        public static async Task ExecuteAsync(this IEnumerable<Func<Task>> tasks)
        {
            await Task.WhenAll(tasks.ToList().Select(x => x()));
        }

        public static async Task ExecuteAsync<T>(this IEnumerable<Func<T, Task>> tasks, T value)
        {
            await Task.WhenAll(tasks.ToList().Select(x => x(value)));
        }
    }
}