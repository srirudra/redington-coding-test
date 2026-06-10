namespace Redington.ProbabilityCalculator.Application.Calculations;

public interface ICalculationService
{
    Task<CalculationResult> CalculateAsync(
        CalculationCommand command,
        CancellationToken cancellationToken = default);
}
