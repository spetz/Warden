using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Warden.Tests.EndToEnd
{
    //TODO: Remove test utils when MSpec works with DNX 
    public class Subject : TestFixtureAttribute
    {
        public Subject(string name)
        {
        }
    }

    public class AwaitResult
    {

        public AwaitResult(Task task)
        {
            AsTask = task;
        }

        public Task AsTask { get; }
    }

    public class AwaitResult<T>
    {
        public AwaitResult()
        {
        }

        public AwaitResult(Task<T> task)
        {
            AsTask = task;
        }

        public Task<T> AsTask { get; }

        public static implicit operator T(AwaitResult<T> m)
        {
            return m.AsTask.Result;
        }
    }

    public static class TaskSpecificationExtensions
    {
        public static AwaitResult<T> Await<T>(this Task<T> task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerExceptions.First();
                }
                throw;
            }

            return new AwaitResult<T>(task);
        }

        public static AwaitResult Await(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerExceptions.First();
                }
                throw;
            }

            return new AwaitResult(task);
        }
    }

    public delegate void Establish();

    public delegate void Because();

    public delegate void It();

    public delegate void Cleanup();

    public static class Catch
    {
        public static Exception Exception(Action action)
        {
            return Throw<Exception>(action);
        }

        public static Exception Exception<T>(Func<T> func)
        {
            try
            {
                func();
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }

        public static T Throw<T>(Action throwingAction)
          where T : Exception
        {
            try
            {
                throwingAction();
            }
            catch (T exception)
            {
                return exception;
            }

            return null;
        }
    }
}