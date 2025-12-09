using SpecDocGenerator.Core.Models;

namespace SpecDocGenerator.Core.Services;

/// <summary>
/// Interface for Excel export service
/// </summary>
public interface IExcelExporter
{
    Task ExportAsync(SpecificationDocument document, string outputPath, CancellationToken cancellationToken = default);
}
