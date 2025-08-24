using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarInsurance.Api.Tests;

public class PolicyExpirationTests
{
    [Fact]
    public async Task CheckExpiredPoliciesAsync_WithRecentExpiration_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PolicyExpirationService>>();
        
        // Create in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestExpirationDb")
            .Options;
        
        using var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        // Add a policy that expired recently
        var expiredPolicy = new InsurancePolicy
        {
            Id = 1,
            CarId = 1,
            Provider = "Test Insurance",
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Today) // Expired today
        };
        dbContext.Policies.Add(expiredPolicy);
        await dbContext.SaveChangesAsync();

        // Create service with null dependencies for the method we're testing
        var service = new PolicyExpirationService(loggerMock.Object, null);

        // Use reflection to test the private method
        var method = typeof(PolicyExpirationService).GetMethod("CheckExpiredPoliciesAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Create a simple service scope that returns our dbContext
        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(AppDbContext)))
                    .Returns(dbContext);
        
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope())
                            .Returns(serviceScopeMock.Object);
    }
}