# TestRepo8
プログラム仕様書作成アプリケーション

## 概要

さまざまなプログラムファイル（.py、.cpp、.js、.java 等）を読み込み、LLMで解析して「仕様書」を自動生成し、Excelファイルで出力するアプリケーションです。

## 機能

- **多言語対応**: C#, Python, JavaScript, TypeScript, Java, C/C++, Go, Rust, Ruby, PHP, Swift, Kotlin, Scala などに対応
- **自動解析**: コードから関数、クラス、依存関係を自動抽出
- **LLM統合**: Azure OpenAI を使用した高度なコード解析（オプション）
- **Excel出力**: 9シートからなる標準的な仕様書テンプレート
  - 01_概要
  - 02_機能仕様
  - 03_API仕様
  - 04_データ構造
  - 05_依存関係
  - 06_ファイル一覧
  - 07_テスト観点
  - 08_リスク
  - 09_変更履歴

## プロジェクト構成

```
SpecDocGenerator/
├── src/
│   ├── SpecDocGenerator.Core/       # コアモデルとインターフェース
│   ├── SpecDocGenerator.Analysis/   # コード解析エンジン
│   ├── SpecDocGenerator.LLM/        # LLM統合サービス
│   ├── SpecDocGenerator.Excel/      # Excel出力サービス
│   └── SpecDocGenerator.CLI/        # コマンドラインインターフェース
└── tests/                           # テストプロジェクト
```

## 必要要件

- .NET 8.0 SDK 以上
- （オプション）Azure OpenAI アカウント（高度な解析用）

## ビルド方法

```bash
# リポジトリをクローン
git clone https://github.com/sck-AkihitoKumagae/TestRepo8.git
cd TestRepo8

# ビルド
dotnet build

# 実行
cd src/SpecDocGenerator.CLI
dotnet run
```

## 使用方法

### 基本的な使用方法

```bash
# 対話モード
dotnet run

# コマンドライン引数で実行
dotnet run /path/to/your/project
```

### Azure OpenAI を使用する場合

環境変数を設定してください：

```bash
# Windows (PowerShell)
$env:AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_KEY="your-api-key"
$env:AZURE_OPENAI_DEPLOYMENT="gpt-4"

# Linux/Mac
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

### 出力例

アプリケーションは以下のような Excel ファイルを生成します：

- 各シートには適切な見出しとフォーマット
- セル内改行対応
- 列幅の自動調整
- ハイパーリンクによる相互参照（今後のバージョンで実装予定）

## アーキテクチャ

### 解析パイプライン

1. **ファイルスキャン**: 対象フォルダから対応する言語のファイルを検出
2. **言語判定**: 拡張子ベースで言語を自動判定
3. **コード解析**: 正規表現ベースで関数、クラス、依存関係を抽出
4. **チャンク化**: 大きなファイルを適切なサイズに分割
5. **要約生成**: LLM を使用して各チャンクを要約（オプション）
6. **階層集約**: 関数 → ファイル → モジュール → システムレベルで集約
7. **仕様書生成**: 標準テンプレートに基づいて仕様書を構築
8. **Excel出力**: ClosedXML を使用して Excel ファイルを生成

### 拡張性

- **ILanguageDetector**: 新しい言語の検出ロジックを追加
- **ICodeAnalyzer**: より高度な構文解析（AST パーサー統合）
- **ILlmService**: 異なる LLM プロバイダー（Ollama、他のクラウドサービス）
- **IExcelExporter**: 異なる出力フォーマット（PDF、Markdown など）

## セキュリティ

- ローカル解析: LLM を使用しない場合、すべての処理はローカルで完結
- オプショナルなクラウド連携: Azure OpenAI は明示的に設定した場合のみ使用
- 機密情報の保護: 環境変数による認証情報の管理

## 今後の機能拡張

- [ ] WPF/Avalonia UI の実装
- [ ] Tree-sitter による高度な構文解析
- [ ] チェックポイント/再開機能
- [ ] 差分生成（前回との比較）
- [ ] 用語集・略語表の自動生成
- [ ] コードメトリクス（複雑度、カバレッジ）
- [ ] ローカル LLM 対応（Ollama）

## ライセンス

MIT License

## 貢献

プルリクエストを歓迎します！

## サポート

Issue を作成してください。
