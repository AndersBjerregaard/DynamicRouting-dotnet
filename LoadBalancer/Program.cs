﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DynamicRouterRabbitMq
{
   
    internal class Program
    {
         static List<string> queue = new List<string>();
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost", Port=5672,
            UserName="guest", Password="guest" };
        

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




            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            var consumerTwo = new EventingBasicConsumer(channel);
            
            consumer.Received += (model, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    queue.Add(message);
                    System.Console.WriteLine(" [x] Received '{0}'", message);
            };
            
            consumerTwo.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if(ea.Exchange == "DR_Exchange"){

                    queue.Add(message);

                }
                else{
                  Console.WriteLine($" [x] Received '{message}'");

                  bool running = true;
                  while(running){
                      // Publish
                      foreach(var item in queue)
                      {
                         System.Console.WriteLine(item);
                          channel.BasicPublish(exchange: "DR_Exchange",
                                               routingKey: item,
                                               basicProperties: null,
                                               body: body);
                          running = false;

                      }
                      System.Console.WriteLine("Waiting for Queues to be rdy...");
                      Thread.Sleep(1000);

                }
                }
            };


            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            channel.BasicConsume(queue: "consumeProducer",
                                 autoAck: true,
                                 consumer: consumerTwo);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}