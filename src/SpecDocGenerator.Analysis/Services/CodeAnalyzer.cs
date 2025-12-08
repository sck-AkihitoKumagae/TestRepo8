using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;
using System.Text.RegularExpressions;

namespace SpecDocGenerator.Analysis.Services;

/// <summary>
/// Basic code analyzer that extracts code structure using regex patterns
/// </summary>
public class CodeAnalyzer : ICodeAnalyzer
{
    private readonly ILanguageDetector _languageDetector;

    public CodeAnalyzer(ILanguageDetector languageDetector)
    {
        _languageDetector = languageDetector;
    }

    public async Task<AnalysisResult> AnalyzeAsync(List<CodeFile> files, CancellationToken cancellationToken = default)
    {
        var result = new AnalysisResult
        {
            Files = files
        };

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var elements = await AnalyzeFileAsync(file, cancellationToken);
            result.Elements.AddRange(elements);
        }

        // Extract dependencies
        ExtractDependencies(result);

        return result;
    }

    public Task<List<CodeChunk>> CreateChunksAsync(AnalysisResult analysisResult, int maxTokens = 4000)
    {
        var chunks = new List<CodeChunk>();
        var approximateCharsPerToken = 4; // Rough estimate
        var maxChars = maxTokens * approximateCharsPerToken;

        foreach (var file in analysisResult.Files)
        {
            var lines = file.Content.Split('\n');
            var currentChunk = new List<string>();
            var currentLength = 0;
            var startLine = 1;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                currentChunk.Add(line);
                currentLength += line.Length;

                // Create chunk if it reaches max size or it's the last line
                if (currentLength >= maxChars || i == lines.Length - 1)
                {
                    if (currentChunk.Count > 0)
                    {
                        chunks.Add(new CodeChunk
                        {
                            Content = string.Join('\n', currentChunk),
                            FilePath = file.FilePath,
                            StartLine = startLine,
                            EndLine = i + 1,
                            Language = file.Language,
                            Metadata = new Dictionary<string, string>
                            {
                                ["FileName"] = file.FileName
                            }
                        });
                    }

                    currentChunk.Clear();
                    currentLength = 0;
                    startLine = i + 2;
                }
            }
        }

        return Task.FromResult(chunks);
    }

    private Task<List<CodeElement>> AnalyzeFileAsync(CodeFile file, CancellationToken cancellationToken)
    {
        var elements = new List<CodeElement>();

        switch (file.Language)
        {
            case ProgrammingLanguage.CSharp:
                elements.AddRange(AnalyzeCSharp(file));
                break;
            case ProgrammingLanguage.Python:
                elements.AddRange(AnalyzePython(file));
                break;
            case ProgrammingLanguage.JavaScript:
            case ProgrammingLanguage.TypeScript:
                elements.AddRange(AnalyzeJavaScript(file));
                break;
            case ProgrammingLanguage.Java:
                elements.AddRange(AnalyzeJava(file));
                break;
            default:
                // Generic analysis for other languages
                elements.AddRange(AnalyzeGeneric(file));
                break;
        }

        return Task.FromResult(elements);
    }

    private List<CodeElement> AnalyzeCSharp(CodeFile file)
    {
        var elements = new List<CodeElement>();
        var lines = file.Content.Split('\n');

        // Simple regex patterns for C#
        var classPattern = new Regex(@"^\s*(public|private|protected|internal)?\s*(class|interface|struct|enum)\s+(\w+)", RegexOptions.Compiled);
        var methodPattern = new Regex(@"^\s*(public|private|protected|internal)?\s*\w+\s+(\w+)\s*\([^)]*\)", RegexOptions.Compiled);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Check for class/interface/struct/enum
            var classMatch = classPattern.Match(line);
            if (classMatch.Success)
            {
                var visibility = classMatch.Groups[1].Value;
                var type = classMatch.Groups[2].Value;
                var name = classMatch.Groups[3].Value;

                elements.Add(new CodeElement
                {
                    Name = name,
                    Type = type switch
                    {
                        "class" => CodeElementType.Class,
                        "interface" => CodeElementType.Interface,
                        "struct" => CodeElementType.Struct,
                        "enum" => CodeElementType.Enum,
                        _ => CodeElementType.Class
                    },
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Visibility = string.IsNullOrEmpty(visibility) ? "internal" : visibility,
                    IsPublic = visibility == "public"
                });
            }

            // Check for methods
            var methodMatch = methodPattern.Match(line);
            if (methodMatch.Success && !classMatch.Success)
            {
                var visibility = methodMatch.Groups[1].Value;
                var name = methodMatch.Groups[2].Value;

                elements.Add(new CodeElement
                {
                    Name = name,
                    Type = CodeElementType.Method,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    Visibility = string.IsNullOrEmpty(visibility) ? "private" : visibility,
                    IsPublic = visibility == "public"
                });
            }
        }

        return elements;
    }

    private List<CodeElement> AnalyzePython(CodeFile file)
    {
        var elements = new List<CodeElement>();
        var lines = file.Content.Split('\n');

        var classPattern = new Regex(@"^\s*class\s+(\w+)", RegexOptions.Compiled);
        var functionPattern = new Regex(@"^\s*def\s+(\w+)\s*\([^)]*\)", RegexOptions.Compiled);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var classMatch = classPattern.Match(line);
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    Name = classMatch.Groups[1].Value,
                    Type = CodeElementType.Class,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    IsPublic = !classMatch.Groups[1].Value.StartsWith("_")
                });
            }

            var functionMatch = functionPattern.Match(line);
            if (functionMatch.Success)
            {
                var name = functionMatch.Groups[1].Value;
                elements.Add(new CodeElement
                {
                    Name = name,
                    Type = CodeElementType.Function,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    IsPublic = !name.StartsWith("_")
                });
            }
        }

        return elements;
    }

    private List<CodeElement> AnalyzeJavaScript(CodeFile file)
    {
        var elements = new List<CodeElement>();
        var lines = file.Content.Split('\n');

        var classPattern = new Regex(@"^\s*class\s+(\w+)", RegexOptions.Compiled);
        var functionPattern = new Regex(@"^\s*(export\s+)?(async\s+)?function\s+(\w+)\s*\([^)]*\)", RegexOptions.Compiled);
        var arrowFunctionPattern = new Regex(@"^\s*(const|let|var)\s+(\w+)\s*=\s*(\([^)]*\)|[^=]+)\s*=>", RegexOptions.Compiled);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var classMatch = classPattern.Match(line);
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    Name = classMatch.Groups[1].Value,
                    Type = CodeElementType.Class,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    IsPublic = true
                });
            }

            var functionMatch = functionPattern.Match(line);
            if (functionMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    Name = functionMatch.Groups[3].Value,
                    Type = CodeElementType.Function,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    IsPublic = functionMatch.Groups[1].Value.Contains("export")
                });
            }

            var arrowMatch = arrowFunctionPattern.Match(line);
            if (arrowMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    Name = arrowMatch.Groups[2].Value,
                    Type = CodeElementType.Function,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    IsPublic = true
                });
            }
        }

        return elements;
    }

    private List<CodeElement> AnalyzeJava(CodeFile file)
    {
        var elements = new List<CodeElement>();
        var lines = file.Content.Split('\n');

        var classPattern = new Regex(@"^\s*(public|private|protected)?\s*(class|interface|enum)\s+(\w+)", RegexOptions.Compiled);
        var methodPattern = new Regex(@"^\s*(public|private|protected)?\s*\w+\s+(\w+)\s*\([^)]*\)", RegexOptions.Compiled);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var classMatch = classPattern.Match(line);
            if (classMatch.Success)
            {
                var visibility = classMatch.Groups[1].Value;
                var type = classMatch.Groups[2].Value;
                var name = classMatch.Groups[3].Value;

                elements.Add(new CodeElement
                {
                    Name = name,
                    Type = type switch
                    {
                        "class" => CodeElementType.Class,
                        "interface" => CodeElementType.Interface,
                        "enum" => CodeElementType.Enum,
                        _ => CodeElementType.Class
                    },
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Visibility = string.IsNullOrEmpty(visibility) ? "package-private" : visibility,
                    IsPublic = visibility == "public"
                });
            }

            var methodMatch = methodPattern.Match(line);
            if (methodMatch.Success && !classMatch.Success)
            {
                var visibility = methodMatch.Groups[1].Value;
                var name = methodMatch.Groups[2].Value;

                elements.Add(new CodeElement
                {
                    Name = name,
                    Type = CodeElementType.Method,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    Visibility = string.IsNullOrEmpty(visibility) ? "package-private" : visibility,
                    IsPublic = visibility == "public"
                });
            }
        }

        return elements;
    }

    private List<CodeElement> AnalyzeGeneric(CodeFile file)
    {
        // Generic analysis - just identify potential functions/classes by common patterns
        var elements = new List<CodeElement>();
        var lines = file.Content.Split('\n');

        var genericFunctionPattern = new Regex(@"^\s*(function|func|fn|def)\s+(\w+)", RegexOptions.Compiled);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var match = genericFunctionPattern.Match(line);
            if (match.Success)
            {
                elements.Add(new CodeElement
                {
                    Name = match.Groups[2].Value,
                    Type = CodeElementType.Function,
                    FilePath = file.FilePath,
                    StartLine = i + 1,
                    Signature = line.Trim(),
                    IsPublic = true
                });
            }
        }

        return elements;
    }

    private void ExtractDependencies(AnalysisResult result)
    {
        foreach (var file in result.Files)
        {
            var imports = new List<string>();

            switch (file.Language)
            {
                case ProgrammingLanguage.CSharp:
                    imports = ExtractCSharpImports(file.Content);
                    break;
                case ProgrammingLanguage.Python:
                    imports = ExtractPythonImports(file.Content);
                    break;
                case ProgrammingLanguage.JavaScript:
                case ProgrammingLanguage.TypeScript:
                    imports = ExtractJavaScriptImports(file.Content);
                    break;
                case ProgrammingLanguage.Java:
                    imports = ExtractJavaImports(file.Content);
                    break;
            }

            if (imports.Any())
            {
                result.Dependencies[file.FilePath] = imports;
            }
        }
    }

    private List<string> ExtractCSharpImports(string content)
    {
        var imports = new List<string>();
        var pattern = new Regex(@"^\s*using\s+([^;]+);", RegexOptions.Multiline | RegexOptions.Compiled);
        var matches = pattern.Matches(content);

        foreach (Match match in matches)
        {
            imports.Add(match.Groups[1].Value.Trim());
        }

        return imports;
    }

    private List<string> ExtractPythonImports(string content)
    {
        var imports = new List<string>();
        var pattern = new Regex(@"^\s*(import|from)\s+([^\s;]+)", RegexOptions.Multiline | RegexOptions.Compiled);
        var matches = pattern.Matches(content);

        foreach (Match match in matches)
        {
            imports.Add(match.Groups[2].Value.Trim());
        }

        return imports;
    }

    private List<string> ExtractJavaScriptImports(string content)
    {
        var imports = new List<string>();
        var pattern = new Regex(@"^\s*import\s+.+\s+from\s+['""]([^'""]+)['""]", RegexOptions.Multiline | RegexOptions.Compiled);
        var matches = pattern.Matches(content);

        foreach (Match match in matches)
        {
            imports.Add(match.Groups[1].Value.Trim());
        }

        // Also check for require statements
        var requirePattern = new Regex(@"require\s*\(\s*['""]([^'""]+)['""]\s*\)", RegexOptions.Compiled);
        var requireMatches = requirePattern.Matches(content);

        foreach (Match match in requireMatches)
        {
            var import = match.Groups[1].Value.Trim();
            if (!imports.Contains(import))
            {
                imports.Add(import);
            }
        }

        return imports;
    }

    private List<string> ExtractJavaImports(string content)
    {
        var imports = new List<string>();
        var pattern = new Regex(@"^\s*import\s+([^;]+);", RegexOptions.Multiline | RegexOptions.Compiled);
        var matches = pattern.Matches(content);

        foreach (Match match in matches)
        {
            imports.Add(match.Groups[1].Value.Trim());
        }

        return imports;
    }
}
