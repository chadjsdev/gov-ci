namespace EcfrApp.DTOs
{
    public class TitleStructureDto
    {
        public int Title { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string LabelLevel { get; set; } = string.Empty;
        public string LabelDescription { get; set; } = string.Empty;
        public bool Reserved { get; set; }
        public long Size { get; set; }
        public List<string> Volumes { get; set; } = [];
        public string DescendantRange { get; set; } = string.Empty;
        public List<StructureNodeDto> Children { get; set; } = [];
    }

    public class StructureNodeDto
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string LabelLevel { get; set; } = string.Empty;
        public string LabelDescription { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Reserved { get; set; }
        public long Size { get; set; }
        public List<string> Volumes { get; set; } = [];
        public string? ReceivedOn { get; set; }
        public string DescendantRange { get; set; } = string.Empty;
        public bool GeneratedId { get; set; }
        public List<StructureNodeDto> Children { get; set; } = [];
    }
}