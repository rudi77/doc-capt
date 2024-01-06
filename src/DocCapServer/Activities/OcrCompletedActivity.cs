using DocCap.Contracts;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Runtime.Bookmarks;

namespace DocCapServer.Activities;

[Elsa.Workflows.Attributes.Activity(
    "DocumentCapture", 
    "Ocr", 
    "Waits for a OcrCompleted message",
    DisplayName = "Ocr Completed")]
public class OcrCompletedActivity : CodeActivity<string>
{
    internal const string InputKey = "OcrCompleted";
    /// <summary>
    /// The message type to receive.
    /// </summary>
    public Type MessageType { get; set; } = default!;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var bookmark = context.CreateBookmark(new DispatchWorkflowBookmark(context.WorkflowExecutionContext.Id), OnResumeAsync, includeActivityInstanceId: false);
        await ValueTask.CompletedTask;
    }

    private async ValueTask OnResumeAsync(ActivityExecutionContext context)
    {
        Console.WriteLine("Resume Activity");

        if (context.TryGetWorkflowInput<object>(InputKey, out var message) && message is IDictionary<string, object> ocrCompletedDict)
        {
            var ocrCompleted = NewMethod(ocrCompletedDict);
            context.Set(Result, ocrCompleted);
        }
        else
        {
            Console.WriteLine($"Message received: {message}");
        }

        await context.CompleteActivityAsync();
    }

    private static OcrCompleted NewMethod(IDictionary<string, object> ocrCompletedDict)
    {
        var instanceId = ocrCompletedDict["InstanceId"] as string;
        var correlationId = Guid.Parse(ocrCompletedDict["CorrelationId"] as string);
        var ocrResult = ocrCompletedDict["Text"] as string;

        if (instanceId == null || correlationId == null || ocrResult == null)
        {
            Console.WriteLine("Invalid OcrCompleted message");
            return null;
        }

        return new OcrCompleted(instanceId, correlationId, ocrResult);
    }
}


