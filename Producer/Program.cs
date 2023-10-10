using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("HostName") ?? "rabbitmq",
    Port = Environment.GetEnvironmentVariable("Port") != default ? int.Parse(Environment.GetEnvironmentVariable("Port")) : 5672,
    UserName = Environment.GetEnvironmentVariable("UserName") ?? "guest",
    Password = Environment.GetEnvironmentVariable("UserName") ?? "guest"
};
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