using FluentValidation.TestHelper;
using Redington.ProbabilityCalculator.Api.Requests;
using Redington.ProbabilityCalculator.Api.Validators;
using Redington.ProbabilityCalculator.Domain.Calculations;

namespace Redington.ProbabilityCalculator.Api.Tests;

[Trait("Category", "Unit")]
public sealed class CalculationRequestDtoValidatorTests
{
    private readonly CalculationRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_RequestIsValid()
    {
        var request = new CalculationRequestDto(0.5m, 0.25m, CalculationType.CombinedWith);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_ReturnErrorForFirstProbability_When_FirstProbabilityIsInvalid()
    {
        var request = new CalculationRequestDto(-0.5m, 0.5m, CalculationType.CombinedWith);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstProbability);
        result.ShouldNotHaveValidationErrorFor(x => x.SecondProbability);
        result.ShouldNotHaveValidationErrorFor(x => x.CalculationType);
    }

    [Fact]
    public void Validate_Should_ReturnErrorForSecondProbability_When_SecondProbabilityIsInvalid()
    {
        var request = new CalculationRequestDto(0.5m, 1.5m, CalculationType.CombinedWith);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SecondProbability);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstProbability);
        result.ShouldNotHaveValidationErrorFor(x => x.CalculationType);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_CalculationTypeIsUndefined()
    {
        var request = new CalculationRequestDto(0.5m, 0.5m, (CalculationType)999);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CalculationType);
    }
}
