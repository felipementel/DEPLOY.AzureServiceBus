using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class QueueEndpoint
    {
        public static void MapQueueSessionEndpointsV1(this IEndpointRouteBuilder app)
        {
            const string QueueTag = "Queue-Session";

            const string partition_session = "partition-session";

            var apiVersionSetQueue_V1 = app
                .NewApiVersionSet(QueueTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Queue_V1 = app
                .MapGroup("/api/v{version:apiVersion}/queues-partitions-sessions")
                .WithApiVersionSet(apiVersionSetQueue_V1);

            Queue_V1
                .MapPost($"/{partition_session}/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(partition_session);

                    for (int i = 0; i < qtd; i++)
                    {
                        if (i % 2 == 0)
                            await sender.SendMessageAsync(new ServiceBusMessage()
                            {
                                MessageId = Guid.NewGuid().ToString(),
                                Body = BinaryData.FromString($"Canal DEPLOY | Global Azure Floripa {i}"),
                                ContentType = "application/json",
                                PartitionKey = "PAR",
                                SessionId = "PAR"
                            }, cancellationToken);
                        else
                            await sender.SendMessageAsync(new ServiceBusMessage()
                            {
                                MessageId = Guid.NewGuid().ToString(),
                                Body = BinaryData.FromString($"Canal DEPLOY | Global Azure Floripa {i}"),
                                ContentType = "application/json",
                                PartitionKey = "IMPAR",
                                SessionId = "IMPAR",

                            }, cancellationToken);
                    }

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
                .MapPost($"/{partition_session}/batch/{{qtd}}", async
                ([FromRoute] int qtd,
                ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(partition_session);
                    List<ServiceBusMessage> messagesPAR = new();
                    List<ServiceBusMessage> messagesIMPAR = new();
                    List<Product> products = Util.GenerateData.Products(qtd);

                    products.ForEach(product =>
                    {
                        if (product.Price > 99.9M)
                            messagesPAR.Add(new ServiceBusMessage()
                            {
                                Body = BinaryData.FromObjectAsJson(product),
                                ContentType = "application/json",
                                PartitionKey = product.Quantity % 2 == 0 ? "PAR" : "IMPAR",
                                SessionId = "MAIOR 100"
                            });
                        else
                            messagesPAR.Add(new ServiceBusMessage()
                            {
                                Body = BinaryData.FromObjectAsJson(product),
                                ContentType = "application/json",
                                PartitionKey = product.Quantity % 2 == 0 ? "PAR" : "IMPAR",
                                SessionId = "MENOR 100"
                            });
                    });

                    await SendBatchSessionAsync(sender, messagesPAR);
                    await SendBatchSessionAsync(sender, messagesIMPAR);

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

        private static async Task SendBatchSessionAsync(
        ServiceBusSender serviceBusSender,
        List<ServiceBusMessage> serviceBusMessages)
        {
            ServiceBusMessageBatch messageBatch = await serviceBusSender.CreateMessageBatchAsync();

            foreach (var message in serviceBusMessages)
            {
                if (!messageBatch.TryAddMessage(message))
                {
                    await serviceBusSender.SendMessagesAsync(messageBatch);
                    messageBatch = await serviceBusSender.CreateMessageBatchAsync();
                    messageBatch.TryAddMessage(new ServiceBusMessage(message.Body)
                    {
                        ContentType = message.ContentType,
                        PartitionKey = message.PartitionKey,
                        SessionId = message.SessionId
                    });
                }
            }

            try
            {
                if (messageBatch.Count > 0)
                {
                    await serviceBusSender.SendMessagesAsync(messageBatch);
                }
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
