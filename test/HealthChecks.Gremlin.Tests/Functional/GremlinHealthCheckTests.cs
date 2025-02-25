using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.Gremlin.Tests.Functional
{
    public class gremlin_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_gremlin_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: new string[] { "gremlin" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_multiple_gremlin_are_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: new string[] { "gremlin" }, name: "1")
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "localhost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: new string[] { "gremlin" }, name: "2");
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_gremlin_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                     .AddGremlin(_ => new GremlinOptions
                     {
                         Hostname = "wronghost",
                         Port = 8182,
                         EnableSsl = false
                     }, tags: new string[] { "gremlin" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("gremlin")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
