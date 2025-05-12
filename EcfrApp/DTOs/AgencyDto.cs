namespace EcfrApp.DTOs;

public class AgencyDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SortableName { get; set; } = string.Empty;
    public List<CfrReferenceDto> CfrReferences { get; set; } = new();
    public List<AgencyDto> Children { get; set; } = new();
}

public class CfrReferenceDto
{
    public int Id { get; set; }
    public string AgencySlug { get; set; } = string.Empty;
    public int Title { get; set; }
    public string Chapter { get; set; } = string.Empty;
}
