using CarInsurance.Api.Data;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarInsurance.Api.Tests;

// Task C: Unit tests
public class InsuranceValidityTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly CarService _carService;

    public InsuranceValidityTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _dbContext = new AppDbContext(options);
        _carService = new CarService(_dbContext);
        SeedData.EnsureSeeded(_dbContext);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    [Fact]
    public async Task NonExistentCarId_ReturnsNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _carService.IsInsuranceValidAsync(9999, new DateOnly(2024, 6, 1)));
    }

    [Fact]
    public async Task DateBefore1900_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _carService.IsInsuranceValidAsync(1, new DateOnly(1899, 12, 31)));
        Assert.Contains("1900", exception.Message);
    }

    [Fact]
    public async Task DateTooFarInFuture_ThrowsArgumentException()
    {
        var farFutureDate = DateOnly.FromDateTime(DateTime.Today).AddYears(11);
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _carService.IsInsuranceValidAsync(1, farFutureDate));
        Assert.Contains("10 years", exception.Message);
    }

    [Fact]
    public async Task ValidDateWithinPolicy_ReturnsTrue()
    {
        var result = await _carService.IsInsuranceValidAsync(1, new DateOnly(2024, 6, 1));
        Assert.True(result);
    }
    
    [Fact]
    public async Task ValidDateOutsidePolicy_ReturnsFalse()
    {
        var result = await _carService.IsInsuranceValidAsync(1, new DateOnly(2023, 12, 31));
        Assert.False(result);
    }
}
