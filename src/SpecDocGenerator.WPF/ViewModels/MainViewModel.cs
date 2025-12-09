using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SpecDocGenerator.Analysis.Services;
using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;
using SpecDocGenerator.Excel.Services;
using SpecDocGenerator.LLM.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SpecDocGenerator.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILanguageDetector _languageDetector;
    private readonly ICodeAnalyzer _codeAnalyzer;
    private readonly ILlmService _llmService;
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<MainViewModel> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _targetFolderPath = string.Empty;

    [ObservableProperty]
    private string _outputFilePath = "仕様書.xlsx";

    [ObservableProperty]
    private string _excludePattern = "bin,obj,node_modules,.git,.vs";

    [ObservableProperty]
    private string _languageFilter = "";

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private bool _isPaused = false;

    [ObservableProperty]
    private bool _canPause = false;

    [ObservableProperty]
    private int _progressPercentage = 0;

    [ObservableProperty]
    private string _progressMessage = "準備完了";

    [ObservableProperty]
    private string _currentPhase = "";

    [ObservableProperty]
    private ObservableCollection<FileTreeItem> _fileTree = new();

    [ObservableProperty]
    private string _previewOverview = "";

    [ObservableProperty]
    private string _previewFunctionalSpec = "";

    [ObservableProperty]
    private string _previewApiSpec = "";

    [ObservableProperty]
    private string _previewDataStructure = "";

    [ObservableProperty]
    private string _previewDependencies = "";

    [ObservableProperty]
    private string _previewTestPerspectives = "";

    [ObservableProperty]
    private string _previewRisks = "";

    [ObservableProperty]
    private string _previewChangeHistory = "";

    [ObservableProperty]
    private int _totalFiles = 0;

    [ObservableProperty]
    private int _processedFiles = 0;

    [ObservableProperty]
    private int _totalElements = 0;

    [ObservableProperty]
    private int _totalDependencies = 0;

    public MainViewModel(
        ILanguageDetector languageDetector,
        ICodeAnalyzer codeAnalyzer,
        ILlmService llmService,
        IExcelExporter excelExporter,
        ILogger<MainViewModel> logger)
    {
        _languageDetector = languageDetector;
        _codeAnalyzer = codeAnalyzer;
        _llmService = llmService;
        _excelExporter = excelExporter;
        _logger = logger;
    }

    [RelayCommand]
    private void SelectTargetFolder()
    {
        var folderDialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "解析対象フォルダを選択してください",
            ShowNewFolderButton = false
        };

        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            TargetFolderPath = folderDialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void SelectOutputFile()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            DefaultExt = ".xlsx",
            FileName = "仕様書.xlsx",
            Title = "出力先を選択"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFilePath = dialog.FileName;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAnalysisAsync()
    {
        if (string.IsNullOrWhiteSpace(TargetFolderPath) || !Directory.Exists(TargetFolderPath))
        {
            System.Windows.MessageBox.Show("有効なフォルダパスを指定してください。", "エラー", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }

        IsProcessing = true;
        CanPause = true;
        IsPaused = false;
        ProgressPercentage = 0;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await ExecuteAnalysisAsync(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            ProgressMessage = "処理がキャンセルされました";
            _logger.LogInformation("Analysis cancelled by user");
        }
        catch (Exception ex)
        {
            ProgressMessage = $"エラー: {ex.Message}";
            _logger.LogError(ex, "Error during analysis");
            System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
            CanPause = false;
            IsPaused = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task ExecuteAnalysisAsync(CancellationToken cancellationToken)
    {
        // Phase 1: File Scanning
        CurrentPhase = "ファイルスキャン";
        ProgressMessage = "ファイルをスキャンしています...";
        ProgressPercentage = 5;

        var codeFiles = await ScanFilesAsync(TargetFolderPath, cancellationToken);
        TotalFiles = codeFiles.Count;

        if (codeFiles.Count == 0)
        {
            ProgressMessage = "コードファイルが見つかりませんでした";
            return;
        }

        ProgressPercentage = 15;
        ProgressMessage = $"{codeFiles.Count} 個のファイルを検出しました";

        // Build file tree
        BuildFileTree(codeFiles);

        // Phase 2: Code Analysis
        CurrentPhase = "コード解析";
        ProgressMessage = "コードを解析しています...";
        ProgressPercentage = 25;

        var analysisResult = await _codeAnalyzer.AnalyzeAsync(codeFiles);
        TotalElements = analysisResult.Elements.Count;
        TotalDependencies = analysisResult.Dependencies.Count;

        ProgressPercentage = 40;
        ProgressMessage = $"{analysisResult.Elements.Count} 個のコード要素を抽出しました";

        // Phase 3: Chunking
        CurrentPhase = "チャンク分割";
        ProgressMessage = "コードをチャンクに分割しています...";
        ProgressPercentage = 50;

        var chunks = await _codeAnalyzer.CreateChunksAsync(analysisResult);
        ProgressMessage = $"{chunks.Count} 個のチャンクを作成しました";

        // Phase 4: Summarization
        CurrentPhase = "要約生成";
        ProgressMessage = "LLMによる要約を生成しています...";
        ProgressPercentage = 60;

        var summaries = new List<Summary>();
        int processedChunks = 0;
        int chunksToProcess = Math.Min(chunks.Count, 20); // Limit for performance

        foreach (var chunk in chunks.Take(chunksToProcess))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var summary = await _llmService.SummarizeChunkAsync(chunk, "", cancellationToken);
            summaries.Add(new Summary
            {
                Level = SummaryLevel.Function,
                Content = summary,
                SourceId = chunk.Id
            });

            processedChunks++;
            ProgressPercentage = 60 + (int)((processedChunks / (double)chunksToProcess) * 20);
            ProgressMessage = $"要約生成中: {processedChunks}/{chunksToProcess}";
        }

        // Phase 5: Specification Generation
        CurrentPhase = "仕様書生成";
        ProgressMessage = "仕様書を生成しています...";
        ProgressPercentage = 85;

        var specDocument = await _llmService.GenerateSpecificationAsync(analysisResult, summaries);
        specDocument.ProjectName = Path.GetFileName(TargetFolderPath);
        specDocument.Overview["プロジェクト名"] = specDocument.ProjectName;

        // Update previews
        UpdatePreviews(specDocument);

        ProgressPercentage = 95;

        // Phase 6: Excel Export
        CurrentPhase = "Excel出力";
        ProgressMessage = "Excelファイルに出力しています...";

        await _excelExporter.ExportAsync(specDocument, OutputFilePath);

        ProgressPercentage = 100;
        ProgressMessage = "処理が完了しました";
        CurrentPhase = "完了";

        System.Windows.MessageBox.Show($"仕様書の生成が完了しました。\n出力先: {Path.GetFullPath(OutputFilePath)}", 
            "完了", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private async Task<List<CodeFile>> ScanFilesAsync(string directoryPath, CancellationToken cancellationToken)
    {
        var codeFiles = new List<CodeFile>();
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        var excludePatterns = ExcludePattern.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList();

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if file should be excluded
            bool shouldExclude = excludePatterns.Any(pattern => 
                filePath.Contains(Path.DirectorySeparatorChar + pattern + Path.DirectorySeparatorChar) ||
                filePath.Contains(Path.DirectorySeparatorChar + pattern));

            if (shouldExclude || !_languageDetector.IsCodeFile(filePath))
                continue;

            try
            {
                var content = await File.ReadAllTextAsync(filePath, cancellationToken);
                var fileInfo = new FileInfo(filePath);

                var language = _languageDetector.DetectLanguage(filePath);
                
                // Apply language filter if specified
                if (!string.IsNullOrWhiteSpace(LanguageFilter))
                {
                    var allowedLanguages = LanguageFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim().ToLower())
                        .ToList();

                    if (!allowedLanguages.Contains(language.ToString().ToLower()))
                        continue;
                }

                var codeFile = new CodeFile
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Language = language,
                    Content = content,
                    LineCount = content.Split('\n').Length,
                    LastModified = fileInfo.LastWriteTime,
                    SizeInBytes = fileInfo.Length
                };

                codeFiles.Add(codeFile);
                ProcessedFiles++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read file: {FilePath}", filePath);
            }
        }

        return codeFiles;
    }

    private void BuildFileTree(List<CodeFile> codeFiles)
    {
        FileTree.Clear();

        var grouped = codeFiles.GroupBy(f => Path.GetDirectoryName(f.FilePath) ?? "")
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            foreach (var file in group)
            {
                FileTree.Add(new FileTreeItem
                {
                    Name = file.FileName,
                    Path = file.FilePath,
                    Language = file.Language.ToString(),
                    Lines = file.LineCount
                });
            }
        }
    }

    private void UpdatePreviews(SpecificationDocument document)
    {
        PreviewOverview = FormatDictionary(document.Overview);
        PreviewFunctionalSpec = FormatFunctionSpecs(document.FunctionSpecs);
        PreviewApiSpec = FormatApiSpecs(document.ApiSpecs);
        PreviewDataStructure = FormatDataStructures(document.DataStructures);
        PreviewDependencies = FormatDependencies(document.Dependencies);
        PreviewTestPerspectives = FormatTestConsiderations(document.TestConsiderations);
        PreviewRisks = FormatRisks(document.Risks);
        PreviewChangeHistory = FormatChangeHistory(document.ChangeHistory);
    }

    private string FormatDictionary(Dictionary<string, string> dict)
    {
        if (dict == null || dict.Count == 0)
            return "データがありません";

        return string.Join("\n\n", dict.Select(kvp => $"【{kvp.Key}】\n{kvp.Value}"));
    }

    private string FormatFunctionSpecs(List<FunctionSpec> specs)
    {
        if (specs == null || specs.Count == 0)
            return "機能仕様がありません";

        return string.Join("\n\n", specs.Select((spec, index) => 
            $"{index + 1}. {spec.FunctionName}\n" +
            $"   概要: {spec.Overview}\n" +
            $"   入力: {spec.Input}\n" +
            $"   出力: {spec.Output}"));
    }

    private string FormatApiSpecs(List<ApiSpec> specs)
    {
        if (specs == null || specs.Count == 0)
            return "API仕様がありません";

        return string.Join("\n\n", specs.Select((spec, index) => 
            $"{index + 1}. {spec.Category}.{spec.Name}\n" +
            $"   説明: {spec.Description}\n" +
            $"   入力: {spec.Input}\n" +
            $"   出力: {spec.Output}"));
    }

    private string FormatDataStructures(List<DataStructure> structures)
    {
        if (structures == null || structures.Count == 0)
            return "データ構造がありません";

        return string.Join("\n\n", structures.GroupBy(d => d.ClassName)
            .Select(g => $"【{g.Key}】\n" + 
                string.Join("\n", g.Select(d => $"  - {d.FieldName}: {d.DataType} ({d.Description})"))));
    }

    private string FormatDependencies(List<Dependency> dependencies)
    {
        if (dependencies == null || dependencies.Count == 0)
            return "依存関係がありません";

        return string.Join("\n", dependencies.Select(d => 
            $"{d.Module} → {d.DependsOn} ({d.Type})"));
    }

    private string FormatTestConsiderations(List<TestConsideration> tests)
    {
        if (tests == null || tests.Count == 0)
            return "テスト観点がありません";

        return string.Join("\n\n", tests.Select((test, index) => 
            $"{index + 1}. {test.FunctionName}\n" +
            $"   期待結果: {test.ExpectedResult}\n" +
            $"   前提条件: {test.Preconditions}"));
    }

    private string FormatRisks(List<Risk> risks)
    {
        if (risks == null || risks.Count == 0)
            return "リスクがありません";

        return string.Join("\n\n", risks.Select((risk, index) => 
            $"{index + 1}. [{risk.RiskId}] {risk.Content}\n" +
            $"   影響度: {risk.Impact} / 発生確率: {risk.Probability}\n" +
            $"   対策: {risk.Mitigation}"));
    }

    private string FormatChangeHistory(List<ChangeHistoryEntry> history)
    {
        if (history == null || history.Count == 0)
            return "変更履歴がありません";

        return string.Join("\n\n", history.Select(entry => 
            $"[{entry.Version}] {entry.Date:yyyy/MM/dd}\n" +
            $"作成者: {entry.Author}\n" +
            $"変更内容: {entry.Changes}"));
    }

    [RelayCommand(CanExecute = nameof(CanPauseCommand))]
    private void Pause()
    {
        IsPaused = true;
        ProgressMessage = "一時停止中...";
    }

    [RelayCommand(CanExecute = nameof(CanResumeCommand))]
    private void Resume()
    {
        IsPaused = false;
        ProgressMessage = "処理を再開しました";
    }

    [RelayCommand(CanExecute = nameof(CanCancelCommand))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settingsWindow = new Views.SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private bool CanStart() => !IsProcessing && !string.IsNullOrWhiteSpace(TargetFolderPath);
    private bool CanPauseCommand() => IsProcessing && !IsPaused && CanPause;
    private bool CanResumeCommand() => IsProcessing && IsPaused;
    private bool CanCancelCommand() => IsProcessing;
}

public class FileTreeItem
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Language { get; set; } = "";
    public int Lines { get; set; }
}
