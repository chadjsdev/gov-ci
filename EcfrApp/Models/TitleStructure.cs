using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class TitleStructure
    {
        [Key]
        public int Title { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string LabelLevel { get; set; } = string.Empty;
        public string LabelDescription { get; set; } = string.Empty;
        public bool Reserved { get; set; }
        public long Size { get; set; } // Emphasized metadata
        public List<string> Volumes { get; set; } = [];
        public string DescendantRange { get; set; } = string.Empty;
        public List<StructureNode> Children { get; set; } = [];
    }

    public class StructureNode
    {
        [Key]
        public int Id { get; set; }
        public int? TitleStructureId { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string LabelLevel { get; set; } = string.Empty;
        public string LabelDescription { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Reserved { get; set; }
        public long Size { get; set; } // Emphasized metadata
        public List<string> Volumes { get; set; } = [];
        public string? ReceivedOn { get; set; } // Nullable for nodes without received_on
        public string DescendantRange { get; set; } = string.Empty;
        public bool GeneratedId { get; set; } // For subject groups
        public List<StructureNode> Children { get; set; } = [];
        public TitleStructure? TitleStructure { get; set; }
    }
}