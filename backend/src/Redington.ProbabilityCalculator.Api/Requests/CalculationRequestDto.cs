using Redington.ProbabilityCalculator.Domain.Calculations;

namespace Redington.ProbabilityCalculator.Api.Requests;

/// <summary>
/// Untrusted external input. Probabilities are accepted as raw decimals so the API
/// can return field-specific validation errors before constructing value objects.
/// </summary>
public sealed record CalculationRequestDto(
    decimal FirstProbability,
    decimal SecondProbability,
    CalculationType CalculationType);
