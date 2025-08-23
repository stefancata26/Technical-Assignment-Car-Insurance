namespace CarInsurance.Api.Models;

// Task B: New model for insurance claims
public class InsuranceClaim
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public DateOnly ClaimDate { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Car Car { get; set; } = null!;
}
