using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace HealthChecks.Network.Tests.Functional
{
    public class ftp_healthcheck_should
    {

        [Fact]
        public async Task be_healthy_when_connection_is_successful()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddFtpHealthCheck(setup =>
                        {
                            setup.AddHost("ftp://localhost:21",
                                createFile: false,
                                credentials: new NetworkCredential("bob", "12345"));
                        }, tags: new string[] { "ftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);
        }

        [Fact]
        public async Task be_healthy_when_connection_is_successful_and_file_is_created()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddFtpHealthCheck(setup =>
                        {
                            setup.AddHost("ftp://localhost:21",
                                createFile: true,
                                credentials: new NetworkCredential("bob", "12345"));
                        }, tags: new string[] { "ftp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("ftp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
        {
            var options = new FtpHealthCheckOptions();
            options.AddHost("ftp://invalid:21");
            var ftpHealthCheck = new FtpHealthCheck(options);

            var result = await ftpHealthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("ftp", instance: ftpHealthCheck, failureStatus: HealthStatus.Degraded,
                    null, timeout: null)
            }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

            result.Exception.Should().BeOfType<OperationCanceledException>();
        }
    }
}
