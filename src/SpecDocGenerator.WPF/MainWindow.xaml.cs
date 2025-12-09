using System.Windows;
using Microsoft.Extensions.Logging;
using SpecDocGenerator.WPF.ViewModels;
using SpecDocGenerator.Analysis.Services;
using SpecDocGenerator.LLM.Services;
using SpecDocGenerator.Excel.Services;

namespace SpecDocGenerator.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services and ViewModel
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

        var llmService = new LlmService(endpoint, apiKey, deployment, loggerFactory.CreateLogger<LlmService>());
        var excelExporter = new ExcelExporter(loggerFactory.CreateLogger<ExcelExporter>());

        var viewModel = new MainViewModel(
            languageDetector,
            codeAnalyzer,
            llmService,
            excelExporter,
            loggerFactory.CreateLogger<MainViewModel>()
        );

        DataContext = viewModel;
    }
}