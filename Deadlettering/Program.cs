﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Expiry
{
    class Program
    {
        static string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
        static string destination = "queue";

        static async Task Main(string[] args)
        {
            await Prepare.Stage(connectionString, destination);

            var client = new QueueClient(connectionString, destination);

            var message = new Message();
            message.Body = Encoding.UTF8.GetBytes("Half life");
            message.TimeToLive = TimeSpan.FromSeconds(1);
            await client.SendAsync(message);

            message = new Message();
            message.Body = Encoding.UTF8.GetBytes("Delivery Count");
            await client.SendAsync(message);

            message = new Message();
            message.Body = Encoding.UTF8.GetBytes("Poor Soul");
            message.UserProperties.Add("Yehaa", "Why so happy?");
            await client.SendAsync(message);

            await Task.Delay(2000);

            client.RegisterMessageHandler(
            async (msg, token) =>
            {
                switch (Encoding.UTF8.GetString(msg.Body))
                {
                    case "Half life":
                        await client.AbandonAsync(msg.SystemProperties.LockToken);
                        break;
                    case "Delivery Count":
                        throw new InvalidOperationException();
                    case "Poor Soul":
                        await client.DeadLetterAsync(msg.SystemProperties.LockToken, new Dictionary<string, object> {
                            { "Reason", "Because we can!" },
                            { "When", DateTimeOffset.UtcNow },
                        });
                        break;
                }
            },
            new MessageHandlerOptions(exception => Task.CompletedTask)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 3
            });

            await Task.Delay(5000); // don't do this at home

            await client.CloseAsync();
        }
    }
}