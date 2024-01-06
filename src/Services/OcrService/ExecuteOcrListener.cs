using DocCap.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OcrService;

public class ExecuteOcrListener
{
    private readonly string _hostname = "localhost"; // Update as needed
    private readonly string _queueName = "ocr-queue"; // Queue name

    public void Listen()
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = _hostname,
            Password = "guest",
            UserName = "guest",
            Port = 5672            
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var command = JsonSerializer.Deserialize<MassTransitMessage<ExecuteOcrCommand>>(message);

            // Process the command
            ProcessCommand(command);
        };

        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }

    private void ProcessCommand(MassTransitMessage<ExecuteOcrCommand> command)
    {
        // Implement OCR processing logic here

        // Send OcrCompleted message
        SendOcrCompletedMessage(command.Message.CorrelationId);
    }

    private void SendOcrCompletedMessage(Guid correlationId)
{
    var factory = new ConnectionFactory() { HostName = _hostname };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    var completedMessage = new OcrCompleted("MyInstanceId", correlationId, "HelloWorld");
    var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(completedMessage));

    channel.BasicPublish(exchange: "", routingKey: "ocr-completed-queue", basicProperties: null, body: messageBody);
}
}
