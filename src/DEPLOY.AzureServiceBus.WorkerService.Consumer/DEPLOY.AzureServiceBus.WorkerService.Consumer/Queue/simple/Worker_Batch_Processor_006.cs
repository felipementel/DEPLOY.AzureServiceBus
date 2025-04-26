using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Batch_Processor_006 : BackgroundService
    {
        private readonly string _queueName = "simple-batch";
        private readonly ILogger<Worker_Batch_Processor_006> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Batch_Processor_006(
            ILogger<Worker_Batch_Processor_006> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        /* O processador oferece conclusao automatica de mensagens processadas,
         * renovacao automatica de bloqueio de mensagens e
         * execucao simultanea de manipuladores de eventos especificados pelo usuario.
         */
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                Console.WriteLine(Environment.NewLine);
                _logger.LogInformation($"{_queueName} at: {DateTimeOffset.Now}");
                Console.WriteLine(Environment.NewLine);
            }

            ServiceBusProcessor processor = _serviceBusClient
                .CreateProcessor(queueName: _queueName, new ServiceBusProcessorOptions()
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
