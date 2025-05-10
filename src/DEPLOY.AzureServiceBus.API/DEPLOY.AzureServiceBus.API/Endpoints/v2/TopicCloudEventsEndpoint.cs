using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v2
{
    public static partial class DEPLOYEndpoint
    {
        public static void MapTopicsCloudEventsEndpointsV2(this IEndpointRouteBuilder app)
        {
            const string TopicTag = "Topic";
            const string topic_cloud_events = "cloud-events";

            var apiVersionSetTopic_V2 = app
                .NewApiVersionSet(TopicTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var Topic_V2 = app
                .MapGroup("/api/v{version:apiVersion}/topics")
                .WithApiVersionSet(apiVersionSetTopic_V2);

            Topic_V2
                .MapPost($"/{topic_cloud_events}/{{qtd}}", async
                (int qtd,
                ServiceBusClient servceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = servceBusClient.CreateSender("cloud-events");

                    List<Product> products = Products(qtd);

                    foreach (var item in products)
                    {
                        var cloudEvent = new CloudEvent(
                            "/cloudevents/canal-deploy/source",
                            "command.globalazure.floripa",
                            item);

                        cloudEvent.Subject = "Create-Product";
                        cloudEvent.DataSchema = "/cloudevents/canal-deploy/schema";
                        cloudEvent.DataContentType = "application/json";

                        ServiceBusMessage message = new ServiceBusMessage(new BinaryData(cloudEvent))
                        {
                            MessageId = Guid.NewGuid().ToString(),
                            ContentType = "application/cloudevents+json",
                            PartitionKey = item.Quantity % 2 == 0 ? "PAR" : "IMPAR",
                            SessionId = item.Quantity % 2 == 0 ? "PAR" : "IMPAR"
                        };

                        await sender.SendMessageAsync(message);
                    }

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-topic-{topic_cloud_events}-v1",
                    Summary = $"post topic {topic_cloud_events} v1",
                    Description = $"post topic {topic_cloud_events} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = TopicTag }
                    }
                })
                .WithSummary($"post topic {topic_cloud_events} v1");
        }
    }
}
