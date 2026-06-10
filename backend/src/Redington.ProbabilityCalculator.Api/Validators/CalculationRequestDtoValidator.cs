using FluentValidation;
using Redington.ProbabilityCalculator.Api.Requests;
using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Api.Validators;

/// <summary>
/// Boundary validation: produces user-friendly, field-specific errors. Reuses the
/// domain invariant via <see cref="Probability.IsValid"/> to avoid duplicating the rule.
/// </summary>
public sealed class CalculationRequestDtoValidator : AbstractValidator<CalculationRequestDto>
{
    public CalculationRequestDtoValidator()
    {
        RuleFor(x => x.FirstProbability)
            .Must(Probability.IsValid)
            .WithMessage("FirstProbability must be between 0 and 1 inclusive.");

        RuleFor(x => x.SecondProbability)
            .Must(Probability.IsValid)
            .WithMessage("SecondProbability must be between 0 and 1 inclusive.");

        RuleFor(x => x.CalculationType)
            .IsInEnum()
            .WithMessage("CalculationType is not supported.");
    }
}
