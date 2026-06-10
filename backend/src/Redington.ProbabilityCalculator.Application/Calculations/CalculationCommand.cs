using Redington.ProbabilityCalculator.Domain.Calculations;
using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Application.Calculations;

/// <summary>
/// Application-level command built from validated input. Probabilities are
/// represented as value objects, so the service does not re-check the range.
/// </summary>
public sealed record CalculationCommand(
    Probability FirstProbability,
    Probability SecondProbability,
    CalculationType CalculationType);
