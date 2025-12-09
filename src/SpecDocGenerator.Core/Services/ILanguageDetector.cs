using SpecDocGenerator.Core.Models;

namespace SpecDocGenerator.Core.Services;

/// <summary>
/// Interface for language detection service
/// </summary>
public interface ILanguageDetector
{
    ProgrammingLanguage DetectLanguage(string filePath);
    bool IsCodeFile(string filePath);
}
