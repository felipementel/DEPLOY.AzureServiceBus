# DEPLOY.AzureServiceBus

dotnet run --project .\src\DEPLOY.AzureServiceBus.API\DEPLOY.AzureServiceBus.API\DEPLOY.AzureServiceBus.API.csproj

dotnet run --project .\src\DEPLOY.AzureServiceBus.WorkerService.Consumer\DEPLOY.AzureServiceBus.WorkerService.Consumer\DEPLOY.AzureServiceBus.WorkerService.Consumer.csproj

dotnet run --project .\src\DEPLOY.AzureServiceBus.Function.Consumer\DEPLOY.AzureServiceBus.Function.Consumer\DEPLOY.AzureServiceBus.Function.Consumer.csproj


 Limitações
É claro que o Emulador do Barramento de Serviço não é um serviço real do Barramento de Serviço. Ele tem algumas limitações:

- Não pode transmitir mensagens usando o protocolo JMS.
- Entidades particionadas não são compatíveis com o Emulador.
- Ele não oferece suporte a operações de gerenciamento em tempo real por meio de um SDK do lado do cliente.
- Ele não oferece suporte a recursos de nuvem, como dimensionamento automático ou recursos de recuperação de desastres geográficos, etc.
- Ele tem um limite de 1 namespace e 50 filas/tópicos.
* Mais limitações podem ser encontradas na [Visão Geral do Emulador do Barramento de Serviço do Azure](https://learn.microsoft.com/pt-br/azure/service-bus-messaging/overview-emulator#known-limitations).
