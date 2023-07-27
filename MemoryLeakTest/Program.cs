using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var host = new HostBuilder()
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
        logging.AddSentry(s =>
        {
            s.Debug = true;
            // TODO: Configure a valid DSN.
            s.Dsn = "https://some-dsn";
            s.MinimumEventLevel = LogLevel.Error;
        });
        logging.AddFilter("Microsoft", LogLevel.Warning);
    })
    .ConfigureServices((context, services) =>
    {
        // TODO: Configure a valid MySQL connection string.
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

Console.WriteLine("Creating database...");
using var context = host.Services.GetRequiredService<ApplicationDbContext>();
await context.Database.EnsureCreatedAsync();
for (int i = 0; i < 1_000_000; i++)
    context.People.Add(new Person { Name = "Initial " + i, Version = Guid.NewGuid() });
await context.SaveChangesAsync();

Console.WriteLine("Starting scheduler...");
var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.Start();
await host.RunAsync();