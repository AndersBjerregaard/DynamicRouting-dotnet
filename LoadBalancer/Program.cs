﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DynamicRouterRabbitMq
{
   
    internal class Program
    {
        static string consumerMsg = "";
        static ManualResetEvent waitHandler = new ManualResetEvent(false);
        static Queue<string> readyConsumers = new Queue<string>();
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

            // channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


            Console.WriteLine(" [*] Waiting for messages.");

            var fromConsumers = new EventingBasicConsumer(channel);
            var fromProducer = new EventingBasicConsumer(channel);
            
            fromConsumers.Received += async (model, ea) => {
                    Console.WriteLine("message received from consumer");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    readyConsumers.Enqueue(message);
                    System.Console.WriteLine(" [x] Received '{0}'", message);
                    System.Console.WriteLine($"length of queue is now {readyConsumers.Count} ");
                    waitHandler.Set();
            };
            
            fromProducer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[loadbalancer]: received {message} from producer");
                
                Console.WriteLine("weeeeeee");
                
                waitHandler.WaitOne();
                System.Console.WriteLine(" [x] Sending '{0}'", message);
                // Publish
                string consumerQueue = readyConsumers.Dequeue();
                Console.WriteLine($"should send to consumer {consumerQueue}");
                System.Console.WriteLine(consumerQueue + ": is ready");
                channel.BasicPublish(exchange: string.Empty,
                                         routingKey: consumerQueue,
                                         basicProperties: null,
                                         body: body);
                if (readyConsumers.Count < 1)
                {
                    Console.WriteLine("readyConsumers count was 0 or less");
                    waitHandler.Reset();
                }
                Thread.Sleep(1000);
                
            };


            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: fromConsumers);

            channel.BasicConsume(queue: "consumeProducer",
                                 autoAck: true,
                                 consumer: fromProducer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            while(true){
                
            }
        }
    }
}