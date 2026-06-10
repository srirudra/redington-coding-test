using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Calculations;

/// <summary>
/// Strategy contract for a single probability calculation operation. Adding a new
/// operation means adding a new implementation, not modifying existing ones.
/// </summary>
public interface IProbabilityCalculation
{
    CalculationType Type { get; }

    decimal Calculate(Probability firstProbability, Probability secondProbability);
}
