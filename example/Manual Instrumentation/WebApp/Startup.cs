using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using LMResourceDetector;
using System.Collections.Generic;

namespace WebAppExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddOpenTelemetryTracing(builder => ConfigureOpenTelemetry(builder));
        }

        private static void ConfigureOpenTelemetry(TracerProviderBuilder builder)
        { 
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string servicename = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Dotnet";
            var otlpEndpoint = "http://localhost:55681/v1/traces";
            string otlp_format = Environment.GetEnvironmentVariable("OTLP_FORMAT") ?? "HTTP";
            string headers = Environment.GetEnvironmentVariable("OTLP_HEADERS") ?? null;
            Dictionary<String, object> resourceAttributeList = ResourceDetector.Detect();
            builder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
            builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(resourceAttributeList));
            builder.AddConsoleExporter();
            builder.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(otlpEndpoint);


                //otlpOptions.Endpoint = new Uri(otlpEndpoint);
                if (otlp_format == "HTTP")
                {
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    if (headers != null)
                    {
                        otlpOptions.Headers = headers;
                        Console.WriteLine("Headers" + headers);
                    }
                    Console.WriteLine("OTLP EXPORTER ENdpoint " + otlpEndpoint + " " + servicename);

                }
                else
                {
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }
            });
         }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
