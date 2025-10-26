using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MathFlow.Infrastructure.Observability;

/// <summary>
/// Configures OpenTelemetry for collecting logs, traces, and metrics
/// </summary>
public static class OpenTelemetryConfigurator
{
    private const string ServiceName = "MathFlow";
    private const string ServiceVersion = "1.0.0";

    /// <summary>
    /// Adds OpenTelemetry to the WebApplicationBuilder
    /// </summary>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
        
        builder.Logging.AddOpenTelemetry(logging => ConfigureLogging(logging, otlpEndpoint));
        
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(ConfigureResource)
            .WithTracing(tracing => ConfigureTracing(tracing, otlpEndpoint))
            .WithMetrics(metrics => ConfigureMetrics(metrics, otlpEndpoint));
        
        return builder;
    }

    /// <summary>
    /// Configures resource attributes (service name, version, etc)
    /// </summary>
    private static void ConfigureResource(ResourceBuilder resource)
    {
        resource
            .AddService(
                serviceName: ServiceName,
                serviceVersion: ServiceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();
    }

    /// <summary>
    /// Configures log exportation
    /// </summary>
    private static void ConfigureLogging(OpenTelemetryLoggerOptions logging, string? otlpEndpoint)
    {
        logging.AddConsoleExporter();
        
        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            logging.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        }
    }

    /// <summary>
    /// Configures instrumentation and trace exportation
    /// </summary>
    private static void ConfigureTracing(TracerProviderBuilder tracing, string? otlpEndpoint)
    {
        tracing
            .AddAspNetCoreInstrumentation(opts =>
            {
                opts.RecordException = true;
                opts.Filter = (httpContext) => 
                {
                    var path = httpContext.Request.Path.Value ?? string.Empty;
                    return !path.Contains("/health") && 
                           !path.Contains("/_framework/");
                };
            })
            .AddHttpClientInstrumentation(opts =>
            {
                opts.RecordException = true;
            });
        
        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            tracing.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        }
    }

    /// <summary>
    /// Configures instrumentation and metrics exportation
    /// </summary>
    private static void ConfigureMetrics(MeterProviderBuilder metrics, string? otlpEndpoint)
    {
        metrics
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddMeter("Microsoft.AspNetCore.Http.Connections")
            .AddMeter("Microsoft.AspNetCore.Routing")
            .AddMeter("Microsoft.AspNetCore.Diagnostics")
            .AddMeter("Microsoft.AspNetCore.RateLimiting")
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation();
        
        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            metrics.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        }
    }
}
