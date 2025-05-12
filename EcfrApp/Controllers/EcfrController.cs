using EcfrApp.Data;
using EcfrApp.Models;
using EcfrApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace EcfrApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EcfrController : ControllerBase
    {
        private readonly EcfrContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://www.ecfr.gov/api";

        public EcfrController(EcfrContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("agency-sizes")]
        public async Task<IActionResult> GetAgencySizes()
        {
            try
            {
                var agencies = await _context.Agencies
                    .Include(a => a.CfrReferences)
                    .Include(a => a.Children)
                    .OrderBy(a => a.SortableName)
                    .ToListAsync();

                var flatAgencies = agencies.SelectMany(a => a.Children.Prepend(a)).ToList();
                var titleStructures = await _context.TitleStructures
                    .Include(ts => ts.Children)
                    .ToListAsync();

                // Fetch correction counts for each agency
                var correctionCounts = _context.CorrectionCounts.ToDictionary(cc => cc.AgencySlug, cc => cc.Count);

                var agencySizes = flatAgencies.Select(agency =>
                {
                    long totalSize = 0;
                    foreach (var cfrRef in agency.CfrReferences)
                    {
                        var titleStructure = titleStructures.FirstOrDefault(ts => ts.Title == cfrRef.Title);
                        if (titleStructure != null)
                        {
                            var matchingNodes = titleStructure.Children
                                .Where(n => (n.Type == "part" && cfrRef.Part == n.Identifier) ||
                                            (n.Type == "subpart" && cfrRef.Subpart == n.Identifier) ||
                                            (n.Type == "subchapter" && cfrRef.Subchapter == n.Identifier) ||
                                            (n.Type == "chapter" && cfrRef.Chapter == n.Identifier) ||
                                            (n.Type == "section" && cfrRef.Section == n.Identifier) ||
                                            (n.Type == "appendix" && cfrRef.Appendix == n.Identifier))
                                .ToList();
                            totalSize += matchingNodes.Sum(c => c.Size);
                        }
                    }

                    long wordCount = totalSize / 2;

                    var dto = new AgencySizeDto
                    {
                        Slug = agency.Slug,
                        Name = agency.Name,
                        ShortName = agency.ShortName,
                        DisplayName = agency.DisplayName,
                        TotalSize = totalSize,
                        WordCount = wordCount,
                        CfrReferences = agency.CfrReferences.Select(r => new CfrReferenceDto
                        {
                            Id = r.Id,
                            AgencySlug = r.AgencySlug ?? string.Empty, // Handle null values
                            Title = r.Title,
                            Chapter = r.Chapter ?? string.Empty // Handle null values
                        }).ToList(),
                        CorrectionCount = correctionCounts.ContainsKey(agency.Slug) ? correctionCounts[agency.Slug] : 0
                    };
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var json = JsonSerializer.Serialize(dto, options);
                    // Replace \u0027 with ' in the JSON string before calculating the checksum
                    json = json.Replace("\\u0027", "'");
                    json = json.Replace("\\u0026", "'");
                    dto.Checksum = Encoding.UTF8.GetByteCount(json);
                    json = JsonSerializer.Serialize(dto, options);
                    json = json.Replace("\\u0027", "'");
                    json = json.Replace("\\u0026", "'");
                    dto.Checksum = Encoding.UTF8.GetByteCount(json);

                    return dto;
                }).ToList();

                return Ok(agencySizes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calculating agency sizes: {ex.Message}");
            }
        }

        [HttpGet("fetch-agencies")]
        public async Task<IActionResult> FetchAgencies()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("EcfrClient");
                var response = await client.GetAsync($"{_baseUrl}/admin/v1/agencies.json");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var agencies = doc.RootElement.GetProperty("agencies").EnumerateArray()
                    .Select(a => ParseAgency(a)).ToList();

                const int batchSize = 50;
                for (int i = 0; i < agencies.Count; i += batchSize)
                {
                    var batch = agencies.Skip(i).Take(batchSize).ToList();
                    foreach (var agency in batch)
                    {
                        var existingAgency = await _context.Agencies
                            .Include(a => a.CfrReferences)
                            .FirstOrDefaultAsync(a => a.Slug == agency.Slug);

                        if (existingAgency != null)
                        {
                            _context.CfrReferences.RemoveRange(existingAgency.CfrReferences);
                            existingAgency.Name = agency.Name;
                            existingAgency.ShortName = agency.ShortName;
                            existingAgency.DisplayName = agency.DisplayName;
                            existingAgency.SortableName = agency.SortableName;
                            existingAgency.CfrReferences = agency.CfrReferences;
                        }
                        else
                        {
                            _context.Agencies.Add(agency);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(agencies.Count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching agencies: {ex.Message}");
            }
        }

        [HttpGet("fetch-corrections")]
        public async Task<IActionResult> FetchCorrections([FromQuery] string? title = null, [FromQuery] string? date = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}/admin/v1/corrections.json";
                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(date))
                {
                    url += "?";
                    if (!string.IsNullOrEmpty(title)) url += $"title={title}&";
                    if (!string.IsNullOrEmpty(date)) url += $"date={date}";
                }

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var corrections = doc.RootElement.GetProperty("ecfr_corrections").EnumerateArray()
                    .Select(c => ParseCorrection(c)).ToList();

                foreach (var correction in corrections)
                {
                    var existingCorrection = await _context.Corrections
                        .Include(c => c.CfrReferences)
                        .FirstOrDefaultAsync(c => c.Id == correction.Id);

                    if (existingCorrection != null)
                    {
                        _context.CorrectionReferences.RemoveRange(existingCorrection.CfrReferences);
                        existingCorrection.Title = correction.Title;
                        existingCorrection.CorrectiveAction = correction.CorrectiveAction;
                        existingCorrection.ErrorCorrected = correction.ErrorCorrected;
                        existingCorrection.ErrorOccurred = correction.ErrorOccurred;
                        existingCorrection.FrCitation = correction.FrCitation;
                        existingCorrection.Position = correction.Position;
                        existingCorrection.DisplayInToc = correction.DisplayInToc;
                        existingCorrection.Year = correction.Year;
                        existingCorrection.LastModified = correction.LastModified;
                        existingCorrection.CfrReferences = correction.CfrReferences;
                    }
                    else
                    {
                        _context.Corrections.Add(correction);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(corrections.Count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching corrections: {ex.Message}");
            }
        }

        [HttpGet("fetch-title-relationships")]
        public async Task<IActionResult> FetchTitleRelationships([FromQuery] int title)
        {
            if (title <= 0)
            {
                return BadRequest("A valid title number must be provided.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("EcfrClient");

                var titlesResponse = await client.GetAsync($"{_baseUrl}/versioner/v1/titles.json");
                titlesResponse.EnsureSuccessStatusCode();
                var titlesJson = await titlesResponse.Content.ReadAsStringAsync();
                using var titlesDoc = JsonDocument.Parse(titlesJson);

                string currentDate = string.Empty;
                if (titlesDoc.RootElement.TryGetProperty("titles", out var titlesArray) &&
                    titlesArray.ValueKind == JsonValueKind.Array)
                {
                    var firstTitle = titlesArray.EnumerateArray().FirstOrDefault();
                    if (firstTitle.ValueKind != JsonValueKind.Undefined &&
                        firstTitle.TryGetProperty("up_to_date_as_of", out var dateProp))
                    {
                        currentDate = dateProp.GetString() ?? string.Empty;
                    }
                }

                currentDate ??= DateTime.UtcNow.ToString("yyyy-MM-dd");

                var nodesProcessed = 0;

                var structureUrl = $"{_baseUrl}/versioner/v1/structure/{currentDate}/title-{title}.json";
                var structureResponse = await client.GetAsync(structureUrl);
                if (!structureResponse.IsSuccessStatusCode)
                {
                    return NotFound($"Title {title} not found.");
                }

                var structureJson = await structureResponse.Content.ReadAsStringAsync();
                using var structureDoc = JsonDocument.Parse(structureJson);

                var titleStructure = ParseTitleStructure(structureDoc.RootElement, title);

                var existingTitle = await _context.TitleStructures
                    .Include(ts => ts.Children)
                    .FirstOrDefaultAsync(ts => ts.Title == title);

                if (existingTitle != null)
                {
                    _context.StructureNodes.RemoveRange(existingTitle.Children);
                    existingTitle.Identifier = titleStructure.Identifier;
                    existingTitle.Label = titleStructure.Label;
                    existingTitle.LabelLevel = titleStructure.LabelLevel;
                    existingTitle.LabelDescription = titleStructure.LabelDescription;
                    existingTitle.Reserved = titleStructure.Reserved;
                    existingTitle.Size = titleStructure.Size;
                    existingTitle.Volumes = titleStructure.Volumes;
                    existingTitle.DescendantRange = titleStructure.DescendantRange;
                    existingTitle.Children = titleStructure.Children.Select(c => AssignTitleStructureId(c, existingTitle.Title)).ToList();
                }
                else
                {
                    await _context.TitleStructures.AddAsync(titleStructure);
                    await _context.SaveChangesAsync();
                    titleStructure.Children = titleStructure.Children.Select(c => AssignTitleStructureId(c, titleStructure.Title)).ToList();
                }

                await _context.SaveChangesAsync();

                nodesProcessed += 1 + titleStructure.Children.Count;
                foreach (var child in titleStructure.Children)
                {
                    nodesProcessed += CountChildNodes(child);
                }

                return Ok(nodesProcessed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching title relationships: {ex.Message}");
            }
        }

        [HttpGet("fetch-all-title-relationships")]
        public async Task<IActionResult> FetchAllTitleRelationships()
        {
            var results = new List<object>();

            for (int title = 1; title <= 50; title++)
            {
                var response = await FetchTitleRelationships(title);
                if (response is OkObjectResult okResult)
                {
                    results.Add(new { Title = title, Data = okResult.Value });
                }
                else if (response is ObjectResult errorResult)
                {
                    results.Add(new { Title = title, Error = errorResult.Value });
                }

                // Pause for 30 seconds to avoid overloading the API
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            return Ok(results);
        }

        [HttpPost("populate-correction-counts")]
        public async Task<IActionResult> PopulateCorrectionCounts()
        {
            try
            {
                // Add Agencies to CorrectionReferences
                ProcessCorrectionReferences();
                // Fetch and populate correction counts
                var correctionCounts = await _context.CorrectionReferences
                    .Where(cr => !string.IsNullOrEmpty(cr.AgencySlug))
                    .GroupBy(cr => cr.AgencySlug)
                    .Select(g => new CorrectionCount
                    {
                        AgencySlug = g.Key ?? "Unknown",
                        Count = g.Select(cr => cr.CorrectionId).Distinct().Count()
                    })
                    .ToListAsync();

                _context.CorrectionCounts.RemoveRange(_context.CorrectionCounts);
                await _context.CorrectionCounts.AddRangeAsync(correctionCounts);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Correction counts populated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error populating correction counts: {ex.Message}");
            }
        }

        private StructureNode AssignTitleStructureId(StructureNode node, int titleStructureId)
        {
            node.TitleStructureId = titleStructureId;
            node.Children = node.Children.Select(c => AssignTitleStructureId(c, titleStructureId)).ToList();
            return node;
        }

        private int CountChildNodes(StructureNode node)
        {
            int count = node.Children.Count;
            foreach (var child in node.Children)
            {
                count += CountChildNodes(child);
            }
            return count;
        }

        private Agency ParseAgency(JsonElement element)
        {
            var agency = new Agency
            {
                Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                ShortName = element.TryGetProperty("short_name", out var shortNameProp) ? shortNameProp.GetString() ?? string.Empty : string.Empty,
                DisplayName = element.TryGetProperty("display_name", out var displayNameProp) ? displayNameProp.GetString() ?? string.Empty : string.Empty,
                SortableName = element.TryGetProperty("sortable_name", out var sortableNameProp) ? sortableNameProp.GetString() ?? string.Empty : string.Empty,
                Slug = element.TryGetProperty("slug", out var slugProp) ? slugProp.GetString() ?? string.Empty : string.Empty,
                CfrReferences = element.TryGetProperty("cfr_references", out var cfrReferencesProp) && cfrReferencesProp.ValueKind == JsonValueKind.Array
                    ? cfrReferencesProp.EnumerateArray().Select(r => new CfrReference
                    {
                        Title = r.TryGetProperty("title", out var titleProp) ? titleProp.GetInt32() : 0,
                        Chapter = r.TryGetProperty("chapter", out var chapterProp) ? chapterProp.GetString() ?? string.Empty : string.Empty,
                        Subchapter = r.TryGetProperty("subchapter", out var subchapterProp) ? subchapterProp.GetString() : null,
                        Subpart = r.TryGetProperty("subpart", out var subpartProp) ? subpartProp.GetString() : null,
                        Part = r.TryGetProperty("part", out var partProp) ? partProp.GetString() : null,
                        Section = r.TryGetProperty("section", out var sectionProp) ? sectionProp.GetString() : null,
                        Appendix = r.TryGetProperty("appendix", out var appendixProp) ? appendixProp.GetString() : null,
                        AgencySlug = element.TryGetProperty("slug", out var agencySlugProp) ? agencySlugProp.GetString() ?? string.Empty : string.Empty,
                        Agency = new Agency
                        {
                            Slug = element.TryGetProperty("slug", out var slugProp) ? slugProp.GetString() ?? string.Empty : string.Empty,
                            Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty
                        }
                    }).ToList()
                    : new List<CfrReference>(),
                Children = element.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array
                    ? childrenProp.EnumerateArray().Select(c => ParseAgency(c)).ToList()
                    : new List<Agency>()
            };
            return agency;
        }

        private Correction ParseCorrection(JsonElement element)
        {
            var correction = new Correction
            {
                Id = element.GetProperty("id").GetInt32(),
                Title = element.GetProperty("title").GetInt32(),
                CorrectiveAction = element.GetProperty("corrective_action").GetString() ?? string.Empty,
                ErrorCorrected = DateTime.Parse(element.GetProperty("error_corrected").GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                ErrorOccurred = DateTime.Parse(element.GetProperty("error_occurred").GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                FrCitation = element.GetProperty("fr_citation").GetString() ?? string.Empty,
                Position = element.GetProperty("position").GetInt32(),
                DisplayInToc = element.GetProperty("display_in_toc").GetBoolean(),
                Year = element.GetProperty("year").GetInt32(),
                LastModified = DateTime.Parse(element.GetProperty("last_modified").GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                CfrReferences = element.GetProperty("cfr_references").EnumerateArray()
                    .Select(r => new CorrectionReference
                    {
                        CfrReference = r.GetProperty("cfr_reference").GetString() ?? string.Empty,
                        Title = r.GetProperty("hierarchy").TryGetProperty("title", out var t) ? t.GetString() : null,
                        Subtitle = r.GetProperty("hierarchy").TryGetProperty("subtitle", out var s) ? s.GetString() : null,
                        Part = r.GetProperty("hierarchy").TryGetProperty("part", out var p) ? p.GetString() : null,
                        Subpart = r.GetProperty("hierarchy").TryGetProperty("subpart", out var sp) ? sp.GetString() : null,
                        Section = r.GetProperty("hierarchy").TryGetProperty("section", out var sec) ? sec.GetString() : null
                    }).ToList()
            };
            return correction;
        }

        private TitleStructure ParseTitleStructure(JsonElement element, int title)
        {
            var titleStructure = new TitleStructure
            {
                Title = title,
                Identifier = element.TryGetProperty("identifier", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty,
                Label = element.TryGetProperty("label", out var labelProp) ? labelProp.GetString() ?? string.Empty : $"Title {title}",
                LabelLevel = element.TryGetProperty("label_level", out var levelProp) ? levelProp.GetString() ?? string.Empty : string.Empty,
                LabelDescription = element.TryGetProperty("label_description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                Reserved = element.TryGetProperty("reserved", out var reservedProp) && reservedProp.GetBoolean(),
                Size = element.TryGetProperty("size", out var sizeProp) ? sizeProp.GetInt64() : 0,
                Volumes = element.TryGetProperty("volumes", out var volumesProp) && volumesProp.ValueKind == JsonValueKind.Array
                    ? volumesProp.EnumerateArray().Select(v => v.GetString() ?? string.Empty).ToList()
                    : new List<string>(),
                DescendantRange = element.TryGetProperty("descendant_range", out var rangeProp) ? rangeProp.GetString() ?? string.Empty : string.Empty,
                Children = element.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array
                    ? childrenProp.EnumerateArray().Select(c => ParseStructureNode(c)).ToList()
                    : new List<StructureNode>()
            };
            return titleStructure;
        }

        private StructureNode ParseStructureNode(JsonElement element)
        {
            var node = new StructureNode
            {
                Identifier = element.TryGetProperty("identifier", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty,
                Label = element.TryGetProperty("label", out var nodeLabelProp) ? nodeLabelProp.GetString() ?? string.Empty : string.Empty,
                LabelLevel = element.TryGetProperty("label_level", out var levelProp) ? levelProp.GetString() ?? string.Empty : string.Empty,
                LabelDescription = element.TryGetProperty("label_description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                Type = element.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty,
                Reserved = element.TryGetProperty("reserved", out var reservedProp) && reservedProp.GetBoolean(),
                Size = element.TryGetProperty("size", out var sizeProp) ? sizeProp.GetInt64() : 0,
                Volumes = element.TryGetProperty("volumes", out var volumesProp) && volumesProp.ValueKind == JsonValueKind.Array
                    ? volumesProp.EnumerateArray().Select(v => v.GetString() ?? string.Empty).ToList()
                    : new List<string>(),
                ReceivedOn = element.TryGetProperty("received_on", out var receivedProp) ? receivedProp.GetString() : null,
                DescendantRange = element.TryGetProperty("descendant_range", out var rangeProp) ? rangeProp.GetString() ?? string.Empty : string.Empty,
                GeneratedId = element.TryGetProperty("generated_id", out var genIdProp) && genIdProp.GetBoolean(),
                Children = element.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array
                    ? childrenProp.EnumerateArray().Select(c => ParseStructureNode(c)).ToList()
                    : new List<StructureNode>()
            };
            return node;
        }

        private List<Agency> GetAssociatedAgencies(int title, string? subtitle, string? chapter, string? subchapter, string? part, string? subpart, string? section)
        {
            var query = _context.StructureNodes.AsQueryable();


            if (!string.IsNullOrEmpty(chapter))
                query = query.Where(sn => sn.Identifier == chapter);

            if (!string.IsNullOrEmpty(subchapter))
                query = query.Where(sn => sn.Identifier == subchapter);

            if (!string.IsNullOrEmpty(part))
                query = query.Where(sn => sn.Identifier == part);

            if (!string.IsNullOrEmpty(subpart))
                query = query.Where(sn => sn.Identifier == subpart);

            if (!string.IsNullOrEmpty(section))
                query = query.Where(sn => sn.Identifier == section);

            var structureNodes = query.ToList();

            var associatedAgencies = structureNodes
                .SelectMany(sn => _context.Agencies.Where(a => a.CfrReferences.Any(cr => cr.Title == title && cr.Part == sn.Identifier)))
                .Distinct()
                .ToList();

            return associatedAgencies;
        }

        private void ProcessCorrectionReferences()
        {
            var correctionReferences = _context.CorrectionReferences.ToList();

            foreach (var correctionReference in correctionReferences)
            {
                if (int.TryParse(correctionReference.Title, out int title))
                {
                    var associatedAgencies = GetAssociatedAgencies(
                        title,
                        null, // Subtitle is not used in this example
                        correctionReference.Chapter,
                        correctionReference.SubChapter,
                        correctionReference.Part,
                        correctionReference.Subpart,
                        correctionReference.Section
                    );

                    if (associatedAgencies.Any())
                    {
                        // Write back the first found agency's slug to the CorrectionReferences table
                        correctionReference.AgencySlug = associatedAgencies.First().Slug;
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid Title value: {correctionReference.Title}");
                }
            }

            // Save changes to the database
            _context.SaveChanges();
        }
    }
}