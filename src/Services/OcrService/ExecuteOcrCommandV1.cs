namespace OcrService;

public class ExecuteOcrCommandV1
{
    [System.Text.Json.Serialization.JsonPropertyName("correlationId")]
    public Guid CorrelationId { get; set; }
    
    /// <summary>
    /// The document data to send to the OCR service.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("documentData")]
    public byte[] DocumentData { get; set;}
}