using SpecDocGenerator.Core.Models;

namespace SpecDocGenerator.Core.Services;

/// <summary>
/// Interface for LLM service
/// </summary>
public interface ILlmService
{
    Task<string> SummarizeChunkAsync(CodeChunk chunk, string promptTemplate, CancellationToken cancellationToken = default);
    Task<Summary> CreateHierarchicalSummaryAsync(List<Summary> childSummaries, SummaryLevel targetLevel, CancellationToken cancellationToken = default);
    Task<SpecificationDocument> GenerateSpecificationAsync(AnalysisResult analysisResult, List<Summary> summaries, CancellationToken cancellationToken = default);
}
