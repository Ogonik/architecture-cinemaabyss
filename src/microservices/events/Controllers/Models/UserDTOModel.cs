using System.Numerics;
using System.Text.Json.Serialization;

namespace EventsKafkaTest.Controllers.Models
{
    public record UserDTOModel
    {  
        [JsonPropertyName("user_id")]
        public required int UserId { get; set; }

        [JsonPropertyName("timestamp")]
        public required DateTime Timestamp { get; set; }

        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("action")]
        public required string Action { get; set; }

        [JsonPropertyName("email")]
        public required string Email { get; set; }
    }
}
