using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace DEPLOY.AzureServiceBus.API.Endpoints.v1
{
    public static partial class QueueEndpoint
    {
        public static void MapQueueEndpointsV1(this IEndpointRouteBuilder app)
        {
            const string QueueTag = "Queue";

            const string simple = "simple";

            var apiVersionSetQueue_V1 = app
                .NewApiVersionSet(QueueTag)
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var Queue_V1 = app
                .MapGroup("/api/v{version:apiVersion}/queues")
                .WithApiVersionSet(apiVersionSetQueue_V1);

            Queue_V1
                .MapPost($"/{simple}", async (
                    ServiceBusClient serviceBusClient,
                    CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender($"{simple}-product");
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Body = BinaryData.FromObjectAsJson(Util.GenerateData.Products(1)),
                        ContentType = "application/json"
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
                .WithSummary($"post queue {simple} v1 - Command Create Product");

            Queue_V1
                .MapPost($"/{simple}-duplicate", async (
                    [FromBody] string msg,
                    ServiceBusClient serviceBusClient,
                    CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender($"{simple}-duplicate");
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString(msg),
                        ContentType = "application/json",
                        MessageId = msg.Length.ToString(),
                    }, cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{simple}-duplicate-v1",
                    Summary = $"post queue {simple}-duplicate v1",
                    Description = $"post queue {simple}-duplicate v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {simple} duplicate v1 - Command Create Product");

            Queue_V1
                .MapPost($"/{simple}-schedule", async (
                    [FromBody] string msg,
                    [FromHeader] double scheduleInSecconds,
                    ServiceBusClient serviceBusClient,
                    CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender($"{simple}-schedule");
                    await sender.ScheduleMessageAsync(new ServiceBusMessage()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Body = BinaryData.FromString(msg),
                        ContentType = "application/json",
                    }, DateTimeOffset.Now.AddSeconds(scheduleInSecconds),
                    cancellationToken);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{simple}-schedule-v1",
                    Summary = $"post queue {simple}-schedule v1",
                    Description = $"post queue {simple}-schedule v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {simple} schedule v1 - Command Create Product");

            Queue_V1
                .MapPost($"/{simple}/{{qtd}}", async (
                    [FromRoute] int qtd,
                    ServiceBusClient serviceBusClient,
                    CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender($"{simple}-batch");

                    List<ServiceBusMessage> messages = new();
                    for (int i = 0; i < qtd; i++)
                    {
                        messages.Add(new ServiceBusMessage(BinaryData.FromString($"Canal DEPLOY | MVPConf Blumenau {i}"))
                        {
                            MessageId = Guid.NewGuid().ToString(),
                            ContentType = "application/text",
                            ReplyTo = "simple-reply",
                        });
                    }

                    await SendBatchAsync(sender, messages);

                    return Results.Accepted();
                })
                .Produces(StatusCodes.Status202Accepted)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{simple}-qtd--v1",
                    Summary = $"post queue {simple} qtd v1",
                    Description = $"post queue {simple} qtd v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {simple} qtd v1 - Command Create Products");
        }

        public static async Task SendBatchAsync(
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
                    messageBatch.TryAddMessage(message);
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
