namespace Redington.ProbabilityCalculator.Application.Logging;

using Redington.ProbabilityCalculator.Application.Calculations;

public interface ICalculationAuditLogger
{
    Task LogAsync(CalculationResult result, CancellationToken cancellationToken = default);
}
