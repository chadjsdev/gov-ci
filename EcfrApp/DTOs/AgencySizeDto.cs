namespace EcfrApp.DTOs
{
    public class AgencySizeDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public long WordCount { get; set; }
        public long Checksum { get; set; }
        public long CorrectionCount { get; set; } 
        public List<CfrReferenceDto> CfrReferences { get; set; } = new List<CfrReferenceDto>();
    }
}