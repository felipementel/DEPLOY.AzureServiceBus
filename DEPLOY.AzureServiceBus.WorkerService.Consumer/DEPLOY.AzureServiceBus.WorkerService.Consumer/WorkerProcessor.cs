using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerProcessor : BackgroundService
    {
        private readonly ILogger<WorkerProcessor> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerProcessor(
            ILogger<WorkerProcessor> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        /*O processador oferece conclusão automática de mensagens processadas,
         * renovação automática de bloqueio de mensagens e
         * execução simultânea de manipuladores de eventos especificados pelo usuário.
         */
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("WorkerProcessor running at: {time}",
                        DateTimeOffset.Now);
                }

                ServiceBusProcessor processor = _serviceBusClient
                    .CreateProcessor(queueName: "simple");

                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                async Task MessageHandler(ProcessMessageEventArgs args)
                {
                    string body = args.Message.Body.ToString();
                    Console.WriteLine(body);

                    await args.CompleteMessageAsync(args.Message);

                    ServiceBusSender sender = _serviceBusClient.CreateSender(args.Message.ReplyTo);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Produto Cadastrado com sucesso."),
                        SessionId = args.Message.ReplyToSessionId,
                        ApplicationProperties =
                        {
                            ["ReplyTo"] = args.Message.ReplyTo,
                            ["SessionId"] = args.Message.SessionId,
                            ["MessageId"] = args.Message.MessageId
                        }
                    }, stoppingToken);
                }

                Task ErrorHandler(ProcessErrorEventArgs args)
                {
                    Console.WriteLine(args.ErrorSource);
                    Console.WriteLine(args.FullyQualifiedNamespace);
                    Console.WriteLine(args.EntityPath);
                    Console.WriteLine(args.Exception.ToString());

                    return Task.CompletedTask;
                }

                await processor.StartProcessingAsync();
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
