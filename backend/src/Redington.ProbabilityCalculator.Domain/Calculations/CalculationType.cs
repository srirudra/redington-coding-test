namespace Redington.ProbabilityCalculator.Domain.Calculations;

/// <summary>
/// Supported probability calculation operations. Explicit values keep the enum
/// stable when serialized across the API boundary.
/// </summary>
public enum CalculationType
{
    CombinedWith = 1,
    Either = 2
}
