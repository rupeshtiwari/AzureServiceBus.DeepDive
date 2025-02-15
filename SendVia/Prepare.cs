using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace SendVia
{
    public static class Prepare
    {
        public static async Task Stage(string connectionString, string inputQueue, string destinationQueue)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            if (await client.QueueExistsAsync(inputQueue)) await client.DeleteQueueAsync(inputQueue);
            await client.CreateQueueAsync(new CreateQueueOptions(inputQueue) { MaxDeliveryCount = 2 });

            if (await client.QueueExistsAsync(destinationQueue)) await client.DeleteQueueAsync(destinationQueue);
            await client.CreateQueueAsync(destinationQueue);
        }

        public static async Task ReportNumberOfMessages(string connectionString, string destination)
        {
            var client = new ServiceBusAdministrationClient(connectionString);

            QueueRuntimeProperties info = await client.GetQueueRuntimePropertiesAsync(destination);

            Console.WriteLine($"#'{info.ActiveMessageCount}' messages in '{destination}'");
        }
    }
}