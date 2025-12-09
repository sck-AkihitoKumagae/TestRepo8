# WPF GUI Application Implementation Report

## 実装概要

既存のCLI（コマンドライン）ベースのコード解析・仕様書生成アプリケーションを、WPFを使用したGUIデスクトップアプリケーションとして実装しました。

## 実装内容

### 1. プロジェクト構成

新規に `SpecDocGenerator.WPF` プロジェクトを作成し、ソリューションに追加しました。

```
src/SpecDocGenerator.WPF/
├── App.xaml                         # アプリケーションエントリポイント
├── App.xaml.cs
├── MainWindow.xaml                  # メインウィンドウ
├── MainWindow.xaml.cs
├── ViewModels/
│   ├── MainViewModel.cs             # メイン画面のビジネスロジック
│   └── SettingsViewModel.cs         # 設定画面のビジネスロジック
├── Views/
│   ├── SettingsWindow.xaml          # 設定ウィンドウ
│   └── SettingsWindow.xaml.cs
├── Converters/
│   └── InverseBooleanConverter.cs   # Boolean反転コンバーター
├── SpecDocGenerator.WPF.csproj      # プロジェクトファイル
└── README.md                        # WPF版のドキュメント
```

### 2. 技術スタック

- **.NET 8.0 (net8.0-windows)**: ターゲットフレームワーク
- **WPF (Windows Presentation Foundation)**: UIフレームワーク
- **CommunityToolkit.Mvvm (8.3.2)**: MVVM実装のヘルパーライブラリ
- **Microsoft.Extensions.Logging (8.0.1)**: ロギング機能
- 既存のCoreライブラリへの参照:
  - SpecDocGenerator.Core
  - SpecDocGenerator.Analysis
  - SpecDocGenerator.LLM
  - SpecDocGenerator.Excel

### 3. アーキテクチャ

#### MVVMパターンの採用

**Model-View-ViewModel (MVVM)** パターンを採用し、UIとビジネスロジックを分離しました。

- **Model**: 既存のCoreプロジェクトのモデルを使用
- **View**: XAMLで定義されたUI
- **ViewModel**: UIとModelの橋渡し、ビジネスロジックを含む

#### 非同期処理

すべての重い処理（ファイルスキャン、解析、LLM呼び出し）は非同期で実行され、UIスレッドをブロックしません。

```csharp
[RelayCommand(CanExecute = nameof(CanStart))]
private async Task StartAnalysisAsync()
{
    // 非同期処理
    await ExecuteAnalysisAsync(_cancellationTokenSource.Token);
}
```

#### キャンセル対応

`CancellationToken` を使用して、ユーザーが処理をキャンセルできるようにしました。

### 4. 主要機能

#### メイン画面 (MainWindow)

**上部: 設定セクション**
- 対象フォルダ選択 (FolderBrowserDialog)
- 出力先Excelファイル選択 (SaveFileDialog)
- 除外パターン設定 (カンマ区切り、デフォルト: bin,obj,node_modules,.git,.vs)
- 言語フィルタ (カンマ区切り、空欄=全言語)

**左側: ファイルツリー**
- 検出されたコードファイルの一覧表示
- ファイル名、言語、行数を表示
- DataGridで実装

**右側上部: 進捗セクション**
- 現在のフェーズ表示 (ファイルスキャン/コード解析/チャンク分割/要約生成/仕様書生成/Excel出力)
- 進捗メッセージ
- プログレスバー (0-100%)

**右側下部: プレビューセクション**
- タブコントロールで8つのセクションを表示:
  1. 概要
  2. 機能仕様
  3. API仕様
  4. データ構造
  5. 依存関係
  6. テスト観点
  7. リスク
  8. 変更履歴

**下部: 制御ボタン**
- 開始: 解析を開始
- 一時停止: 処理を一時停止 (UI実装済み、ロジックは今後実装)
- 再開: 一時停止した処理を再開 (UI実装済み、ロジックは今後実装)
- キャンセル: 処理をキャンセル
- 設定: 設定ウィンドウを開く

#### 設定画面 (SettingsWindow)

**LLM設定タブ**
- プロバイダ選択 (Azure OpenAI / ローカルLLM)
- Azure OpenAI 設定:
  - エンドポイント
  - APIキー
  - デプロイ名
  - 接続テスト (UI実装済み、ロジックは今後実装)
- LLMパラメータ:
  - Temperature (0.0 - 2.0、スライダー)
  - 最大トークン数 (1000 - 8000、スライダー)

**処理設定タブ**
- チャンクサイズ (1000 - 8000)
- 並列度 (1 - 8) (UI実装済み、ロジックは今後実装)
- 再試行回数 (0 - 5) (UI実装済み、ロジックは今後実装)
- ログ保存先フォルダ選択

### 5. データバインディング

WPFのデータバインディング機能を活用し、UIとViewModelを連携:

