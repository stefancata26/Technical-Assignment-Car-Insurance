namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record InsuranceClaimDto(DateOnly ClaimDate, string? Description, decimal Amount); // Task B: input DTO for insurance claim
public record InsuranceClaimResponse(long Id, long CarId, DateOnly ClaimDate, string? Description, decimal Amount); // Task B: output DTO for insurance claim
public record HistoryItemDto(
    string Type,
    DateOnly EventDate,

    // Policy details
    DateOnly? PolicyStartDate,
    DateOnly? PolicyEndDate,
    string? PolicyProvider,

    // Claim details
    string? ClaimDescription,
    decimal? ClaimAmount
); // Task B: input DTO for history items
public record CarHistoryResponse(long CarId, string Vin, List<HistoryItemDto> Timeline); // Task B: output DTO for history items
