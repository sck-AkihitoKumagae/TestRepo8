using Microsoft.Extensions.Logging;
using SpecDocGenerator.Analysis.Services;
using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;
using SpecDocGenerator.Excel.Services;
using SpecDocGenerator.LLM.Services;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  プログラム仕様書作成アプリケーション");
        Console.WriteLine("  Specification Document Generator");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        // Get input directory
        string inputPath;
        if (args.Length > 0)
        {
            inputPath = args[0];
        }
        else
        {
            Console.Write("分析対象のフォルダパスを入力してください: ");
            inputPath = Console.ReadLine() ?? "";
        }

        if (string.IsNullOrWhiteSpace(inputPath) || !Directory.Exists(inputPath))
        {
            Console.WriteLine("エラー: 有効なフォルダパスを指定してください。");
            return;
        }

        // Get output path
        Console.Write("出力先のExcelファイルパス (デフォルト: 仕様書.xlsx): ");
        var outputPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = "仕様書.xlsx";
        }

        // Initialize services
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var languageDetector = new LanguageDetector();
        var codeAnalyzer = new CodeAnalyzer(languageDetector);
        
        // LLM configuration (optional)
        string? endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        string? apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
        string? deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
        
        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine();
            Console.WriteLine("注意: Azure OpenAI の設定が見つかりません。");
            Console.WriteLine("環境変数 AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_KEY, AZURE_OPENAI_DEPLOYMENT を設定すると、");
            Console.WriteLine("LLMによる高度な解析が可能になります。");
            Console.WriteLine("現在は基本的な解析のみを実行します。");
            Console.WriteLine();
        }

        var llmService = new LlmService(endpoint, apiKey, deployment, loggerFactory.CreateLogger<LlmService>());
        var excelExporter = new ExcelExporter(loggerFactory.CreateLogger<ExcelExporter>());

        try
        {
            Console.WriteLine("📂 ファイルをスキャンしています...");
            var codeFiles = await ScanFilesAsync(inputPath, languageDetector);
            Console.WriteLine($"✓ {codeFiles.Count} 個のコードファイルを検出しました。");
            Console.WriteLine();

            if (codeFiles.Count == 0)
            {
                Console.WriteLine("⚠ コードファイルが見つかりませんでした。");
                return;
            }

            Console.WriteLine("🔍 コードを解析しています...");
            var analysisResult = await codeAnalyzer.AnalyzeAsync(codeFiles);
            Console.WriteLine($"✓ {analysisResult.Elements.Count} 個のコード要素を抽出しました。");
            Console.WriteLine();

            Console.WriteLine("📝 仕様書を生成しています...");
            var chunks = await codeAnalyzer.CreateChunksAsync(analysisResult);
            Console.WriteLine($"✓ {chunks.Count} 個のチャンクに分割しました。");

            var summaries = new List<Summary>();
            int processedChunks = 0;
            
            foreach (var chunk in chunks.Take(10)) // Limit for demo
            {
                var summary = await llmService.SummarizeChunkAsync(chunk, "", CancellationToken.None);
                summaries.Add(new Summary
                {
                    Level = SummaryLevel.Function,
                    Content = summary,
                    SourceId = chunk.Id
                });
                
                processedChunks++;
                if (processedChunks % 5 == 0)
                {
                    Console.WriteLine($"  処理中: {processedChunks}/{Math.Min(chunks.Count, 10)} チャンク完了");
                }
            }

            Console.WriteLine("✓ サマリーを生成しました。");
            Console.WriteLine();

            Console.WriteLine("📊 仕様書ドキュメントを作成しています...");
            var specDocument = await llmService.GenerateSpecificationAsync(analysisResult, summaries);
            
            // Set project name from directory
            specDocument.ProjectName = Path.GetFileName(inputPath);
            specDocument.Overview["プロジェクト名"] = specDocument.ProjectName;
            
            Console.WriteLine("✓ 仕様書ドキュメントを作成しました。");
            Console.WriteLine();

            Console.WriteLine("💾 Excelファイルに出力しています...");
            await excelExporter.ExportAsync(specDocument, outputPath);
            Console.WriteLine($"✓ 仕様書を保存しました: {Path.GetFullPath(outputPath)}");
            Console.WriteLine();

            // Display summary
            Console.WriteLine("===========================================");
            Console.WriteLine("  完了サマリー");
            Console.WriteLine("===========================================");
            Console.WriteLine($"📁 対象フォルダ: {inputPath}");
            Console.WriteLine($"📄 ファイル数: {codeFiles.Count}");
            Console.WriteLine($"📊 総行数: {codeFiles.Sum(f => f.LineCount):N0}");
            Console.WriteLine($"🔧 要素数: {analysisResult.Elements.Count}");
            Console.WriteLine($"🔗 依存関係: {analysisResult.Dependencies.Count}");
            Console.WriteLine($"💾 出力先: {Path.GetFullPath(outputPath)}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("✅ 処理が正常に完了しました！");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ エラーが発生しました: {ex.Message}");
            Console.WriteLine($"詳細: {ex}");
        }
    }

    static async Task<List<CodeFile>> ScanFilesAsync(string directoryPath, ILanguageDetector languageDetector)
    {
        var codeFiles = new List<CodeFile>();
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var filePath in files)
        {
            if (!languageDetector.IsCodeFile(filePath))
                continue;

            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var fileInfo = new FileInfo(filePath);

                var codeFile = new CodeFile
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Language = languageDetector.DetectLanguage(filePath),
                    Content = content,
                    LineCount = content.Split('\n').Length,
                    LastModified = fileInfo.LastWriteTime,
                    SizeInBytes = fileInfo.Length
                };

                codeFiles.Add(codeFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"警告: ファイル {filePath} の読み込みに失敗しました: {ex.Message}");
            }
        }

        return codeFiles;
    }
}
