using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Tests;

/// <summary>
/// Integration tests using WebApplicationFactory.
/// Uses an isolated InMemory database so no SQL Server is required.
/// </summary>
public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with InMemory for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var dbName = "IntegrationTestDb_" + Guid.NewGuid();
                services.AddDbContext<AppDbContext>(opts =>
                    opts.UseInMemoryDatabase(dbName)
                        .ConfigureWarnings(w => w.Ignore(
                            Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));
            });
        });
    }

    // ── Test: Unauthenticated request → 401 ───────────────────────────────────

    [Fact]
    public async Task UnauthenticatedRequest_To_ProtectedEndpoint_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Test: Wrong role (SalesEmployee) cannot access Admin-only endpoint → 403

    [Fact]
    public async Task SalesEmployee_AccessingAdminEndpoint_Returns403()
    {
        var client = _factory.CreateClient();

        // Register + login as SalesEmployee
        var token = await GetTokenAsync(client, "sales@int-test.com", "Sales#Test1", "SalesEmployee");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static async Task<string> GetTokenAsync(
        HttpClient client, string email, string password, string role)
    {
        // Register
        var registerBody = JsonSerializer.Serialize(new
        {
            fullName = "Test User",
            email,
            password,
            role
        });
        var registerResp = await client.PostAsync("/api/auth/register",
            new StringContent(registerBody, Encoding.UTF8, "application/json"));

        if (!registerResp.IsSuccessStatusCode)
        {
            var err = await registerResp.Content.ReadAsStringAsync();
            if (!err.Contains("already registered"))
                throw new Exception($"Register failed: {registerResp.StatusCode} {err}");
        }

        // Login
        var loginBody = JsonSerializer.Serialize(new { email, password });
        var loginResp = await client.PostAsync("/api/auth/login",
            new StringContent(loginBody, Encoding.UTF8, "application/json"));

        if (!loginResp.IsSuccessStatusCode)
        {
            var err = await loginResp.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {loginResp.StatusCode} {err}");
        }

        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(loginJson);
        return doc.RootElement.GetProperty("token").GetString()!;
    }
}
