using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var host = new HostBuilder()
    .ConfigureLogging((context, logging) => logging.AddConsole())
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseMySql("server=localhost;userid=root;pwd=whatever;database=testing", new MySqlServerVersion("8.0")));
        services.AddQuartz(q =>
        {
            q.SchedulerName = "Test-Scheduler";
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UseDefaultThreadPool(p => p.MaxConcurrency = 32);
            q.ScheduleJob<TestJob>(trigger => trigger
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()));
        });
    })
    .UseConsoleLifetime()
    .Build();

var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.Start();
await host.RunAsync();