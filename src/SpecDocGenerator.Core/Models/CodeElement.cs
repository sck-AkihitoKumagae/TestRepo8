namespace SpecDocGenerator.Core.Models;

/// <summary>
/// Type of code element
/// </summary>
public enum CodeElementType
{
    Function,
    Method,
    Class,
    Interface,
    Struct,
    Enum,
    Property,
    Field,
    Module,
    Namespace
}

/// <summary>
/// Represents a parsed code element (function, class, etc.)
/// </summary>
public class CodeElement
{
    public string Name { get; set; } = string.Empty;
    public CodeElementType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public string Visibility { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}
