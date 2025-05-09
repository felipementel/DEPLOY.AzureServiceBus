using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class QueueEndpoint
    {
        public static void MapQueuePartitionEndpointsV1(this IEndpointRouteBuilder app)
        {
            const string QueueTag = "Queue-Partition";

            const string partition = "partition";

            var apiVersionSetQueue_V1 = app
                .NewApiVersionSet(QueueTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Queue_V1 = app
                .MapGroup("/api/v{version:apiVersion}/queues-partitions")
                .WithApiVersionSet(apiVersionSetQueue_V1);

            Queue_V1
                .MapPost($"/{partition}", async
                (ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    Product product = Util.GenerateData.Products(1)[0];

                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Body = BinaryData.FromObjectAsJson(product, new System.Text.Json.JsonSerializerOptions()
                        {
                            WriteIndented = true
                        }),
                        ContentType = "application/text",
                        PartitionKey = product.Quantity % 2 == 0 ? "PAR" : "IMPAR"
                    }, cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{partition}-v1",
                    Summary = $"post queue {partition} v1",
                    Description = $"post queue {partition} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {partition} v1 - Product");

            Queue_V1
                .MapPost($"/{partition}/batch/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);
                    List<ServiceBusMessage> messagesPAR = new();
                    List<ServiceBusMessage> messagesIMPAR = new();
                    List<Product> products = Util.GenerateData.Products(qtd);

                    products.ForEach(product =>
                    {
                        if (product.Quantity % 2 == 0)
                            messagesPAR.Add(new ServiceBusMessage()
                            {
                                MessageId = Guid.NewGuid().ToString(),
                                Body = BinaryData.FromObjectAsJson(product),
                                ContentType = "application/json",
                                PartitionKey = "PAR"
                            });
                        else
                            messagesIMPAR.Add(new ServiceBusMessage()
                            {
                                Body = BinaryData.FromObjectAsJson(product),
                                ContentType = "application/json",
                                PartitionKey = "IMPAR"
                            });
                    });

                    await SendBatchAsync(sender, messagesPAR);
                    await SendBatchAsync(sender, messagesIMPAR);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{partition} batch v1",
                    Summary = $"post queue {partition} batch v1",
                    Description = $"post queue {partition} batch v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {partition} batch v1");
        }
    }
}
