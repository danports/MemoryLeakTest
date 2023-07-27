using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

[DisallowConcurrentExecution]
public class TestJob : IJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestJob> _logger;
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

    public TestJob(ApplicationDbContext context, ILogger<TestJob> logger, DbContextOptions<ApplicationDbContext> contextOptions)
    {
        _context = context;
        _logger = logger;
        _contextOptions = contextOptions;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var people = await _context.People.ToListAsync();
        _logger.LogInformation("Updating people: {Count}", people.Count);
        foreach (var person in people)
        {
            person.Name = "John " + Random.Shared.Next(1_000_000);
            person.Version = Guid.NewGuid();
        }

        // Cause a concurrency exception:
        using var tempContext = new ApplicationDbContext(_contextOptions);
        var conflict = await tempContext.People.FindAsync(people[0].Id);
        _logger.LogInformation("Creating conflict for: {Id}", conflict.Id);
        conflict.Name = "Something Else";
        conflict.Version = Guid.NewGuid();
        await tempContext.SaveChangesAsync();

        // Boom!
        await _context.SaveChangesAsync();
    }
}
