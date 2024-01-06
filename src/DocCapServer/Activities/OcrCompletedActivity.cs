using DocCap.Contracts;
using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.Workflows;

namespace DocCapServer.Activities;

[Elsa.Workflows.Attributes.Activity("Bludelta", 
    "Ocr", "Waits for a OcrCompleted message", DisplayName = "Ocr Completed")]
public class OcrCompletedActivity : Trigger<OcrCompleted>
{
    internal const string InputKey = "OcrCompleted";
    /// <summary>
    /// The message type to receive.
    /// </summary>
    public Type MessageType { get; set; } = default!;

    /// <inheritdoc />
    protected override object GetTriggerPayload(TriggerIndexingContext context) => GetBookmarkPayload(context.ExpressionExecutionContext);

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // If we did not receive external input, it means we are just now encountering this activity and we need to block execution by creating a bookmark.
        if (!context.TryGetWorkflowInput<object>(InputKey, out var message))
        {
            // Create bookmarks for when we receive the expected HTTP request.
            context.CreateBookmark(GetBookmarkPayload(context.ExpressionExecutionContext));
            return;
        }

        // Provide the received message as output.
        context.Set(Result, message);
        
        // Complete.
        await context.CompleteActivityAsync();
    }

    private object GetBookmarkPayload(ExpressionExecutionContext context)
    {
        // Generate bookmark data for message type.
        return new OcrBookmarkPayload(Guid.NewGuid(), MessageType);
    }
}
internal record OcrBookmarkPayload(Guid CorrelationId, Type MessageType);
