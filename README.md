
# **[Warden](http://spetz.github.io/warden/)**

[![Build status](https://ci.appveyor.com/api/projects/status/47l3ldatuj526tf5/branch/master?svg=true)](https://ci.appveyor.com/project/spetz/Warden/branch/master)

> Define "health checks" for your applications, resources and
> infrastructure. Keep your **Warden** on the watch.

**Please note that it's just a preview version of the library (work in progress).**

**What is Warden?**
----------------

Warden is a simple, next-gen, (yet to be) cross-platform library built to solve the problem of monitoring the resources such as the websites, API, databases, CPU etc. It allows to quickly define the **[watchers](https://github.com/spetz/Warden/wiki/watcher)** responsible for performing checks on specific resources and **[integrations](https://github.com/spetz/Warden/wiki/integration)** to easily notify about any issues related to the possible downtime of your system. On top of that, you may use all of this information to collect the custom metrics thanks to the **[hooks](https://github.com/spetz/Warden/wiki/Hooks)**.


**What kind of monitoring is available?**
----------------
Currently available **[watchers](https://github.com/spetz/Warden/wiki/watcher)**:

 - **[MongoDB](https://github.com/spetz/Warden/wiki/Watcher-type-MongoDB)**
 - **[MSSQL](https://github.com/spetz/Warden/wiki/Watcher-type-MSSQL)**
 - **[Performance](https://github.com/spetz/Warden/wiki/Watcher-type-Performance)**
 - **[Redis](https://github.com/spetz/Warden/wiki/Watcher-type-Redis)**
 - **[Web](https://github.com/spetz/Warden/wiki/Watcher-type-Web)**

More watchers are coming soon (File, Network etc.)

**What are the integrations with external services?**
----------------
As for now, you may use the following **[integrations](https://github.com/spetz/Warden/wiki/integration)**:

 - **[SendGrid](https://github.com/spetz/Warden/wiki/Integration-with-SendGrid)**

More integrations are coming soon.

**Is there any documentation?**
----------------

Yes, please navigate to the **[wiki](https://github.com/spetz/Warden/wiki)** page where you can find detailed information about configuring and running the **Warden**.

**Installation**
----------------

Available as a **[NuGet package](https://www.nuget.org/packages/Warden/)**. 
```
Install-Package Warden -Pre
```

**Watchers** and **integrations** are available as a separate _NuGet packages_ listed **[here](https://www.nuget.org/profiles/Spetz)**.

**Cross-platform availability**
----------------

| Project              |   .NET 4.5.1  |  DotNet 5.4  |            Comment              |      
|----------------------|:-------------:|:------------:|---------------------------------
| **[Warden Core](https://github.com/spetz/Warden/wiki/Warden)**         |        +      |        +     |     
| **[Disk Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-Disk)**         |        +      |        -     | _System.IO not compatible_
| **[MongoDB Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-MongoDB)**      |        +      |        -     | _MongoDB Driver not compatible_
| **[MSSQL Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-MSSQL)**        |        +      |        -     | _Dapper not compatible_
| **[Performance Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-Performance)**  |        +      |        -     | _PerformanceCounter not compatible_
| **[Redis Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-Redis)**        |        +      |        -     | _StackExchange.Redis not compatible_
| **[Web Watcher](https://github.com/spetz/Warden/wiki/Watcher-type-Web)**          |        +      |        +     |
| **[SendGrid Integration](https://github.com/spetz/Warden/wiki/Integration-with-SendGrid)** |        +      |        -     | _SendGrid not compatible_


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
var warden = Warden.Create(configuration);
await warden.StartAsync();
```
Please check out the **[examples](https://github.com/spetz/Warden/wiki/Examples)** by cloning the repository.