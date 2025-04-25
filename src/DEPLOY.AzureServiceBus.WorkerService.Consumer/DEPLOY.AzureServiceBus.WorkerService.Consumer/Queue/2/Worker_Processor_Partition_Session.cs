using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Processor_Partition_Session : BackgroundService
    {
        const string queueName = "partition-session";
        private readonly ILogger<Worker_Processor_Partition_Session> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Processor_Partition_Session(
            ILogger<Worker_Processor_Partition_Session> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                Console.WriteLine(Environment.NewLine);
                _logger.LogInformation("Partition Session at: {time}",
                    DateTimeOffset.Now);
                Console.WriteLine(Environment.NewLine);
            }

            ServiceBusSessionProcessor processor = _serviceBusClient
                .CreateSessionProcessor(queueName: queueName, new ServiceBusSessionProcessorOptions
                {
                    AutoCompleteMessages = false,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                    SessionIds = { "PAR", "IMPAR" },
                    MaxConcurrentSessions = 1,
                    SessionIdleTimeout = TimeSpan.FromSeconds(10)
                });

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            processor.SessionInitializingAsync += SessionInitializingHandler;
            processor.SessionClosingAsync += SessionClosingHandler;

            async Task MessageHandler(ProcessSessionMessageEventArgs args)
            {
                string body = args.Message.Body.ToString();

                Console.WriteLine(body);
                Console.WriteLine($"    PartitionKey: {args.Message.PartitionKey}");
                Console.WriteLine($"    Session:      {args.Message.SessionId}");

                await args.CompleteMessageAsync(args.Message);
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                Console.WriteLine(args.ErrorSource);
                Console.WriteLine(args.FullyQualifiedNamespace);
                Console.WriteLine(args.EntityPath);
                Console.WriteLine(args.Exception.ToString());
                return Task.CompletedTask;
            }

            async Task SessionInitializingHandler(ProcessSessionEventArgs args)
            {
                await args.SetSessionStateAsync(new BinaryData("Some state specific to this session when the session is opened for processing."));
            }

            async Task SessionClosingHandler(ProcessSessionEventArgs args)
            {
                // Podemos querer limpar o estado da sessão quando não houver mais mensagens disponíveis
                // para a sessão ou quando alguma mensagem de terminal conhecida tiver sido recebida.
                // Isso depende inteiramente do cenário da aplicação.

                BinaryData sessionState = await args.GetSessionStateAsync();
                if (sessionState.ToString() ==
                    "Fim da sessão xpto")
                {
                    await args.SetSessionStateAsync(null);
                }
            }

            await processor.StartProcessingAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await processor.StopProcessingAsync(stoppingToken);
        }
    }
}
