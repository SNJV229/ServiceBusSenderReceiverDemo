using Microsoft.Azure.ServiceBus;
using SBShared.Models;
using System.Text;
using System.Text.Json;

namespace SBReceiver
{
    class program
    {
        const string connectionString = "Endpoint=sb://sendreceiveservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Q/+jvOJ+MizIv5zoIqtM0FprR+pd9m1Ix41saA8JQqY=";
        const string queueName = "personqueue";
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to the queue message receiving console window");
            //Connection to the queue
            queueClient = new QueueClient(connectionString, queueName);

            //Options
            var MessageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            //Register the process of message Async which listen for the changes
            queueClient.RegisterMessageHandler(processMessageAsync, MessageHandlerOptions);

            Console.ReadLine();

            //Close the connection string
            await queueClient.CloseAsync();
        }

        private static async Task processMessageAsync(Message message, CancellationToken token)
        {
            var jsonString = Encoding.UTF8.GetString(message.Body);
            PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);
            Console.WriteLine($"Person in the queue is: {person.FirstName} {person.LastName}");

            await queueClient.CompleteAsync(message.SystemProperties.LockToken);   //30 sec lock tocken
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Message Handler exception: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}