using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Calculations;

/// <summary>
/// CombinedWith: P(A) * P(B).
/// </summary>
public sealed class CombinedWithCalculation : IProbabilityCalculation
{
    public CalculationType Type => CalculationType.CombinedWith;

    public decimal Calculate(Probability firstProbability, Probability secondProbability)
    {
        return firstProbability.Value * secondProbability.Value;
    }
}
