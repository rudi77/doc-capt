using DocCap.Contracts;
using DocCapServer.Activities;
using Elsa.Workflows.Helpers;
using Elsa.Workflows.Runtime.Bookmarks;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Models;
using Elsa.Workflows.Runtime.Options;
using Elsa.Workflows.Runtime.Requests;
using MassTransit;


namespace DocCapServer.Consumers;

public class OcrCompletedConsumer : IConsumer<OcrCompleted>
{
    //private readonly IWorkflowDispatcher _workflowRuntime;

    // public OcrCompletedConsumer(IWorkflowDispatcher workflowRuntime)
    // {
    //     _workflowRuntime = workflowRuntime;
    // }

    private readonly IWorkflowInbox _workflowInbox;

    public OcrCompletedConsumer(IWorkflowInbox workflowInbox)
    {
        _workflowInbox = workflowInbox;
    }

    public async Task Consume(ConsumeContext<OcrCompleted> context)
    {
        // var correlationId = context.CorrelationId?.ToString();
        // var cancellationToken = context.CancellationToken;
        // var messageType = typeof(OcrCompleted);
        // var message = context.Message;
        // var activityTypeName = ActivityTypeNameHelper.GenerateTypeName(typeof(OcrCompletedActivity));

        // var bookmark = new OcrBookmarkPayload(context.CorrelationId.Value, messageType);
        // var input = new Dictionary<string, object> { [nameof(OcrCompleted)] = message };
        // var request = new DispatchTriggerWorkflowsRequest(activityTypeName, bookmark)
        // {
        //     CorrelationId = correlationId,
        //     Input = input
        // };
        // var result = await _workflowRuntime.DispatchAsync(request, cancellationToken);
        // return;

            var message = new NewWorkflowInboxMessage
            {
                CorrelationId = context.Message.CorrelationId.ToString(),
                ActivityTypeName = ActivityTypeNameHelper.GenerateTypeName<OcrCompletedActivity>(),
                //BookmarkPayload = new DispatchWorkflowBookmark(context.Message.CorrelationId.ToString()),
                Input = new Dictionary<string, object>
                {
                    {OcrCompletedActivity.InputKey, context.Message}       
                }
            };

            await _workflowInbox.SubmitAsync(message);

    }
}