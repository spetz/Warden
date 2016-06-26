using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Warden.Core
{
    /// <summary>
    /// Custom extension methods.
    /// </summary>
    public static class Extensions
    {
        public static void Execute(this IEnumerable<Expression<Action>> actions)
        {
            foreach (var action in actions)
            {
                action.Compile()();
            }
        }

        public static void Execute<T>(this IEnumerable<Expression<Action<T>>> actions, T value)
        {
            foreach (var action in actions)
            {
                action.Compile()(value);
            }
        }

        public static async Task ExecuteAsync(this IEnumerable<Expression<Func<Task>>> tasks)
        {
            await Task.WhenAll(tasks.ToList().Select(x => x.Compile()()));
        }

        public static async Task ExecuteAsync<T>(this IEnumerable<Expression<Func<T, Task>>> tasks, T value)
        {
            await Task.WhenAll(tasks.ToList().Select(x => x.Compile()(value)));
        }
    }
}