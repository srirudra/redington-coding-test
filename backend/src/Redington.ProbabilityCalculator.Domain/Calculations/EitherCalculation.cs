using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Calculations;

/// <summary>
/// Either: P(A) + P(B) - P(A) * P(B).
/// </summary>
public sealed class EitherCalculation : IProbabilityCalculation
{
    public CalculationType Type => CalculationType.Either;

    public decimal Calculate(Probability firstProbability, Probability secondProbability)
    {
        return firstProbability.Value
            + secondProbability.Value
            - firstProbability.Value * secondProbability.Value;
    }
}
