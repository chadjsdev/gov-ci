using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class CorrectionCount
    {
        [Key]
        public string AgencySlug { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
