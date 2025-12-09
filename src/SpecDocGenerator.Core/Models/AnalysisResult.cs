namespace SpecDocGenerator.Core.Models;

/// <summary>
/// Result of code analysis
/// </summary>
public class AnalysisResult
{
    public List<CodeFile> Files { get; set; } = new();
    public List<CodeElement> Elements { get; set; } = new();
    public Dictionary<string, List<string>> Dependencies { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
