using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarInsurance.Api.Services;

public class PolicyExpirationService : BackgroundService
{
    private readonly ILogger<PolicyExpirationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<long> _processedPolicyIds = new();

    public PolicyExpirationService(
        ILogger<PolicyExpirationService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Policy Expiration Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiredPoliciesAsync();
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Policy Expiration Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    public async Task CheckExpiredPoliciesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);

        // Convert DateOnly to DateTime for comparison
        var nowDateOnly = DateOnly.FromDateTime(now);
        var oneHourAgoDateOnly = DateOnly.FromDateTime(oneHourAgo);

        // Find policies that expired in the last hour
        var recentlyExpiredPolicies = await dbContext.Policies
            .Where(p => p.EndDate <= nowDateOnly && 
                       p.EndDate > oneHourAgoDateOnly)
            .ToListAsync();

        foreach (var policy in recentlyExpiredPolicies)
        {
            // Skip if we've already processed this policy
            if (_processedPolicyIds.Contains(policy.Id))
                continue;

            // Log the expiration message
            _logger.LogWarning("Policy {PolicyId} for Car {CarId} expired on {ExpirationDate}",
                policy.Id, policy.CarId, policy.EndDate);

            // Mark as processed to avoid duplicates
            _processedPolicyIds.Add(policy.Id);
        }

        // Clean up old processed IDs (optional, prevents memory growth over time)
        if (_processedPolicyIds.Count > 1000)
        {
            _processedPolicyIds.Clear();
        }
    }
}