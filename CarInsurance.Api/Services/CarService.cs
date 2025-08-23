using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate == null || p.EndDate >= date)
        );
    }

    // Task B: Create Insurance Claim
    public async Task<InsuranceClaimResponse> CreateInsuranceClaimAsync(long carId, InsuranceClaimDto request)
    {
        var car = await _db.Cars.FindAsync(carId);
        if (car == null)
            throw new KeyNotFoundException($"Car with ID {carId} not found");

        var claim = new InsuranceClaim
        {
            CarId = carId,
            ClaimDate = request.ClaimDate,
            Description = request.Description,
            Amount = request.Amount
        };

        _db.InsuranceClaims.Add(claim);
        await _db.SaveChangesAsync();

        return new InsuranceClaimResponse(
            Id: claim.Id,
            CarId: claim.CarId,
            ClaimDate: claim.ClaimDate,
            Description: claim.Description,
            Amount: claim.Amount
        );
    }

    // Task B: Get Car History
    public async Task<CarHistoryResponse> GetCarHistoryAsync(long carId)
    {
        var car = await _db.Cars
            .Include(c => c.Policies)
            .Include(c => c.InsuranceClaims)
            .FirstOrDefaultAsync(c => c.Id == carId);

        if (car == null)
            throw new KeyNotFoundException($"Car with ID {carId} not found");

        var timeline = new List<HistoryItemDto>();

        // Add policies to timeline
        foreach (var policy in car.Policies)
        {
            timeline.Add(new HistoryItemDto(
                Type: "Policy",
                EventDate: policy.StartDate,
                PolicyStartDate: policy.StartDate,
                PolicyEndDate: policy.EndDate,
                PolicyProvider: policy.Provider,
                ClaimDescription: null,
                ClaimAmount: null
            ));
        }

        // Add claims to timeline
        foreach (var claim in car.InsuranceClaims)
        {
            timeline.Add(new HistoryItemDto(
                Type: "Claim",
                EventDate: claim.ClaimDate,
                PolicyStartDate: null,
                PolicyEndDate: null,
                PolicyProvider: null,
                ClaimDescription: claim.Description,
                ClaimAmount: claim.Amount
            ));
        }

        // Sort timeline by EventDate
        timeline = timeline.OrderBy(item => item.EventDate).ToList();

        return new CarHistoryResponse(
            car.Id,
            car.Vin,
            timeline
        );
    }
}
