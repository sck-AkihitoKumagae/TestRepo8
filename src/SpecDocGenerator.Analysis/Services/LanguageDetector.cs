using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;

namespace SpecDocGenerator.Analysis.Services;

/// <summary>
/// Language detection service based on file extensions
/// </summary>
public class LanguageDetector : ILanguageDetector
{
    private static readonly Dictionary<string, ProgrammingLanguage> ExtensionMap = new()
    {
        { ".cs", ProgrammingLanguage.CSharp },
        { ".py", ProgrammingLanguage.Python },
        { ".js", ProgrammingLanguage.JavaScript },
        { ".jsx", ProgrammingLanguage.JavaScript },
        { ".ts", ProgrammingLanguage.TypeScript },
        { ".tsx", ProgrammingLanguage.TypeScript },
        { ".java", ProgrammingLanguage.Java },
        { ".cpp", ProgrammingLanguage.Cpp },
        { ".cc", ProgrammingLanguage.Cpp },
        { ".cxx", ProgrammingLanguage.Cpp },
        { ".hpp", ProgrammingLanguage.Cpp },
        { ".h", ProgrammingLanguage.C },
        { ".c", ProgrammingLanguage.C },
        { ".go", ProgrammingLanguage.Go },
        { ".rs", ProgrammingLanguage.Rust },
        { ".rb", ProgrammingLanguage.Ruby },
        { ".php", ProgrammingLanguage.PHP },
        { ".swift", ProgrammingLanguage.Swift },
        { ".kt", ProgrammingLanguage.Kotlin },
        { ".kts", ProgrammingLanguage.Kotlin },
        { ".scala", ProgrammingLanguage.Scala },
        { ".html", ProgrammingLanguage.HTML },
        { ".htm", ProgrammingLanguage.HTML },
        { ".css", ProgrammingLanguage.CSS },
        { ".sql", ProgrammingLanguage.SQL }
    };

    private static readonly HashSet<string> ExcludedDirectories = new()
    {
        "node_modules", "bin", "obj", "dist", "build", ".git", ".svn", 
        "vendor", "packages", "__pycache__", ".venv", "venv", "target"
    };

    public ProgrammingLanguage DetectLanguage(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return ExtensionMap.TryGetValue(extension, out var language) 
            ? language 
            : ProgrammingLanguage.Unknown;
    }

    public bool IsCodeFile(string filePath)
    {
        // Check if file is in excluded directory
        var parts = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Any(p => ExcludedDirectories.Contains(p)))
        {
            return false;
        }

        // Check if it's a known code file extension
        var language = DetectLanguage(filePath);
        return language != ProgrammingLanguage.Unknown;
    }
}
