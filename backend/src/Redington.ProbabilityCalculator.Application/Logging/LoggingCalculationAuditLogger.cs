using Microsoft.Extensions.Logging;
using Redington.ProbabilityCalculator.Application.Calculations;

namespace Redington.ProbabilityCalculator.Application.Logging;

/// <summary>
/// Writes calculation audit records as structured log events. The configured
/// logging provider (Serilog) routes them to its sinks (console and rolling file).
/// </summary>
public sealed class LoggingCalculationAuditLogger : ICalculationAuditLogger
{
    private readonly ILogger<LoggingCalculationAuditLogger> _logger;

    public LoggingCalculationAuditLogger(ILogger<LoggingCalculationAuditLogger> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(CalculationResult result, CancellationToken cancellationToken = default)
    {
        // Tag the event so the logging pipeline can route audit records to a dedicated sink.
        using (_logger.BeginScope(new Dictionary<string, object> { ["EventType"] = "CalculationAudit" }))
        {
            _logger.LogInformation(
                "Calculation audit: {CalculationType} A={FirstProbability} B={SecondProbability} Result={Result} At={CalculatedAtUtc:o}",
                result.CalculationType,
                result.FirstProbability,
                result.SecondProbability,
                result.Result,
                result.CalculatedAtUtc);
        }

        return Task.CompletedTask;
    }
}
