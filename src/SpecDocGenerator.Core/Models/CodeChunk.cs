namespace SpecDocGenerator.Core.Models;

/// <summary>
/// Represents a chunk of code for LLM processing
/// </summary>
public class CodeChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public ProgrammingLanguage Language { get; set; }
    public List<string> RelatedElements { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
