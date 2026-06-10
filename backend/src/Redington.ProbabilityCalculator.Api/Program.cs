using System.Text.Json.Serialization;
using FluentValidation;
using Redington.ProbabilityCalculator.Api.Middleware;
using Redington.ProbabilityCalculator.Api.Requests;
using Redington.ProbabilityCalculator.Api.Validators;
using Redington.ProbabilityCalculator.Application.Calculations;
using Redington.ProbabilityCalculator.Application.Logging;
using Redington.ProbabilityCalculator.Domain.Calculations;
using Redington.ProbabilityCalculator.Domain.ValueObjects;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured operational logging (console + rolling file) configured via appsettings "Serilog" section.
builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

// Serialize/accept the calculation type enum as a string ("CombinedWith"/"Either").
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Domain calculation strategies — add a new registration to support a new operation.
builder.Services.AddSingleton<IProbabilityCalculation, CombinedWithCalculation>();
builder.Services.AddSingleton<IProbabilityCalculation, EitherCalculation>();

// Application services.
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddSingleton<ICalculationAuditLogger, LoggingCalculationAuditLogger>();

// Boundary validation.
builder.Services.AddScoped<IValidator<CalculationRequestDto>, CalculationRequestDtoValidator>();

// Liveness/readiness probe for load balancers and orchestrators.
builder.Services.AddHealthChecks();

// Allow the React dev frontend to call the API during local development.
const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Emit a concise one-line summary per HTTP request.
app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(FrontendCorsPolicy);

app.MapHealthChecks("/health");

app.MapPost("/api/calculations", async (
    CalculationRequestDto request,
    IValidator<CalculationRequestDto> validator,
    ICalculationService calculationService,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    var validation = await validator.ValidateAsync(request, cancellationToken);
    if (!validation.IsValid)
    {
        var logger = loggerFactory.CreateLogger("Redington.ProbabilityCalculator.Api.Calculations");
        logger.LogInformation(
            "Calculation request rejected by validation. Invalid fields: {InvalidFields}",
            string.Join(", ", validation.Errors.Select(error => error.PropertyName).Distinct()));

        return Results.ValidationProblem(validation.ToDictionary());
    }

    var command = new CalculationCommand(
        Probability.Create(request.FirstProbability),
        Probability.Create(request.SecondProbability),
        request.CalculationType);

    var result = await calculationService.CalculateAsync(command, cancellationToken);

    return Results.Ok(result);
});

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program
{
}
