using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class DEPLOYEndpoint
    {
        public static void MapTopicsEndpointsV1(this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
        {
            const string TopicTag = "Topic";

            var apiVersionSetTopic_V1 = app
                .NewApiVersionSet(TopicTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Topic_V1 = app
                .MapGroup("/api/v{version:apiVersion}/topic")
                .WithApiVersionSet(apiVersionSetTopic_V1);

            Topic_V1
                .MapPost("/topic-sem-particao", async
                (ServiceBusClient servceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = servceBusClient.CreateSender("deploy-sem-particao");
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Canal DEPLOY"),
                        ContentType = "application/text"
                    }, cancellationToken);
                })
                .Produces(202)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = "post-topic-sem-particao-v1",
                    Summary = "post topic sem particao v1",
                    Description = "post topic sem particao v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = TopicTag }
                    }
                })
                .WithSummary("post topic sem particao v1");
        }
    }
}