```xaml
<TextBox Text="{Binding TargetFolderPath}" IsReadOnly="True"/>
<Button Command="{Binding SelectTargetFolderCommand}"/>
<ProgressBar Value="{Binding ProgressPercentage}" Maximum="100"/>
<TextBlock Text="{Binding ProgressMessage}"/>
```

### 6. コマンドパターン

`RelayCommand` を使用してボタンのクリックイベントをViewModelのメソッドにバインド:

```csharp
[RelayCommand]
private void SelectTargetFolder()
{
    // フォルダ選択ダイアログを表示
}

[RelayCommand(CanExecute = nameof(CanStart))]
private async Task StartAnalysisAsync()
{
    // 解析を開始
}
```

### 7. 既存機能の再利用

CLI版のロジックを最大限再利用し、コードの重複を避けました:

- `LanguageDetector`: 言語検出
- `CodeAnalyzer`: コード解析
- `LlmService`: LLM統合
- `ExcelExporter`: Excel出力

ViewModelでこれらのサービスをDIコンテナ的に注入し、UIから呼び出します。

```csharp
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
```

### 8. エラーハンドリング

適切なエラーハンドリングを実装:

- try-catch ブロックでエラーをキャッチ
- ユーザーフレンドリーなエラーメッセージをMessageBoxで表示
- ロギングでエラーの詳細を記録

```csharp
catch (OperationCanceledException)
{
    ProgressMessage = "処理がキャンセルされました";
    _logger.LogInformation("Analysis cancelled by user");
}
catch (Exception ex)
{
    ProgressMessage = $"エラー: {ex.Message}";
    _logger.LogError(ex, "Error during analysis");
    System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}", 
        "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

## 実装の特徴

### 1. 最小限の変更

要件に従い、既存のCoreロジックはそのまま使用し、GUIはラッパーとして実装しました。既存のコードに対する変更は最小限です。

### 2. 非同期実行と安定性

- すべての重い処理は非同期で実行
- UIスレッドをブロックしないため、アプリケーションは常に応答可能
- キャンセルトークンでユーザーが処理を中断可能

### 3. プレビュー機能

解析中・解析後にプレビューエリアで仕様書の各セクションを確認できます。これにより、Excel出力前に内容を確認できます。

### 4. 進捗表示

詳細な進捗表示により、ユーザーは現在の処理状況を把握できます:
- フェーズ名
- 進捗メッセージ
- プログレスバー (0-100%)

### 5. 設定の永続化

環境変数を使用してLLM設定を保存します。これにより、アプリケーションを再起動しても設定が保持されます。

## 今後の実装予定

以下の機能はUI実装済みですが、ロジックは今後実装予定です:

- [ ] 一時停止/再開機能の実装
- [ ] 並列処理の実装
- [ ] リトライ機能の実装
- [ ] 接続テスト機能の実装
- [ ] 設定のファイル保存 (永続化)
- [ ] チェックポイント機能
- [ ] ダークモード対応
- [ ] 差分解析機能
- [ ] エクスポート形式の追加 (PDF, Markdown)

## ビルドと実行

### ビルド

```bash
# WPFプロジェクトのみをビルド
dotnet build src/SpecDocGenerator.WPF

# ソリューション全体をビルド
dotnet build
```

### 実行

```bash
# 開発モードで実行
cd src/SpecDocGenerator.WPF
dotnet run

# リリースビルドを作成して実行
dotnet build -c Release
# Windows上で生成された .exe を実行
```

### 注意点

- このアプリケーションはWindows専用です (WPFはWindowsのみサポート)
- クロスプラットフォーム対応が必要な場合は、Avalonia UIなどの代替UIフレームワークを検討してください
- Linux/macOS上でビルドする場合、`EnableWindowsTargeting=true` を設定する必要があります

## テスト

現時点では、GUIアプリケーションのユニットテストは実装されていません。今後、以下のテストを追加する予定です:

- ViewModelのユニットテスト
- コマンドの動作テスト
- データバインディングの検証

## まとめ

要件定義に従い、以下を実現しました:

✅ **既存CUIロジックの再利用**: Coreライブラリをそのまま使用  
✅ **GUIラッパー化**: MVVMパターンでUIとロジックを分離  
✅ **非同期実行**: async/await で安定した非同期処理  
✅ **進捗通知**: 詳細な進捗表示とプログレスバー  
✅ **中断対応**: キャンセルトークンで処理を中断可能  
✅ **長文対応**: 既存のチャンク分割・階層要約ロジックを踏襲  
✅ **LLM設定**: 設定画面でモデル、温度、最大トークン、APIキーを変更可能  
✅ **画面仕様**: フォルダ選択、ファイルツリー、進捗バー、プレビュー、制御ボタンを実装

Windows環境でのデスクトップアプリケーションとして、ユーザーフレンドリーなGUIを提供します。
