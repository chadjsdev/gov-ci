namespace EcfrApp.DTOs;

public class CorrectionDto
{
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
    public List<CorrectionReferenceDto> CfrReferences { get; set; } = new();
}

public class CorrectionReferenceDto
{
    public string CfrReference { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Part { get; set; }
    public string? Subpart { get; set; }
    public string? Section { get; set; }
}
