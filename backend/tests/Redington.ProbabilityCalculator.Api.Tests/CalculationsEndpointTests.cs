using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Redington.ProbabilityCalculator.Domain.Calculations;

namespace Redington.ProbabilityCalculator.Api.Tests;

[Trait("Category", "Integration")]
public sealed class CalculationsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public CalculationsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private sealed record CalculationResponse(
        decimal FirstProbability,
        decimal SecondProbability,
        CalculationType CalculationType,
        decimal Result,
        DateTime CalculatedAtUtc);

    [Fact]
    public async Task Post_Should_Return200AndResult_When_RequestIsValid()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            firstProbability = 0.5m,
            secondProbability = 0.5m,
            calculationType = "CombinedWith"
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CalculationResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(0.25m, body!.Result);
        Assert.Equal(CalculationType.CombinedWith, body.CalculationType);
    }

    [Theory]
    [InlineData(-0.1, 0.5, "CombinedWith")]
    [InlineData(0.5, 1.5, "Either")]
    public async Task Post_Should_Return400_When_ProbabilityIsOutOfRange(
        decimal first, decimal second, string type)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            firstProbability = first,
            secondProbability = second,
            calculationType = type
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Should_Return400_When_CalculationTypeIsUnknown()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/calculations", new
        {
            firstProbability = 0.5m,
            secondProbability = 0.5m,
            calculationType = "NotARealType"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Should_AcceptScientificNotation_When_BodyUsesExponentialNumber()
    {
        var client = _factory.CreateClient();

        // 1.8e-2 == 0.018; JSON numbers in exponential form are accepted.
        using var content = new StringContent(
            """{"firstProbability":1.8e-2,"secondProbability":0.5,"calculationType":"CombinedWith"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/calculations", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CalculationResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(0.009m, body!.Result);
    }
}
