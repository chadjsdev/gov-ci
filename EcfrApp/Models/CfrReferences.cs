namespace EcfrApp.Models
{
    public class CfrReference
    {
        public int Id { get; set; }
        public string? AgencySlug { get; set; }
        public int Title { get; set; }
        public string? Chapter { get; set; }
        public string? Subchapter { get; set; }
        public string? Subpart { get; set; }
        public string? Part { get; set; }
        public string? Section { get; set; } 
        public string? Appendix { get; set; } 
        public required Agency Agency { get; set; }
    }
}