//namespace Domain.Dtos
//{
//    public class CreateRoadmapDto
//    {
//        public string Title { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//        public bool? IsDraft { get; set; } 
//        public Guid CreatedBy { get; set; }
//        public DateTime? CreatedAt { get; set; } 
//        public int OverallDuration { get; set; }
//        public List<CreateMilestoneDto> Milestones { get; set; } = new();
//    }

//    public class CreateMilestoneDto
//    {
//        public string Name { get; set; }
//        public string Description { get; set; }
//        public List<CreateSectionDto> Sections { get; set; } = new();
//    }

//    public class CreateSectionDto
//    {
//        public string Name { get; set; }
//        public string Description { get; set; }
//        public List<CreateTaskDto> Tasks { get; set; } = new();
//    }

//    public class CreateTaskDto
//    {
//        public string Name { get; set; }
//        public DateTime DateStart { get; set; }
//        public DateTime DateEnd { get; set; }
//    }
//}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class CreateRoadmapDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool? IsDraft { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int OverallDuration { get; set; }
        public List<CreateMilestoneDto> Milestones { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class CreateMilestoneDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CreateSectionDto> Sections { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class CreateSectionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CreateTaskDto> Tasks { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class CreateTaskDto
    {
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }
}
