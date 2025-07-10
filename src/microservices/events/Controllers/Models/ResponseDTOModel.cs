using System.Numerics;
using System.Text.Json.Serialization;

namespace EventsKafkaTest.Controllers.Models
{
    public class ResponseDTOModel
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        
        [JsonPropertyName("partition")]
        public required int Partition { get; set; }
        
        [JsonPropertyName("offset")]
        public required long Offset { get; set; }

        [JsonPropertyName("event")]
        public required ResponseEvent Event { get; set; }
    }

    public class ResponseEvent(int entityId, string entityType)
    {
        private readonly int _entityId = entityId;
        private readonly string _entityType = entityType;

        [JsonPropertyName("id")]
        public string Id => $"{_entityType}-{_entityId}-viewed";

        [JsonPropertyName("type")]
        public string Type => _entityType;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp => DateTime.Now;

        [JsonPropertyName("payload")]
        public object? Payload => new object();
    }
}
