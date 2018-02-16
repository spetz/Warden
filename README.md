![Warden](http://spetz.github.io/img/warden_logo.png)

#### **OPEN SOURCE & CROSS-PLATFORM TOOL FOR SIMPLIFIED MONITORING**
#### **[getwarden.net](http://getwarden.net)**

[![Build status](https://ci.appveyor.com/api/projects/status/47l3ldatuj526tf5/branch/master?svg=true)](https://ci.appveyor.com/project/spetz/Warden/branch/master)

> Define "health checks" for your applications, resources and
> infrastructure. Keep your **Warden** on the watch.


**What is Warden?**
----------------

**Warden** is a simple, **cross-platform** library, built to **solve the problem of monitoring the resources** such as the websites, API, databases, CPU etc. 

It allows to quickly define the **[watchers](https://github.com/spetz/Warden/wiki/watcher)** responsible for performing checks on specific resources and **[integrations](https://github.com/spetz/Warden/wiki/integration)** to easily notify about any issues related to the possible downtime of your system. 

Each **[watcher](https://github.com/spetz/Warden/wiki/watcher)** might have its own custom **[interval](https://github.com/warden-stack/Warden/wiki/Interval)**. For example you may want to check e.g. the *API* availability every 500 milliseconds, while the *database* should respond only every 10 seconds and so on. Or you could just set the common one **[interval](https://github.com/warden-stack/Warden/wiki/Interval)** (5 seconds by default) for all of the **[watchers](https://github.com/spetz/Warden/wiki/watcher)**.

On top of that, you may use all of this information to collect the custom metrics thanks to the **[hooks](https://github.com/spetz/Warden/wiki/Hooks)**.


**Roadmap**
----------------

- [x] Migrate fully to the .NET Core (NET Standard).
- [x] Move all of the extensions (Wardens and Integrations) into the separate repositiories.
- [ ] Apply new features.

**What kind of monitoring is available?**
----------------
 - **[Disk](https://github.com/warden-stack/Warden.Watchers.Disk)**
 - **[MongoDB](https://github.com/warden-stack/Warden.Watchers.MongoDB)**
 - **[MSSQL](https://github.com/warden-stack/Warden.Watchers.MSSQL)**
 - **[Performance](https://github.com/warden-stack/Warden.Watchers.Performance)**
 - **[Process](https://github.com/warden-stack/Warden.Watchers.Process)**
 - **[Redis](https://github.com/warden-stack/Warden.Watchers.Redis)**
 - **[Server](https://github.com/warden-stack/Warden.Watchers.Server)**
 - **[Web](https://github.com/warden-stack/Warden.Watchers.Web)**
 - **[SSL certificates (3rd party)](https://github.com/janpieterz/Warden.Watchers.SSL)**
 - **[Azure Storage (3rd party)](https://github.com/janpieterz/Warden.Watchers.AzureStorage)**
 - **[Azure Service Bus (3rd party)](https://github.com/janpieterz/Warden.Watchers.AzureServiceBus)**

**What are the integrations with external services?**
----------------
 - **[Cachet](https://github.com/warden-stack/Warden.Integrations.Cachet)**
 - **[HTTP API](https://github.com/warden-stack/Warden.Integrations.HTTP-API)**
 - **[MS SQL](https://github.com/warden-stack/Warden.Integrations.MSSQL)**
 - **[SendGrid](https://github.com/warden-stack/Warden.Integrations.SendGrid)**
 - **[Slack](https://github.com/warden-stack/Warden.Integrations.Slack)**
 - **[SMTP](https://github.com/warden-stack/Warden.Integrations.Smtp)**
 - **[Twilio](https://github.com/warden-stack/Warden.Integrations.Twilio)**
 - **[Seq (3rd party)](https://github.com/janpieterz/Warden.Integrations.Seq)**

**How can I see what's happening with my system?**
----------------

You can make use of the **[Web Panel](https://github.com/spetz/Warden/wiki/Web-Panel)** which provides the UI for organizing your monitoring workspace, dashboard with real-time statistics and the storage of the historical data. If you don't want to host it on your own, there's an online version available, running in the Azure cloud at **[panel.getwarden.net](http://panel.getwarden.net)** 

**Is there any documentation?**
----------------

Yes, please navigate to the **[wiki](https://github.com/spetz/Warden/wiki)** page where you can find detailed information about configuring and running the **Warden**.

**Installation**
----------------

Available as a **[NuGet package](https://www.nuget.org/packages/Warden/)**. 
```
dotnet add package Warden
```

**Watchers** and **Integrations** are available as a separate _NuGet packages_ listed **[here](https://www.nuget.org/packages?q=warden)**.

**Quick start**:
----------------

Configure the **[Warden](https://github.com/spetz/Warden/wiki/Warden)** by adding the  **[watchers](https://github.com/spetz/Warden/wiki/Watcher)** and setting up the **[hooks](https://github.com/spetz/Warden/wiki/Hooks)** and **[integrations](https://github.com/spetz/Warden/wiki/Integration)**  to get notified about failures, successful checks, exceptions etc. - use that information e.g. in order to let your system administrator know when something goes wrong or to build your custom metrics.
```csharp
var configuration = WardenConfiguration
    .Create()
    .AddWebWatcher("http://my-website.com")
    .AddMongoDbWatcher("mongodb://localhost:27017", "MyDatabase", cfg =>
    {
        cfg.WithQuery("Users", "{\"name\": \"admin\"}")
           .EnsureThat(users => users.Any(user => user.role == "admin"));
    })
    .IntegrateWithSendGrid("api-key", "noreply@system.com", cfg =>
    {
        cfg.WithDefaultSubject("Monitoring status")
           .WithDefaultReceivers("admin@system.com");
    })
    .SetGlobalWatcherHooks(hooks =>
    {
        hooks.OnStart(check => GlobalHookOnStart(check))
             .OnCompleted(result => GlobalHookOnCompleted(result));
    })
    .SetAggregatedWatcherHooks((hooks, integrations) =>
    {
        hooks.OnFirstFailureAsync(results => 
                integrations.SendGrid().SendEmailAsync("Monitoring errors have occured."))
             .OnFirstSuccessAsync(results => 
                integrations.SendGrid().SendEmailAsync("Everything is up and running again!"));
    })
    .SetHooks(hooks =>
    {
        hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration))
             .OnError(exception => Logger.Error(exception));
    })
    .Build();
```

Start the **Warden** and let him do his job - now **you have the full control** over your system monitoring!
```csharp
var warden = WardenInstance.Create(configuration);
await warden.StartAsync();
```
Please check out the **[examples](https://github.com/spetz/Warden/wiki/Examples)** by cloning the [repository](https://github.com/warden-stack/Warden.Examples).


**Contribute**
----------------

Want to contribute? Please check the **[contribution guidelines](https://github.com/warden-stack/Warden/blob/master/CONTRIBUTING.md)**. 