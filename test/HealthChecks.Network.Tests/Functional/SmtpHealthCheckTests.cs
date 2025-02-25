using System.Net;
using FluentAssertions;
using HealthChecks.Network.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.Network.Tests.Functional
{

    public class smtp_healthcheck_should
    {

        //Host and login account to fast switch tests against different server
        private const string _host = "localhost";
        private const string _validAccount = "admin@healthchecks.com";
        private const string _validPassword = "beatpulse";

        [Fact]
        public async Task be_healthy_when_connecting_using_ssl()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        //SSL on by default
                        setup.Host = _host;
                        setup.Port = 465;
                        setup.ConnectionType = SmtpConnectionType.SSL;
                        setup.AllowInvalidRemoteCertificates = true;
                    }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_healthy_when_connecting_using_tls()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        //SSL on by default
                        setup.Host = _host;
                        setup.Port = 587;
                        setup.ConnectionType = SmtpConnectionType.TLS;
                        setup.AllowInvalidRemoteCertificates = true;
                    }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_healthy_when_connecting_using_connection_type_auto()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 587;
                        setup.AllowInvalidRemoteCertificates = true;
                    }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task be_unhealthy_when_connecting_to_an_invalid_smtp_port_with_mode_auto()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddSmtpHealthCheck(setup =>
                        {
                            setup.Host = _host;
                            setup.Port = 45;
                            setup.AllowInvalidRemoteCertificates = true;
                        }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });


            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_when_connection_and_login_with_valid_account_using_ssl_port_and_mode_auto()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 465;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(_validAccount, _validPassword);
                    }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task be_healthy_when_connection_and_login_with_valid_account_using_tls_port_and_mode_auto()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddSmtpHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 587;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(_validAccount, _validPassword);
                    }, tags: new string[] { "smtp" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("smtp")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();

        }


        [Fact]
        public async Task be_unhealthy_when_connection_and_login_with_an_invalid_account()
        {
            var webHostBuilder = new WebHostBuilder()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                      .AddSmtpHealthCheck(setup =>
                      {
                          setup.Host = _host;
                          setup.Port = 587;
                          setup.AllowInvalidRemoteCertificates = true;
                          setup.LoginWith(_validAccount, "wrongpass");
                      }, tags: new string[] { "smtp" });
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("smtp")
                     });
                 });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

        }

        [Fact(Skip = "The SMTP interoperability service at test.smtp.org has been shut down due to abuse.")]
        public async Task be_healthty_when_login_with_plain_smtp()
        {
            /* We use test.smtp.org service to test raw smtp connections as nowadays
            most mail server docker images does not support this scenario
            This test is skipped as this service might not be working in the future */

            var webHostBuilder = new WebHostBuilder()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                      .AddSmtpHealthCheck(setup =>
                      {
                          setup.Host = "test.smtp.org";
                          setup.Port = 25;
                          setup.ConnectionType = SmtpConnectionType.PLAIN;
                          setup.LoginWith("user19@test.smtp.org", "pass19");
                      }, tags: new string[] { "smtp" });
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("smtp")
                     });
                 });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();

        }
    }
}
