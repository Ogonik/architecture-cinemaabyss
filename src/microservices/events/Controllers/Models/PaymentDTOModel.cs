using System.Numerics;
using System.Text.Json.Serialization;

namespace EventsKafkaTest.Controllers.Models
{
    public class PaymentDTOModel
    {
        [JsonPropertyName("payment_id")]
        public required int PaymentId { get; set; }

        [JsonPropertyName("user_id")]
        public required int UserId { get; set; }

        [JsonPropertyName("amount")]
        public float Amount { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("timestamp")]
        public required DateTime Timestamp { get; set; }

        [JsonPropertyName("method_type")]
        public required string MethodType { get; set; }

    } 
}
