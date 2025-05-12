using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class CorrectionReference
    {
        [Key]
        public int Id { get; set; }
        public int CorrectionId { get; set; }
        public string AgencySlug { get; set; } = string.Empty;
        public string CfrReference { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Chapter { get; set; }
        public string? SubChapter { get; set; }
        public string? Part { get; set; }
        public string? Subpart { get; set; }
        public string? Section { get; set; }
        public Correction? Correction { get; set; }
    }
}
