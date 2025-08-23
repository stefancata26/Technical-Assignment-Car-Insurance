namespace CarInsurance.Api.Models;

public class InsurancePolicy
{
    public long Id { get; set; }

    public long CarId { get; set; }
    public Car Car { get; set; } = default!;

    public string? Provider { get; set; }
    public DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; } // Task A: required at model level
}
