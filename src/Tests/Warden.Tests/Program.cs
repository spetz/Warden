using System;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NUnitLite;

namespace Warden.Tests
{
    //TODO: Remove when NUnit works with DNX Test runner
    public class Program
    {
        public int Main(string[] args)
        {
#if DNX451
            return new AutoRun().Execute(args);
#else
            return new AutoRun().Execute(typeof(Program).GetTypeInfo().Assembly, Console.Out, Console.In, args);
#endif
        }
    }
}
