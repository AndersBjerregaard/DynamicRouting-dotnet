﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DynamicRouterRabbitMq
{
   
    internal class Program
    {
        static string consumerMsg = "";
        static ManualResetEvent waitHandler = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("HostName") ?? "rabbitmq",
                Port = Environment.GetEnvironmentVariable("Port") != default ? int.Parse(Environment.GetEnvironmentVariable("Port")) : 5672,
                UserName = Environment.GetEnvironmentVariable("UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("UserName") ?? "guest"
            };
        

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            

            // declare a server-named queue
            var queueName = "loadQueues";
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: queueName,
                              exchange: "DR_Exchange",
                              routingKey: "");

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            var consumerTwo = new EventingBasicConsumer(channel);
            
            consumer.Received += (model, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    consumerMsg = message;
                    System.Console.WriteLine(" [x] Received '{0}'", message);
                    waitHandler.Set();
            };
            
            consumerTwo.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
        
                System.Console.WriteLine(" [x] Sending '{0}'", message);
                waitHandler.WaitOne();
                // Publish
                System.Console.WriteLine(consumerMsg);
                channel.BasicPublish(exchange: string.Empty,
                                         routingKey: consumerMsg,
                                         basicProperties: null,
                                         body: body);
                    
                
                System.Console.WriteLine("Waiting for Queues to be rdy...");
                Thread.Sleep(1000);
                System.Console.WriteLine("Done");
                waitHandler.Reset();
            };


            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            channel.BasicConsume(queue: "consumeProducer",
                                 autoAck: true,
                                 consumer: consumerTwo);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            while(true){
                
            }
        }
    }
}