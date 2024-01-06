using DocCap.Contracts;
using MassTransit;

namespace OcrService;

public class ExecuteOcrCommandConsumer : IConsumer<ExecuteOcrCommand>
{
    public async Task Consume(ConsumeContext<ExecuteOcrCommand> context)
    {
        var command = context.Message;
        // Process the command
        ProcessCommand(command);

        // Send OcrCompleted message
        await context.Publish(new OcrCompleted(command.InstanceId, command.CorrelationId, "HelloWorld"));
    }

    private void ProcessCommand(DocCap.Contracts.ExecuteOcrCommand command)
    {
        // Implement OCR processing logic here
    }
}
