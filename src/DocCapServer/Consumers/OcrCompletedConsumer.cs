using DocCap.Contracts;
using DocCapServer.Activities;
using Elsa.Workflows.Helpers;
using Elsa.Workflows.Runtime.Bookmarks;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Models;
using MassTransit;


namespace DocCapServer.Consumers;

public class OcrCompletedConsumer : IConsumer<OcrCompleted>
{
    private readonly IWorkflowInbox _workflowInbox;

    public OcrCompletedConsumer(IWorkflowInbox workflowInbox)
    {
        _workflowInbox = workflowInbox;
    }

    public async Task Consume(ConsumeContext<OcrCompleted> context)
    {
        var message = new NewWorkflowInboxMessage
        {            
            ActivityTypeName = ActivityTypeNameHelper.GenerateTypeName<OcrCompletedActivity>(),
            BookmarkPayload = new DispatchWorkflowBookmark(context.Message.InstanceId),
            WorkflowInstanceId = context.Message.InstanceId,
            Input = new Dictionary<string, object>
            {
                {OcrCompletedActivity.InputKey, context.Message}       
            }
        };

        var result = await _workflowInbox.SubmitAsync(message);
        return;
    }
}

public class OcrCompletedConsumerDefinition : ConsumerDefinition<OcrCompletedConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<OcrCompletedConsumer> consumerConfigurator)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ClearMessageDeserializers();
        endpointConfigurator.UseRawJsonSerializer();
    }
}