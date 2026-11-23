namespace EmpleadaHogar.Models;

public sealed class EmployerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? SocialSecurityEmployerCode { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<EmployeeProfile> Employees { get; set; } = new List<EmployeeProfile>();
}
