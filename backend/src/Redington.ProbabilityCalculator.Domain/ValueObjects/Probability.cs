namespace Redington.ProbabilityCalculator.Domain.ValueObjects;

/// <summary>
/// Value object that centralises the probability invariant: a probability is a
/// decimal in the inclusive range [0, 1]. Once constructed, the domain can rely
/// on the value being valid.
/// </summary>
public readonly record struct Probability
{
    public decimal Value { get; }

    private Probability(decimal value)
    {
        Value = value;
    }

    public static Probability Create(decimal value)
    {
        if (value is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Probability must be between 0 and 1 inclusive.");
        }

        return new Probability(value);
    }

    public static bool IsValid(decimal value)
    {
        return value is >= 0m and <= 1m;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
