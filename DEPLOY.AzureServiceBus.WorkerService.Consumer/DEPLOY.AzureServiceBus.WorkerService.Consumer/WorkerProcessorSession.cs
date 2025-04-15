using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerProcessorSession : BackgroundService
    {
        private readonly ILogger<WorkerProcessorSession> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerProcessorSession(
            ILogger<WorkerProcessorSession> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool mustReply = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    Console.WriteLine(Environment.NewLine);
                    _logger.LogInformation("Partition Session at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusSessionProcessor processor = _serviceBusClient
                    .CreateSessionProcessor(queueName: "partition-session", new ServiceBusSessionProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        ReceiveMode = ServiceBusReceiveMode.PeekLock,
                        SessionIds = { "session-1", "session-2", "session-3" },
                        MaxConcurrentCallsPerSession = 2,
                        MaxConcurrentSessions = 3
                    });

                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                processor.SessionInitializingAsync += SessionInitializingHandler;
                processor.SessionClosingAsync += SessionClosingHandler;

                async Task MessageHandler(ProcessSessionMessageEventArgs args)
                {
                    var body = args.Message.Body.ToString();

                    // we can evaluate application logic and use that to determine how to settle the message.
                    await args.CompleteMessageAsync(args.Message);

                    // we can also set arbitrary session state using this receiver
                    // the state is specific to the session, and not any particular message
                    await args.SetSessionStateAsync(new BinaryData("Some state specific to this session when processing a message."));
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
                    // We may want to clear the session state when no more messages are available for the session or when some known terminal message
                    // has been received. This is entirely dependent on the application scenario.
                    BinaryData sessionState = await args.GetSessionStateAsync();
                    if (sessionState.ToString() ==
                        "Some state that indicates the final message was received for the session")
                    {
                        await args.SetSessionStateAsync(null);
                    }
                }

                await processor.StartProcessingAsync();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
