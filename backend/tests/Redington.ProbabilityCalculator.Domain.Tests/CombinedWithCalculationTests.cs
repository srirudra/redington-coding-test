using Redington.ProbabilityCalculator.Domain.Calculations;
using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class CombinedWithCalculationTests
{
    [Fact]
    public void Calculate_Should_ReturnProduct_When_BothProbabilitiesAreValid()
    {
        var calculation = new CombinedWithCalculation();

        var result = calculation.Calculate(Probability.Create(0.5m), Probability.Create(0.5m));

        Assert.Equal(0.25m, result);
    }

    [Fact]
    public void Type_Should_BeCombinedWith()
    {
        Assert.Equal(CalculationType.CombinedWith, new CombinedWithCalculation().Type);
    }
}
