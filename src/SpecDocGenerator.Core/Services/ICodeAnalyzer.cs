using SpecDocGenerator.Core.Models;

namespace SpecDocGenerator.Core.Services;

/// <summary>
/// Interface for code analysis service
/// </summary>
public interface ICodeAnalyzer
{
    Task<AnalysisResult> AnalyzeAsync(List<CodeFile> files, CancellationToken cancellationToken = default);
    Task<List<CodeChunk>> CreateChunksAsync(AnalysisResult analysisResult, int maxTokens = 4000);
}
