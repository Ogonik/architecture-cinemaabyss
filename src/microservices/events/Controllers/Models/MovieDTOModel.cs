using System.Numerics;
using System.Text.Json.Serialization;

namespace EventsKafkaTest.Controllers.Models
{
    public class MovieDTOModel
    {
        [JsonPropertyName("movie_id")]
        public required int MovieId { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("action")]
        public required string Action { get; set; }

        [JsonPropertyName("user_id")]
        public required int UserId { get; set; }

        [JsonPropertyName("rating")]
        public float Rating { get; set; }

        [JsonPropertyName("genres")]
        public List<string>? Genres { get; set; }       
    }
}
