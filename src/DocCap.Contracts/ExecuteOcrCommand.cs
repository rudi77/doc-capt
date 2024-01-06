namespace DocCap.Contracts;

/// <summary>
/// Command to execute OCR on a document.
/// </summary>
public class ExecuteOcrCommand
{
    public ExecuteOcrCommand(string instanceId, Guid correlationId, byte[] documentData)        
    {
        InstanceId = instanceId;
        CorrelationId = correlationId;
        DocumentData = documentData;
    }

    /// <summary>
    /// The instance id of the Workflow instance.
    /// </summary>
    public string InstanceId { get; set; }

    public Guid CorrelationId { get; set; }

    /// <summary>
    /// The document data to send to the OCR service.
    /// </summary>
    public byte[] DocumentData { get; set;}
}
