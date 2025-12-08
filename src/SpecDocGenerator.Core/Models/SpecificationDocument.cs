namespace SpecDocGenerator.Core.Models;

/// <summary>
/// Specification document structure
/// </summary>
public class SpecificationDocument
{
    public string ProjectName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
    
    // Sheet 01: Overview
    public Dictionary<string, string> Overview { get; set; } = new();
    
    // Sheet 02: Function Specifications
    public List<FunctionSpec> FunctionSpecs { get; set; } = new();
    
    // Sheet 03: API Specifications
    public List<ApiSpec> ApiSpecs { get; set; } = new();
    
    // Sheet 04: Data Structures
    public List<DataStructure> DataStructures { get; set; } = new();
    
    // Sheet 05: Dependencies
    public List<Dependency> Dependencies { get; set; } = new();
    
    // Sheet 06: File List
    public List<CodeFileInfo> FileList { get; set; } = new();
    
    // Sheet 07: Test Considerations
    public List<TestConsideration> TestConsiderations { get; set; } = new();
    
    // Sheet 08: Risks
    public List<Risk> Risks { get; set; } = new();
    
    // Sheet 09: Change History
    public List<ChangeHistoryEntry> ChangeHistory { get; set; } = new();
}

public class FunctionSpec
{
    public string FunctionId { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Preconditions { get; set; } = string.Empty;
}

public class ApiSpec
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DataStructure
{
    public string ClassName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string References { get; set; } = string.Empty;
}

public class Dependency
{
    public string Module { get; set; } = string.Empty;
    public string DependsOn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Internal/External
    public string Version { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CodeFileInfo
{
    public string Path { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public DateTime LastModified { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class TestConsideration
{
    public string TestId { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string Preconditions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class Risk
{
    public string RiskId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Probability { get; set; } = string.Empty;
    public string Mitigation { get; set; } = string.Empty;
}

public class ChangeHistoryEntry
{
    public string Version { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Changes { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}
