using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost", Port=5672 };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "consumeProducer",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);


const string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);


for (var i = 0; i < 1000; i++)
{
    channel.BasicPublish(exchange: string.Empty,
                         routingKey: "consumeProducer",
                         basicProperties: null,
                         body: body);
    Console.WriteLine($" [x] Sent {message}");
    Thread.Sleep(1000);
}

Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();