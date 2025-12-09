namespace SpecDocGenerator.Core.Models;

/// <summary>
/// Represents a source code file
/// </summary>
public class CodeFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ProgrammingLanguage Language { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public DateTime LastModified { get; set; }
    public long SizeInBytes { get; set; }
}
