using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sentry.Tests
{
    [Specification]
    public abstract class SpecificationBase
    {
        public Exception ExceptionThrown;
        public bool ExceptionExpected;

        protected virtual async Task EstablishContext()
        {
            ExceptionExpected = false;
        }

        protected virtual async Task BecauseOf()
        {
        }

        protected virtual async Task CleanUp()
        {
        }

        [OneTimeSetUp]
        public async Task TestInitialize()
        {
            await Task.WhenAll(EstablishContext());

            try
            {
                await BecauseOf();
            }
            catch (Exception exception)
            {
                if (ExceptionExpected == false)
                    throw;

                ExceptionThrown = exception;
            }
        }

        [OneTimeTearDown]
        public async Task TestCleanup()
        {
            await CleanUp();
        }
    }

    public class SpecificationAttribute : TestFixtureAttribute
    {
    }

    public class ThenAttribute : TestAttribute
    {
    }
}