using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class QueueEndpoint
    {
        public static void MapQueueEndpointsV1(this IEndpointRouteBuilder app)
        {
            const string QueueTag = "Queue";

            const string simple = "simple";
            const string auto_delete = "auto-delete";
            const string partition = "partition";
            const string partition_session = "partition-session";

            var apiVersionSetQueue_V1 = app
                .NewApiVersionSet(QueueTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Queue_V1 = app
                .MapGroup("/api/v{version:apiVersion}/queue")
                .WithApiVersionSet(apiVersionSetQueue_V1);

            Queue_V1
                .MapPost($"/{simple}", async
                (ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(simple);
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromObjectAsJson(Util.GenerateData.Products(1)),
                        ContentType = "application/json",
                        ReplyTo = "simple-reply",
                    }, cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{simple}-v1",
                    Summary = $"post queue {simple} v1",
                    Description = $"post queue {simple} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {simple} v1");

            Queue_V1
                .MapPost($"/partition/{partition}/batch/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);
                    List<ServiceBusMessage> messages = new();
                    List<Product> products = Util.GenerateData.Products(qtd);

                    products.ForEach(product =>
                    {
                        messages.Add(new ServiceBusMessage()
                        {
                            Body = BinaryData.FromObjectAsJson(product),
                            ContentType = "application/json",
                            PartitionKey = product.Quantity % 2 == 0 ? "PAR" : "IMPAR"
                        });
                    });

                    await SendBatch(sender, messages);

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

            Queue_V1
                .MapPost($"/partition/{partition}/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    List<int> numeros = Util.GenerateData.Numbers(0, 100, qtd);

                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);
                    int numeroAleatorio = numeros[new Random().Next(0, numeros.Count)];
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString($"Canal DEPLOY {numeroAleatorio}"),
                        ContentType = "application/text",
                        PartitionKey = numeroAleatorio % 2 == 0 ? "PAR" : "IMPAR"
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
                .WithSummary($"post queue {partition} v1");

            Queue_V1
                .MapPost($"/partition_session/{partition_session}", async
                (ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    List<int> numeros = Util.GenerateData.Numbers(50, 200, 40);

                    ServiceBusSender sender = serviceBusClient.CreateSender(partition_session);
                    int numeroAleatorio = numeros[new Random().Next(0, numeros.Count)];

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString($"Canal DEPLOY {numeroAleatorio}"),
                        ContentType = "application/text",
                        PartitionKey = numeroAleatorio / 2 == 0 ? "PAR" : "IMPAR",
                        SessionId = numeroAleatorio > 99 ? "MAIOR 100" : "MENOR 100"
                    }, cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{partition_session}-v1",
                    Summary = $"post queue {partition_session} v1",
                    Description = $"post queue {partition_session} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {partition_session} v1");

            Queue_V1
                .MapPost($"/partition_session/{partition_session}/batch/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);
                    List<ServiceBusMessage> messages = new();
                    List<Product> products = Util.GenerateData.Products(qtd);

                    products.ForEach(product =>
                    {
                        messages.Add(new ServiceBusMessage()
                        {
                            Body = BinaryData.FromObjectAsJson(product),
                            ContentType = "application/json",
                            PartitionKey = product.Quantity % 2 == 0 ? "PAR" : "IMPAR",
                            SessionId = product.Price > 99.9M ? "MAIOR 100" : "MENOR 100"
                        });
                    });

                    await SendBatch(sender, messages);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{partition_session}-v1",
                    Summary = $"post queue {partition_session} v1",
                    Description = $"post queue {partition_session} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {partition_session} v1");
        }

        public static async Task SendBatch(
            ServiceBusSender serviceBusSender,
            List<ServiceBusMessage> serviceBusMessages)
        {
            ServiceBusMessageBatch messageBatch = await serviceBusSender.CreateMessageBatchAsync();

            serviceBusMessages.ForEach(message =>
            {
                if (!messageBatch.TryAddMessage(message))
                {
                    throw new Exception($"Message {message.MessageId} is too large to fit in the batch.");
                }
            });

            try
            {
                await serviceBusSender.SendMessagesAsync(messageBatch);
            }
            catch (ServiceBusException ex)
            {
                Console.WriteLine($"Error ServiceBusException sending batch: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Exception sending batch: {ex.Message}");
            }
        }
    }
}
