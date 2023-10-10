﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DynamicRouterRabbitMq
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string hostName = Environment.GetEnvironmentVariable("HOSTNAME") ?? "HARDCODED_CONSUMERNAME1";
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("HostName") ?? "rabbitmq",
                Port = Environment.GetEnvironmentVariable("Port") != default ? int.Parse(Environment.GetEnvironmentVariable("Port")) : 5672,
                UserName = Environment.GetEnvironmentVariable("UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("UserName") ?? "guest"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "DR_Exchange", type: ExchangeType.Direct);

            channel.QueueDeclare(hostName,false,false,false, null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine($"[consumer {hostName}]: Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [consumer {hostName}]: Received {message}");

                Thread.Sleep(1100);

                Console.WriteLine($"[consumer {hostName}]: Done");


                var messageTwo = hostName;
                var bodyTwo = Encoding.UTF8.GetBytes(messageTwo);

                channel.BasicPublish(exchange: "DR_Exchange",
                    routingKey: "",
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(hostName));
            };

            
                         
            channel.BasicConsume(queue: hostName,
                                 autoAck: true,
                                 consumer: consumer);

            channel.BasicPublish(exchange: "DR_Exchange",
                    routingKey: "",
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(hostName));
            while(true){    
            }   
        }
    }
}