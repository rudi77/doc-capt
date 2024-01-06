namespace DocCap.Contracts;

public class OcrCompleted(string instanceId, Guid correlationId, string text)
{
    /// <summary>
    /// The instance id of the Workflow instance.
    /// </summary>
    public string InstanceId { get; set; } = instanceId;


    public Guid CorrelationId { get; set; } = correlationId;

    /// <summary>
    /// The text extracted from the document.
    /// </summary>
    public string Text { get; set; } = text;
}
