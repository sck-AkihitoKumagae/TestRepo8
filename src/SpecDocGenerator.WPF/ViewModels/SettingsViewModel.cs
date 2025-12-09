using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace SpecDocGenerator.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _llmProvider = "Azure OpenAI";

    [ObservableProperty]
    private string _azureOpenAiEndpoint = "";

    [ObservableProperty]
    private string _azureOpenAiKey = "";

    [ObservableProperty]
    private string _azureOpenAiDeployment = "gpt-4";

    [ObservableProperty]
    private string _localLlmEndpoint = "http://localhost:11434";

    [ObservableProperty]
    private double _temperature = 0.7;

    [ObservableProperty]
    private int _maxTokens = 4000;

    [ObservableProperty]
    private int _chunkSize = 4000;

    [ObservableProperty]
    private int _parallelism = 1;

    [ObservableProperty]
    private int _retryCount = 3;

    [ObservableProperty]
    private string _logPath = "logs";

    public SettingsViewModel()
    {
        LoadSettings();
    }

    [RelayCommand]
    private void LoadSettings()
    {
        // Load from environment variables
        AzureOpenAiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "";
        AzureOpenAiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? "";
        AzureOpenAiDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            // Save to environment variables (for current session)
            if (!string.IsNullOrWhiteSpace(AzureOpenAiEndpoint))
                Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", AzureOpenAiEndpoint);
            
            if (!string.IsNullOrWhiteSpace(AzureOpenAiKey))
                Environment.SetEnvironmentVariable("AZURE_OPENAI_KEY", AzureOpenAiKey);
            
            if (!string.IsNullOrWhiteSpace(AzureOpenAiDeployment))
                Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", AzureOpenAiDeployment);

            System.Windows.MessageBox.Show("設定を保存しました。\n※環境変数への永続的な保存は、システム設定から行ってください。", 
                "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"設定の保存に失敗しました: {ex.Message}", 
                "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void TestConnection()
    {
        if (string.IsNullOrWhiteSpace(AzureOpenAiEndpoint) || string.IsNullOrWhiteSpace(AzureOpenAiKey))
        {
            System.Windows.MessageBox.Show("Azure OpenAI のエンドポイントとAPIキーを設定してください。", 
                "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            // TODO: Implement actual connection test
            System.Windows.MessageBox.Show("接続テスト機能は今後実装予定です。", 
                "情報", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"接続テストに失敗しました: {ex.Message}", 
                "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SelectLogPath()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "ログ保存先フォルダを選択してください",
            ShowNewFolderButton = true,
            SelectedPath = LogPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            LogPath = dialog.SelectedPath;
        }
    }
}
