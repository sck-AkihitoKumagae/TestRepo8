# アーキテクチャドキュメント / Architecture Documentation

## システム概要 / System Overview

このアプリケーションは、プログラムコードを解析し、仕様書を自動生成する .NET 8 ベースのシステムです。

## アーキテクチャ図 / Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     SpecDocGenerator.CLI                     │
│                  (Command Line Interface)                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌────────────────┐ ┌──────────────┐ ┌───────────────┐
│  Analysis      │ │     LLM      │ │    Excel      │
│   Service      │ │   Service    │ │   Service     │
└────────┬───────┘ └──────┬───────┘ └───────┬───────┘
         │                │                 │
         └────────────────┼─────────────────┘
                          │
                          ▼
                 ┌────────────────┐
                 │      Core      │
                 │    Models &    │
                 │  Interfaces    │
                 └────────────────┘
```

## プロジェクト構成 / Project Structure

### 1. SpecDocGenerator.Core

**役割**: コアモデルとインターフェースの定義

**主要コンポーネント**:
- `Models/`: データモデル
  - `CodeFile`: ソースコードファイル
  - `CodeElement`: コード要素（関数、クラス等）
  - `CodeChunk`: 処理用のコードチャンク
  - `Summary`: LLM生成サマリー
  - `SpecificationDocument`: 最終的な仕様書
  
- `Services/`: サービスインターフェース
  - `ILanguageDetector`: 言語検出
  - `ICodeAnalyzer`: コード解析
  - `ILlmService`: LLM統合
  - `IExcelExporter`: Excel出力

### 2. SpecDocGenerator.Analysis

**役割**: コード解析エンジン

**主要クラス**:
- `LanguageDetector`: ファイル拡張子による言語判定
- `CodeAnalyzer`: 正規表現ベースのコード解析
  - C#, Python, JavaScript, Java 対応
  - 関数、クラス、依存関係の抽出
  - チャンク分割機能

**解析フロー**:
1. ファイルスキャン
2. 言語判定
3. 構文解析（正規表現）
4. 依存関係抽出
5. チャンク化

### 3. SpecDocGenerator.LLM

**役割**: LLM統合とサマリー生成

**主要機能**:
- Azure OpenAI 統合（準備済み）
- 基本的なサマリー生成（LLMなし）
- 階層的要約
- プロンプトテンプレート管理

**処理フロー**:
1. チャンク単位の要約（Map）
2. 階層的集約（Reduce）
3. 仕様書の構造化

### 4. SpecDocGenerator.Excel

**役割**: Excel ファイル生成

**使用ライブラリ**: ClosedXML

**生成シート**:
1. 概要
2. 機能仕様
3. API仕様
4. データ構造
5. 依存関係
6. ファイル一覧
7. テスト観点
8. リスク
9. 変更履歴

**スタイリング**:
- ヘッダー: 太字 + グレー背景
- セル内改行対応
- 自動列幅調整
- ボーダー適用

### 5. SpecDocGenerator.CLI

**役割**: コマンドラインインターフェース

**機能**:
- インタラクティブモード
- コマンドライン引数サポート
- 進捗表示
- エラーハンドリング
- 日本語/英語メッセージ

## データフロー / Data Flow

```
1. ファイル入力
   ↓
2. LanguageDetector → CodeFile[]
   ↓
3. CodeAnalyzer → AnalysisResult
   ↓
4. ChunkGenerator → CodeChunk[]
   ↓
5. LlmService → Summary[]
   ↓
6. SpecificationGenerator → SpecificationDocument
   ↓
7. ExcelExporter → Excel File
```

## 設計パターン / Design Patterns

### 1. Dependency Injection
- インターフェースベースの設計
- 疎結合なコンポーネント
- テスタビリティの向上

### 2. Strategy Pattern
- 言語ごとの解析戦略
- LLMプロバイダーの切り替え

### 3. Template Method
- Excel シート生成のテンプレート化
- プロンプトテンプレート

### 4. Builder Pattern
- SpecificationDocument の構築

## 拡張性 / Extensibility

### 新しい言語の追加

1. `ProgrammingLanguage` enum に言語を追加
2. `LanguageDetector` に拡張子マッピングを追加
3. `CodeAnalyzer` に解析ロジックを追加

```csharp
// Example
private List<CodeElement> AnalyzeNewLanguage(CodeFile file)
{
    // Custom parsing logic
}
```

### 新しいLLMプロバイダーの追加

1. `ILlmService` を実装
2. プロバイダー固有のクライアント統合
3. 設定管理

```csharp
public class OllamaLlmService : ILlmService
{
    // Ollama implementation
}
```

### 新しい出力形式の追加

1. 新しいインターフェース定義
2. エクスポーター実装

```csharp
public interface IPdfExporter
{
    Task ExportAsync(SpecificationDocument document, string outputPath);
}
```

## パフォーマンス考慮事項 / Performance Considerations

### 現在の実装
- **同期処理**: 順次ファイル処理
- **メモリ**: すべてのファイルをメモリに読み込み
- **チャンク化**: 4000トークン（約16KB）単位

### 最適化の余地
- **並列処理**: `Parallel.ForEach` によるファイル並列解析
- **ストリーミング**: 大容量ファイルのストリーミング読み込み
- **キャッシング**: 解析結果のキャッシュ
- **インクリメンタル**: 変更ファイルのみの再解析

## セキュリティ / Security

### 現在の対策
- ローカル処理（オプション）
- 環境変数による認証情報管理
- ファイルアクセス検証

### 今後の強化
- PII検出とマスキング
- 秘密情報のスキャン
- サンドボックス実行
- アクセス制御

## エラーハンドリング / Error Handling

### 戦略
1. **Try-Catch**: 各レイヤーで適切な例外処理
2. **ロギング**: Microsoft.Extensions.Logging
3. **グレースフルデグラデーション**: LLM利用不可時の基本機能
4. **ユーザーフレンドリー**: 明確なエラーメッセージ

### エラーカテゴリ
- ファイルアクセスエラー
- 解析エラー
- LLMエラー（タイムアウト、API制限）
- Excel出力エラー

## 今後の改善計画 / Future Improvements

### Phase 1: 基盤強化
- [ ] 単体テスト追加（カバレッジ 80%以上）
- [ ] 統合テスト
- [ ] ドキュメント拡充

### Phase 2: 機能拡張
- [ ] Tree-sitter 統合（高精度構文解析）
- [ ] より多くの言語対応
- [ ] 並列処理
- [ ] 差分生成

### Phase 3: UI実装
- [ ] WPF/Avalonia デスクトップアプリ
- [ ] リアルタイムプレビュー
- [ ] プロンプトエディタ
- [ ] 設定管理UI

### Phase 4: 高度な機能
- [ ] ローカルLLM対応（Ollama）
- [ ] コードメトリクス
- [ ] 依存関係グラフ
- [ ] PDF/Markdown 出力

## 技術スタック / Tech Stack

- **言語**: C# 12
- **フレームワーク**: .NET 8
- **ライブラリ**:
  - ClosedXML: Excel生成
  - Microsoft.Extensions.Logging: ログ
  - Azure.AI.OpenAI: LLM統合（オプション）
  - Newtonsoft.Json: JSON処理

## 開発ツール / Development Tools

- **IDE**: Visual Studio 2022 / VS Code / Rider
- **バージョン管理**: Git
- **CI/CD**: GitHub Actions
- **パッケージ管理**: NuGet

## 参考資料 / References

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ClosedXML Documentation](https://github.com/ClosedXML/ClosedXML)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
