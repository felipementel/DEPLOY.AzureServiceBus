using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class DEPLOYEndpoint
    {
        public static void MapTopicsEndpointsV1(this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
        {
            const string TopicTag = "Topic";
            const string topic_without_partition = "without-partition";
            const string topic_cloud_events = "cloud-events";


            var apiVersionSetTopic_V1 = app
                .NewApiVersionSet(TopicTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Topic_V1 = app
                .MapGroup("/api/v{version:apiVersion}/topic")
                .WithApiVersionSet(apiVersionSetTopic_V1);

            Topic_V1
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

            Topic_V1
                .MapPost($"/{topic_cloud_events}/{{qtd}}", async
                (int qtd, 
                ServiceBusClient servceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = servceBusClient.CreateSender("cloud-events");

                    List<Product> products = Util.GenerateData.Products(qtd);

                    foreach (var item in products)
                    {
                        var cloudEvent = new CloudEvent(
                            "/cloudevents/canal-deploy/source",
                            "mvpconf.product",
                            item);

                        ServiceBusMessage message = new ServiceBusMessage(new BinaryData(cloudEvent))
                        {
                            ContentType = "application/cloudevents+json",
                            PartitionKey = item.Quantity % 2 == 0 ? "PAR" : "IMPAR",
                            SessionId = item.Quantity % 2 == 0 ? "PAR" : "IMPAR"
                        };

                        // send the message
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
