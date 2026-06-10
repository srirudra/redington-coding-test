using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class ProbabilityTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("0.5")]
    [InlineData("1")]
    public void Create_Should_ReturnProbability_When_ValueIsValid(string input)
    {
        var value = decimal.Parse(input);

        var probability = Probability.Create(value);

        Assert.Equal(value, probability.Value);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("1.01")]
    public void Create_Should_Throw_When_ValueIsOutsideAllowedRange(string input)
    {
        var value = decimal.Parse(input);

        Assert.Throws<ArgumentOutOfRangeException>(() => Probability.Create(value));
    }

    [Fact]
    public void Create_Should_Throw_When_ValueIsDecimalMinValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Probability.Create(decimal.MinValue));
    }

    [Fact]
    public void Create_Should_Throw_When_ValueIsDecimalMaxValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Probability.Create(decimal.MaxValue));
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("1", true)]
    [InlineData("-0.01", false)]
    [InlineData("1.01", false)]
    public void IsValid_Should_ReflectInclusiveRange(string input, bool expected)
    {
        var value = decimal.Parse(input);

        Assert.Equal(expected, Probability.IsValid(value));
    }
}
