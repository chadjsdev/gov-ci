using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcfrApp.Models
{
    public class Correction
    {
        [Key]
        public int Id { get; set; }
        public int Title { get; set; }
        public string CorrectiveAction { get; set; } = string.Empty;
        public DateTime ErrorCorrected { get; set; }
        public DateTime ErrorOccurred { get; set; }
        public string FrCitation { get; set; } = string.Empty;
        public int Position { get; set; }
        public bool DisplayInToc { get; set; }
        public int Year { get; set; }
        public DateTime LastModified { get; set; }
        public List<CorrectionReference> CfrReferences { get; set; } = [];
    }
}
