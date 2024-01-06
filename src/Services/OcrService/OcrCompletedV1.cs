public class OcrCompletedV1
{
    public Guid CorrelationId { get; set; }
    
    /// <summary>
    /// The text extracted from the document.
    /// </summary>
    public string Text { get; set; }
}