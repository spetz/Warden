# Sentry

[![Build status](https://ci.appveyor.com/api/projects/status/47l3ldatuj526tf5/branch/master?svg=true)](https://ci.appveyor.com/project/spetz/sentry/branch/master)

> Define "health checks" for your applications, resources and
> infrastructure. Keep your **Sentry** on the watch.

**Quick start**:
----------------

Define the **[watchers](https://github.com/spetz/Sentry/wiki/Watchers)** that will monitor your resources. You may choose between the website, API, MSSQL and MongoDB **watchers** - many more are coming soon (Redis, CPU, Memory, File etc.)
```csharp
//Define a watcher for the website 
var myWebsiteUrl = "http://my-website.com";
var websiteWatcherConfiguration = WebsiteWatcherConfiguration
    .Create(myWebsiteUrl)
    .Build();
var websiteWatcher = WebsiteWatcher.Create("My website watcher", websiteWatcherConfiguration);

//Define a watcher for the API 
var myApiUrl = "http://my-api.com";
var apiWatcherConfiguration = ApiWatcherConfiguration
    .Create("http://my-api.com", HttpRequest.Post("users", new {name = "test"}))
    .WithHeaders(new Dictionary<string, string>
    {
        ["Authorization"] = "Token MyBase64EncodedString",
    })
    .EnsureThat(response => response.Headers.Location != null)
    .Build();
var apiWatcher = ApiWatcher.Create("My API watcher", apiWatcherConfiguration);

//Define a watcher for the MSSQL database 
var myConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=MyDatabase;Integrated Security=True";
var mssqlWatcherConfiguration = MsSqlWatcherConfiguration
    .Create(myConnectionString)
    .WithQuery("select * from users where id = @id", new Dictionary<string, object> {["id"] = 1})
    .EnsureThat(users => users.Any(user => user.Name == "admin"))
    .Build();
var mssqlWatcher = MsSqlWatcher.Create("My database watcher", mssqlWatcherConfiguration);
```

Configure the **[Sentry](https://github.com/spetz/Sentry/wiki/Sentry)** by adding the previously defined **watchers**, setting up the **[Hooks](https://github.com/spetz/Sentry/wiki/Hooks)** (callbacks) to get notified about failures, successful checks, exceptions etc. - use that information e.g. in order to let your system administrator know when something goes wrong or to build your custom metrics.
```csharp
var sentryConfiguration = SentryConfiguration
    .Create()
    .SetHooks(hooks =>
    {
        //Configure the Sentry hooks
        hooks.OnError(exception => Logger.Error(exception));
        hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration));
    })
    .AddWatcher(apiWatcher)
    .AddWatcher(mssqlWatcher)
    .AddWatcher(websiteWatcher, hooks =>
    {
        //Configure the hooks for this particular watcher
        hooks.OnStartAsync(check => WebsiteHookOnStartAsync(check));
        hooks.OnFailureAsync(result => WebsiteHookOnFailureAsync(result));
        hooks.OnSuccessAsync(result => WebsiteHookOnSuccessAsync(result));
        hooks.OnCompletedAsync(result => WebsiteHookOnCompletedAsync(result));
    })
    .SetGlobalWatcherHooks(hooks =>
    {
        //Configure the common hooks for all of the watchers
        hooks.OnStart(check => GlobalHookOnStart(check));
        hooks.OnFailure(result => GlobalHookOnFailure(result));
        hooks.OnSuccess(result => GlobalHookOnSuccess(result));
        hooks.OnCompleted(result => GlobalHookOnCompleted(result));
        hooks.OnError(exception => Logger.Error(exception));
    })
    .Build();
```

Start the **Sentry** and let him do his job - now **you have the full control** over your system monitoring!
```csharp
var sentry = Sentry.Create(sentryConfiguration);
await sentry.StartAsync();
```
Please check out the **[examples](https://github.com/spetz/Sentry/wiki/Examples)** by cloning the repository.