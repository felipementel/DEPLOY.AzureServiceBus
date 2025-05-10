using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace DEPLOY.AzureServiceBus.API.Extensions
{
    public static class OpenApiConfigExtension
    {
        public static void AddOpenApiConfig(this IServiceCollection services)
        {
            services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            services.AddRouting(opt =>
            {
                opt.LowercaseUrls = true;
                opt.LowercaseQueryStrings = true;
            });

            services
                .AddApiVersioning(options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";

                    options.SubstituteApiVersionInUrl = true;
                })
                .EnableApiVersionBinding();

            services.AddEndpointsApiExplorer();

            services.AddOpenApi("v1", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;

                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Felipe Augusto, MVP",
                        Url = new Uri("https://www.youtube.com/@D.E.P.L.O.Y"),
                    };
                    document.Info.License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    };
                    return Task.CompletedTask;
                });
            });

            services.AddOpenApi("v2", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;

                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Felipe Augusto, MVP",
                        Url = new Uri("https://www.youtube.com/@D.E.P.L.O.Y"),
                    };
                    document.Info.License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    };
                    return Task.CompletedTask;
                });
            });
        }

        public static void UseOpenApiConfig(this WebApplication app)
        {
            app.MapOpenApi();

            //scalar
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Canal DEPLOY - Azure Service Bus");
                options.WithTheme(ScalarTheme.BluePlanet);
                options.WithSidebar(true);
                options.WithTestRequestButton(true);
                options.WithLayout(ScalarLayout.Modern);
            });
        }
    }
}
