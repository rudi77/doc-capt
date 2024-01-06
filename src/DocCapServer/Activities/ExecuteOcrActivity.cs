using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using MassTransit;
using DocCap.Contracts;

namespace DocCapServer.Activities;

[Elsa.Workflows.Attributes.Activity(
    "DocumentCapture", 
    "Ocr", 
    "Sends a document to the ocr service.", 
    DisplayName = "Send to Ocr")]
public class ExecuteOcrActivity : CodeActivity
{
    /// <summary>
    /// The file content to send to the OCR service.
    /// </summary>
    [Elsa.Workflows.Attributes.Input(Description = "The file content to send to the OCR service.")]
    public Input<byte[]> DocumentContent { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var sendEndpointProvider = context.WorkflowExecutionContext.GetService<ISendEndpointProvider>();

        if (sendEndpointProvider == null)
        {
            throw new ArgumentNullException("SendEndpointProvider is null");
        }

        // create OcrCommand 
        var correlationId = Guid.NewGuid();
        var content = DocumentContent.Get<byte[]>(context);
        var command = new ExecuteOcrCommand(context.WorkflowExecutionContext.Id, correlationId, content);
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:ocr-queue"));

        await sendEndpoint.Send(command);      
    }
}
