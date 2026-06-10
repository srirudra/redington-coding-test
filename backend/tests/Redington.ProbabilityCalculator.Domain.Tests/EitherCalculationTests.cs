using Redington.ProbabilityCalculator.Domain.Calculations;
using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class EitherCalculationTests
{
    [Fact]
    public void Calculate_Should_ReturnEitherProbability_When_BothProbabilitiesAreValid()
    {
        var calculation = new EitherCalculation();

        var result = calculation.Calculate(Probability.Create(0.5m), Probability.Create(0.5m));

        Assert.Equal(0.75m, result);
    }

    [Fact]
    public void Type_Should_BeEither()
    {
        Assert.Equal(CalculationType.Either, new EitherCalculation().Type);
    }
}
