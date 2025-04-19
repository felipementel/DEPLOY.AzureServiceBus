using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Processor_Partition : BackgroundService
    {
        const string queueName = "partition";
        private readonly ILogger<Worker_Processor_Partition> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Processor_Partition(
            ILogger<Worker_Processor_Partition> logger,
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

            if (_logger.IsEnabled(LogLevel.Information))
            {
                Console.WriteLine(Environment.NewLine);
                _logger.LogInformation($"{queueName}" + " at: {time}",
                    DateTimeOffset.Now);
                Console.WriteLine(Environment.NewLine);
            }

            ServiceBusProcessor processor = _serviceBusClient
                .CreateProcessor(queueName: queueName, new ServiceBusProcessorOptions()
                {
                    MaxConcurrentCalls = 50,
                    AutoCompleteMessages = false,
                });

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await processor.StopProcessingAsync(stoppingToken);
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            Console.WriteLine(body);
            Console.WriteLine($"    PartitionKey: {args.Message.PartitionKey}");

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
    }
}
