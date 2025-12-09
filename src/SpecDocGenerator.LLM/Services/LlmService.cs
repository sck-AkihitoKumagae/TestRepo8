using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SpecDocGenerator.LLM.Services;

/// <summary>
/// LLM service with support for Azure OpenAI
/// Note: Full Azure OpenAI integration requires Azure.AI.OpenAI package
/// This implementation provides basic functionality without external API calls
/// </summary>
public class LlmService : ILlmService
{
    private readonly ILogger<LlmService> _logger;
    private readonly bool _isConfigured;
    private readonly string? _endpoint;
    private readonly string? _apiKey;
    private readonly string _deploymentName;

    public LlmService(string? endpoint, string? apiKey, string? deploymentName, ILogger<LlmService> logger)
    {
        _logger = logger;
        _endpoint = endpoint;
        _apiKey = apiKey;
        _deploymentName = deploymentName ?? "gpt-4";

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            _isConfigured = true;
            _logger.LogInformation("LLM service configured with endpoint: {Endpoint}", endpoint);
        }
        else
        {
            _logger.LogWarning("LLM service not configured. API Key or Endpoint missing. Using basic analysis.");
            _isConfigured = false;
        }
    }

    public async Task<string> SummarizeChunkAsync(CodeChunk chunk, string promptTemplate, CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("LLM not configured, returning basic summary");
            return GenerateBasicSummary(chunk);
        }

        try
        {
            // TODO: Implement actual Azure OpenAI API call when needed
            // For now, use basic summary
            _logger.LogWarning("Azure OpenAI integration not fully implemented. Using basic summary.");
            return GenerateBasicSummary(chunk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing chunk from {FilePath}", chunk.FilePath);
            return GenerateBasicSummary(chunk);
        }
    }

    public async Task<Summary> CreateHierarchicalSummaryAsync(List<Summary> childSummaries, SummaryLevel targetLevel, CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("LLM not configured, returning basic aggregated summary");
            return GenerateBasicHierarchicalSummary(childSummaries, targetLevel);
        }

        try
        {
            // TODO: Implement actual Azure OpenAI API call when needed
            // For now, use basic summary
            _logger.LogWarning("Azure OpenAI integration not fully implemented. Using basic aggregation.");
            return GenerateBasicHierarchicalSummary(childSummaries, targetLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hierarchical summary at {Level} level", targetLevel);
            return GenerateBasicHierarchicalSummary(childSummaries, targetLevel);
        }
    }

    public async Task<SpecificationDocument> GenerateSpecificationAsync(AnalysisResult analysisResult, List<Summary> summaries, CancellationToken cancellationToken = default)
    {
        var doc = new SpecificationDocument
        {
            GeneratedDate = DateTime.Now
        };

        // Generate Overview
        doc.Overview = await GenerateOverviewAsync(analysisResult, summaries, cancellationToken);

        // Generate Function Specifications
        doc.FunctionSpecs = GenerateFunctionSpecs(analysisResult, summaries);

        // Generate API Specifications
        doc.ApiSpecs = GenerateApiSpecs(analysisResult);

        // Generate Data Structures
        doc.DataStructures = GenerateDataStructures(analysisResult);

        // Generate Dependencies
        doc.Dependencies = GenerateDependencies(analysisResult);

        // Generate File List
        doc.FileList = GenerateFileList(analysisResult);

        // Generate Test Considerations
        doc.TestConsiderations = await GenerateTestConsiderationsAsync(analysisResult, cancellationToken);

        // Generate Risks
        doc.Risks = GenerateRisks(analysisResult);

        // Add change history entry
        doc.ChangeHistory.Add(new ChangeHistoryEntry
        {
            Version = doc.Version,
            Date = doc.GeneratedDate,
            Changes = "Initial specification document generated",
            Author = "SpecDocGenerator"
        });

        return doc;
    }

    private string BuildPrompt(string template, CodeChunk chunk)
    {
        if (string.IsNullOrEmpty(template))
        {
            template = @"Analyze the following code and provide a concise summary including:
1. Purpose: What does this code do?
2. Inputs: What are the parameters/inputs?
3. Outputs: What does it return/produce?
4. Side Effects: Does it modify state or have side effects?
5. Dependencies: What external components does it depend on?

Code:
{CONTENT}

Language: {LANGUAGE}
File: {FILEPATH}
Lines: {STARTLINE}-{ENDLINE}";
        }

        return template
            .Replace("{CONTENT}", chunk.Content)
            .Replace("{LANGUAGE}", chunk.Language.ToString())
            .Replace("{FILEPATH}", chunk.FilePath)
            .Replace("{STARTLINE}", chunk.StartLine.ToString())
            .Replace("{ENDLINE}", chunk.EndLine.ToString());
    }

    private string GenerateBasicSummary(CodeChunk chunk)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Code from: {chunk.FilePath} (lines {chunk.StartLine}-{chunk.EndLine})");
        sb.AppendLine($"Language: {chunk.Language}");
        sb.AppendLine($"Content length: {chunk.Content.Length} characters");
        
        // Count basic elements
        var lines = chunk.Content.Split('\n');
        var commentLines = lines.Count(l => l.TrimStart().StartsWith("//") || l.TrimStart().StartsWith("#"));
        sb.AppendLine($"Total lines: {lines.Length}");
        sb.AppendLine($"Comment lines: {commentLines}");

        return sb.ToString();
    }

    private Summary GenerateBasicHierarchicalSummary(List<Summary> childSummaries, SummaryLevel targetLevel)
    {
        var content = $"Aggregated summary at {targetLevel} level\n" +
                      $"Contains {childSummaries.Count} child summaries\n\n" +
                      string.Join("\n\n", childSummaries.Select((s, i) => $"Component {i + 1}:\n{s.Content}"));

        return new Summary
        {
            Level = targetLevel,
            Content = content,
            ChildSummaryIds = childSummaries.Select(s => s.Id).ToList()
        };
    }

    private async Task<Dictionary<string, string>> GenerateOverviewAsync(AnalysisResult analysisResult, List<Summary> summaries, CancellationToken cancellationToken)
    {
        var overview = new Dictionary<string, string>
        {
            ["プロジェクト名"] = "Analyzed Project",
            ["ファイル数"] = analysisResult.Files.Count.ToString(),
            ["総行数"] = analysisResult.Files.Sum(f => f.LineCount).ToString(),
            ["プログラミング言語"] = string.Join(", ", analysisResult.Files.Select(f => f.Language).Distinct()),
            ["要素数"] = analysisResult.Elements.Count.ToString(),
            ["公開API数"] = analysisResult.Elements.Count(e => e.IsPublic).ToString(),
            ["依存関係数"] = analysisResult.Dependencies.Count.ToString()
        };

        // Add system-level summary if available
        var systemSummary = summaries.FirstOrDefault(s => s.Level == SummaryLevel.System);
        if (systemSummary != null)
        {
            overview["システム概要"] = systemSummary.Content;
        }
        else
        {
            overview["システム概要"] = "プロジェクトの詳細な分析により生成された仕様書";
        }

        return overview;
    }

    private List<FunctionSpec> GenerateFunctionSpecs(AnalysisResult analysisResult, List<Summary> summaries)
    {
        var specs = new List<FunctionSpec>();
        var functionElements = analysisResult.Elements.Where(e => 
            e.Type == CodeElementType.Function || 
            e.Type == CodeElementType.Method).ToList();

        int id = 1;
        foreach (var element in functionElements)
        {
            var relatedSummary = summaries.FirstOrDefault(s => 
                s.Metadata.ContainsKey("ElementName") && 
                s.Metadata["ElementName"] == element.Name);

            specs.Add(new FunctionSpec
            {
                FunctionId = $"F{id:D3}",
                FunctionName = element.Name,
                Overview = relatedSummary?.Content ?? $"{element.Name}の機能",
                Input = string.Join(", ", element.Parameters),
                Output = element.ReturnType,
                Preconditions = element.IsPublic ? "公開API" : "内部利用"
            });
            id++;
        }

        return specs;
    }

    private List<ApiSpec> GenerateApiSpecs(AnalysisResult analysisResult)
    {
        var specs = new List<ApiSpec>();
        var publicElements = analysisResult.Elements.Where(e => e.IsPublic).ToList();

        foreach (var element in publicElements)
        {
            var category = element.Type switch
            {
                CodeElementType.Class => "クラス",
                CodeElementType.Interface => "インターフェース",
                CodeElementType.Function => "関数",
                CodeElementType.Method => "メソッド",
                _ => "その他"
            };

            specs.Add(new ApiSpec
            {
                Category = category,
                Name = element.Name,
                Input = string.Join(", ", element.Parameters),
                Output = element.ReturnType,
                Description = element.Documentation ?? $"{element.Name}の説明"
            });
        }

        return specs;
    }

    private List<DataStructure> GenerateDataStructures(AnalysisResult analysisResult)
    {
        var structures = new List<DataStructure>();
        var classElements = analysisResult.Elements.Where(e => 
            e.Type == CodeElementType.Class || 
            e.Type == CodeElementType.Struct || 
            e.Type == CodeElementType.Interface).ToList();

        foreach (var classElement in classElements)
        {
            // Add the class itself
            structures.Add(new DataStructure
            {
                ClassName = classElement.Name,
                FieldName = "(クラス定義)",
                DataType = classElement.Type.ToString(),
                Description = classElement.Documentation ?? $"{classElement.Name}のデータ構造",
                References = classElement.FilePath
            });

            // Add fields/properties
            var relatedFields = analysisResult.Elements.Where(e => 
                (e.Type == CodeElementType.Field || e.Type == CodeElementType.Property) &&
                e.FilePath == classElement.FilePath &&
                e.StartLine >= classElement.StartLine &&
                e.StartLine <= classElement.EndLine).ToList();

            foreach (var field in relatedFields)
            {
                structures.Add(new DataStructure
                {
                    ClassName = classElement.Name,
                    FieldName = field.Name,
                    DataType = field.ReturnType,
                    Description = field.Documentation ?? "",
                    References = field.IsPublic ? "公開" : "非公開"
                });
            }
        }

        return structures;
    }

    private List<Dependency> GenerateDependencies(AnalysisResult analysisResult)
    {
        var dependencies = new List<Dependency>();

        foreach (var kvp in analysisResult.Dependencies)
        {
            var moduleName = Path.GetFileNameWithoutExtension(kvp.Key);

            foreach (var dep in kvp.Value)
            {
                var isExternal = !dep.StartsWith(".") && !dep.Contains(Path.DirectorySeparatorChar);

                dependencies.Add(new Dependency
                {
                    Module = moduleName,
                    DependsOn = dep,
                    Type = isExternal ? "外部" : "内部",
                    Version = "",
                    Notes = ""
                });
            }
        }

        return dependencies;
    }

    private List<CodeFileInfo> GenerateFileList(AnalysisResult analysisResult)
    {
        return analysisResult.Files.Select(f => new CodeFileInfo
        {
            Path = f.FilePath,
            Language = f.Language.ToString(),
            LineCount = f.LineCount,
            LastModified = f.LastModified,
            Notes = ""
        }).ToList();
    }

    private async Task<List<TestConsideration>> GenerateTestConsiderationsAsync(AnalysisResult analysisResult, CancellationToken cancellationToken)
    {
        var considerations = new List<TestConsideration>();
        var publicFunctions = analysisResult.Elements.Where(e => 
            e.IsPublic && 
            (e.Type == CodeElementType.Function || e.Type == CodeElementType.Method))
            .Take(10) // Limit to first 10 for brevity
            .ToList();

        int id = 1;
        foreach (var func in publicFunctions)
        {
            considerations.Add(new TestConsideration
            {
                TestId = $"T{id:D3}",
                FunctionName = func.Name,
                ExpectedResult = "正常に実行され、期待される出力を返すこと",
                Preconditions = "必要な入力パラメータが正しく設定されていること",
                Notes = "エッジケースと異常系のテストも実施すること"
            });
            id++;
        }

        return considerations;
    }

    private List<Risk> GenerateRisks(AnalysisResult analysisResult)
    {
        var risks = new List<Risk>();

        // Analyze potential risks
        if (analysisResult.Files.Count > 50)
        {
            risks.Add(new Risk
            {
                RiskId = "R001",
                Content = "大規模プロジェクトのため、メンテナンス性に課題がある可能性",
                Impact = "中",
                Probability = "中",
                Mitigation = "モジュール化とドキュメント整備を進める"
            });
        }

        var complexElements = analysisResult.Elements.Count(e => e.Dependencies.Count > 5);
        if (complexElements > 0)
        {
            risks.Add(new Risk
            {
                RiskId = "R002",
                Content = $"複雑な依存関係を持つ要素が{complexElements}個存在",
                Impact = "中",
                Probability = "高",
                Mitigation = "依存関係の整理とリファクタリングを検討"
            });
        }

        if (!risks.Any())
        {
            risks.Add(new Risk
            {
                RiskId = "R001",
                Content = "現時点で重大なリスクは検出されていません",
                Impact = "低",
                Probability = "低",
                Mitigation = "継続的なコードレビューと品質管理"
            });
        }

        return risks;
    }
}
