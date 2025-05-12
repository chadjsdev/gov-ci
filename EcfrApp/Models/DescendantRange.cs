using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class DescendantRange
    {
        [Key]
        public int Id { get; set; }

        public int Title { get; set; }
        public string Chapter { get; set; } = string.Empty;
        public string Subchapter { get; set; } = string.Empty;
        public string Part { get; set; } = string.Empty;
        public string Subpart { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;

        public string RangeStart { get; set; } = string.Empty;
        public string RangeEnd { get; set; } = string.Empty;
    }
}
