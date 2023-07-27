using Microsoft.Extensions.Logging;
using Quartz;

public class TestJob : IJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestJob> _logger;

    public TestJob(ApplicationDbContext context, ILogger<TestJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running job");
        for (int j = 0; j < 1_000_000; j++)
            _context.People.Add(new Person { Id = j });
        return Task.CompletedTask;
    }
}
