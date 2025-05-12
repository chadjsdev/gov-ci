using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class Agency
    {
        [Key]
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SortableName { get; set; } = string.Empty;
        public List<CfrReference> CfrReferences { get; set; } = [];
        public List<Agency> Children { get; set; } = [];
    }
}
