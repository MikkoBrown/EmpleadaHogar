namespace EmpleadaHogar.Models;

/// <summary>Clasificación del parámetro. Los valores legales se introducen y mantienen desde la aplicación.</summary>
public enum EconomicParameterCategory
{
    Salary = 1,
    SocialSecurity = 2,
    Tax = 3,
    Bonus = 4,
    WorkingTime = 5,
    Other = 99
}

public enum EconomicParameterValueType
{
    Amount = 1,
    Percentage = 2,
    Number = 3,
    Text = 4,
    Boolean = 5
}

/// <summary>
/// Valor de un parámetro económico o laboral durante un periodo de vigencia.
/// Cada usuario mantiene su propia parametrización. No se incluyen porcentajes legales precargados.
/// </summary>
public sealed class EconomicParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public EconomicParameterCategory Category { get; set; }
    public EconomicParameterValueType ValueType { get; set; }
    public decimal? NumericValue { get; set; }
    public string? TextValue { get; set; }
    public string? Unit { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
