

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Dtos
    {
    public class CompletionStatusDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("progress")]
        public int? Progress { get; set; }

        [JsonPropertyName("isCompleted")]
        public bool? IsCompleted { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }
}

