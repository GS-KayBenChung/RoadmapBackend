﻿
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Domain.Dtos
{
    public class RoadmapUpdateDto
    {
        public RoadmapPatchDto Roadmap { get; set; }
        public List<MilestonePatchDto> Milestones { get; set; }
        public List<SectionPatchDto> Sections { get; set; }
        public List<TaskPatchDto> Tasks { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class RoadmapPatchDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class MilestonePatchDto
    {
        public Guid MilestoneId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid RoadmapId { get; set; }
        public bool IsDeleted { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class SectionPatchDto
    {
        public Guid SectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid MilestoneId { get; set; }
        public bool IsDeleted { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class TaskPatchDto
    {
        public Guid TaskId { get; set; }
        public string Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        public Guid MilestoneId { get; set; }
        public Guid SectionId { get; set; }
        public bool IsDeleted { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; } = new Dictionary<string, JsonElement>();
    }

}
