using Redington.ProbabilityCalculator.Application.Calculations;
using Redington.ProbabilityCalculator.Application.Logging;
using Redington.ProbabilityCalculator.Domain.Calculations;

namespace Redington.ProbabilityCalculator.Application.Calculations;

/// <summary>
/// Selects the calculation strategy matching the requested type, executes it, and
/// writes an audit record. New operations are picked up automatically via DI.
/// </summary>
public sealed class CalculationService : ICalculationService
{
    private readonly IReadOnlyDictionary<CalculationType, IProbabilityCalculation> _calculations;
    private readonly ICalculationAuditLogger _auditLogger;

    public CalculationService(
        IEnumerable<IProbabilityCalculation> calculations,
        ICalculationAuditLogger auditLogger)
    {
        _calculations = calculations.ToDictionary(calculation => calculation.Type);
        _auditLogger = auditLogger;
    }

    public async Task<CalculationResult> CalculateAsync(
        CalculationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_calculations.TryGetValue(command.CalculationType, out var calculation))
        {
            throw new ArgumentException(
                $"Unsupported calculation type: {command.CalculationType}",
                nameof(command));
        }

        var value = calculation.Calculate(command.FirstProbability, command.SecondProbability);

        var result = new CalculationResult(
            command.FirstProbability.Value,
            command.SecondProbability.Value,
            command.CalculationType,
            value,
            DateTime.UtcNow);

        await _auditLogger.LogAsync(result, cancellationToken);

        return result;
    }
}
