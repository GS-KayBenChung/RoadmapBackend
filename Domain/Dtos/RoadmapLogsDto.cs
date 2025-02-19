using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class RoadmapLogsDto
    {
        //[JsonPropertyName("logId")]
        public Guid LogId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("activityAction")]
        public string ActivityAction { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
    }
}
