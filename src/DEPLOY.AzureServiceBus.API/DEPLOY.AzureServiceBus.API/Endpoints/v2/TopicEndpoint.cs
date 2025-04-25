using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v2
{
    public static partial class DEPLOYEndpoint
    {
        public static void MapTopicsEndpointsV2(this IEndpointRouteBuilder app)
        {
            const string TopicTag = "Topic";
            const string topic_without_partition = "without-partition";

            var apiVersionSetTopic_V2 = app
                .NewApiVersionSet(TopicTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var Topic_V2 = app
                .MapGroup("/api/v{version:apiVersion}/topic")
                .WithApiVersionSet(apiVersionSetTopic_V2);

            Topic_V2
                .MapPost($"/{topic_without_partition}", async
                (ServiceBusClient servceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = servceBusClient.CreateSender(topic_without_partition);
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Canal DEPLOY"),

                        ContentType = "application/text"
                    }, cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-topic-{topic_without_partition}-v1",
                    Summary = $"post topic {topic_without_partition} v1",
                    Description = $"post topic {topic_without_partition} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = TopicTag }
                    }
                })
                .WithSummary($"post topic {TopicTag} v1");
        }
    }
}