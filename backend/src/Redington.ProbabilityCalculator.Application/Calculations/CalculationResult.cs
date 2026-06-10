using Redington.ProbabilityCalculator.Domain.Calculations;

namespace Redington.ProbabilityCalculator.Application.Calculations;

/// <summary>
/// Result of a probability calculation, including the inputs and a UTC timestamp.
/// </summary>
public sealed record CalculationResult(
    decimal FirstProbability,
    decimal SecondProbability,
    CalculationType CalculationType,
    decimal Result,
    DateTime CalculatedAtUtc);
