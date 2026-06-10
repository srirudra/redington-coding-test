using Redington.ProbabilityCalculator.Application.Calculations;
using Redington.ProbabilityCalculator.Application.Logging;
using Redington.ProbabilityCalculator.Domain.Calculations;
using Redington.ProbabilityCalculator.Domain.ValueObjects;

namespace Redington.ProbabilityCalculator.Application.Tests;

[Trait("Category", "Unit")]
public sealed class CalculationServiceTests
{
    private sealed class RecordingAuditLogger : ICalculationAuditLogger
    {
        public CalculationResult? LastResult { get; private set; }

        public Task LogAsync(CalculationResult result, CancellationToken cancellationToken = default)
        {
            LastResult = result;
            return Task.CompletedTask;
        }
    }

    private static CalculationService CreateService(RecordingAuditLogger logger)
    {
        var strategies = new IProbabilityCalculation[]
        {
            new CombinedWithCalculation(),
            new EitherCalculation()
        };

        return new CalculationService(strategies, logger);
    }

    [Fact]
    public async Task CalculateAsync_Should_ReturnProduct_When_CalculationTypeIsCombinedWith()
    {
        var logger = new RecordingAuditLogger();
        var service = CreateService(logger);
        var command = new CalculationCommand(
            Probability.Create(0.5m),
            Probability.Create(0.5m),
            CalculationType.CombinedWith);

        var result = await service.CalculateAsync(command);

        Assert.Equal(0.25m, result.Result);
        Assert.Equal(CalculationType.CombinedWith, result.CalculationType);
    }

    [Fact]
    public async Task CalculateAsync_Should_ReturnEitherProbability_When_CalculationTypeIsEither()
    {
        var logger = new RecordingAuditLogger();
        var service = CreateService(logger);
        var command = new CalculationCommand(
            Probability.Create(0.5m),
            Probability.Create(0.5m),
            CalculationType.Either);

        var result = await service.CalculateAsync(command);

        Assert.Equal(0.75m, result.Result);
    }

    [Fact]
    public async Task CalculateAsync_Should_WriteAuditEntry_When_CalculationSucceeds()
    {
        var logger = new RecordingAuditLogger();
        var service = CreateService(logger);
        var command = new CalculationCommand(
            Probability.Create(0.2m),
            Probability.Create(0.3m),
            CalculationType.CombinedWith);

        var result = await service.CalculateAsync(command);

        Assert.NotNull(logger.LastResult);
        Assert.Equal(result, logger.LastResult);
    }

    [Fact]
    public async Task CalculateAsync_Should_Throw_When_CalculationTypeIsUnsupported()
    {
        var logger = new RecordingAuditLogger();
        var service = CreateService(logger);
        var command = new CalculationCommand(
            Probability.Create(0.5m),
            Probability.Create(0.5m),
            (CalculationType)999);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CalculateAsync(command));
    }
}
