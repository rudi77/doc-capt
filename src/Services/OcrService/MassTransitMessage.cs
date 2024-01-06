using System.Collections.Generic;

public class MassTransitMessage<TMessage>
{
    // decorate properties with attributes to match MassTransit message headers
    [System.Text.Json.Serialization.JsonPropertyName("messageId")]
    public string RequestId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("initiatorId")]
    public string InitiatorId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("sourceAddress")]
    public string SourceAddress { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("messageType")]
    public List<string> MessageType { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public TMessage Message { get; set; }
}
