using System.Linq;
using Machine.Specifications;
using Warden.Configurations;
using Warden.Core;
using Warden.Watchers.Web;
using It = Machine.Specifications.It;

namespace Warden.Tests.EndToEnd.Core
{
    public class Warden_specs
    {
        protected static IWarden Warden { get; set; }
        protected static WardenConfiguration WardenConfiguration { get; set; }
        protected static WebWatcher WebWatcher { get; set; }
        protected static WebWatcherConfiguration WatcherConfiguration { get; set; }
        protected static IWardenIteration WardenIteration { get; set; } 
    }

    [Subject("Warden exeuction with watcher")]
    public class when_running_one_iteration_without_any_hooks_setup : Warden_specs
    {
        Establish context = () =>
        {
            WatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/200")
                .Build();
            WebWatcher = WebWatcher.Create("Valid web watcher",WatcherConfiguration);
            WardenConfiguration = WardenConfiguration
                .Create()
                .AddWatcher(WebWatcher)
                .RunOnlyOnce()
                .Build();
            Warden = new Warden(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_be_just_fine = () => true.ShouldBeTrue(); // :)
    }

    [Subject("Warden exeuction with invalid watcher")]
    public class when_running_one_iteration_with_on_completed_hooks_setup : Warden_specs
    {
        Establish context = () =>
        {
            WatcherConfiguration = WebWatcherConfiguration
                .Create("http://httpstat.us/400")
                .Build();
            WebWatcher = WebWatcher.Create("Invalid web watcher", WatcherConfiguration);
            WardenConfiguration = WardenConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnIterationCompleted(iteration => UpdateWardenIteration(iteration));
                })
                .AddWatcher(WebWatcher)
                .RunOnlyOnce()
                .Build();
            Warden = new Warden(WardenConfiguration);
        };

        Because of = async () => await Warden.StartAsync().Await().AsTask;

        It should_return_the_iteration_with_invalid_results = () => WardenIteration.Results.All(x => !x.IsValid).ShouldBeTrue();

        private static void UpdateWardenIteration(IWardenIteration wardenIteration)
        {
            WardenIteration = wardenIteration;
        }
    }
}