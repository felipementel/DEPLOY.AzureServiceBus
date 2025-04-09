using Azure.Messaging.ServiceBus;
using System.Text.Json;

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
                })
                .Produces(202)
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
                .MapPost($"/{auto_delete}", async
                (ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    ServiceBusSender sender = serviceBusClient.CreateSender(auto_delete);
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromObjectAsJson(
                            System.Text.Json.JsonSerializer.Serialize(
                                Util.GenerateData.Products(1),
                                new JsonSerializerOptions()
                                {
                                    WriteIndented = true
                                })
                            ),
                        ContentType = "application/json"
                    }, cancellationToken);
                })
                .Produces(202)
                .WithOpenApi(operation => new(operation)
                {
                    OperationId = $"post-queue-{auto_delete}-v1",
                    Summary = $"post queue {auto_delete} v1",
                    Description = $"post queue {auto_delete} v1",
                    Tags = new List<Microsoft.OpenApi.Models.OpenApiTag>
                    {
                        new() { Name = QueueTag }
                    }
                })
                .WithSummary($"post queue {auto_delete} v1");

            Queue_V1
                .MapPost($"/{partition}", async
                (ServiceBusClient serviceBusClient,
                CancellationToken cancellationToken) =>
                {
                    List<int> numeros = Util.GenerateData.Numbers(0, 100, 10);

                    ServiceBusSender sender = serviceBusClient.CreateSender(partition);
                    int numeroAleatorio = numeros[new Random().Next(0, numeros.Count)];
                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString($"Canal DEPLOY {numeroAleatorio}"),
                        ContentType = "application/text",
                        PartitionKey = numeroAleatorio / 2 == 0 ? "PAR" : "IMPAR"
                    }, cancellationToken);
                })
                .Produces(202)
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
                .MapPost($"/{partition_session}", async
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
                })
                .Produces(202)
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
    }
}
