# 貢献ガイドライン / Contributing Guidelines

## プルリクエストの提出方法 / How to Submit Pull Requests

1. リポジトリをフォーク / Fork the repository
2. 新しいブランチを作成 / Create a new branch
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. 変更を加える / Make your changes
4. テストを追加・実行 / Add and run tests
   ```bash
   dotnet test
   ```
5. コミット / Commit your changes
   ```bash
   git commit -m "Add feature: your feature description"
   ```
6. プッシュ / Push to your fork
   ```bash
   git push origin feature/your-feature-name
   ```
7. プルリクエストを作成 / Create a Pull Request

## コーディング規約 / Coding Standards

- C# のコーディング規約に従う / Follow C# coding conventions
- 適切なコメントを追加 / Add meaningful comments
- XMLドキュメントコメントを使用 / Use XML documentation comments
- 命名規則：
  - クラス名: PascalCase
  - メソッド名: PascalCase
  - 変数名: camelCase
  - 定数名: PascalCase

## 機能追加の提案 / Feature Suggestions

以下の分野での貢献を歓迎します：

### 優先度: 高 / High Priority

- **言語サポート拡張** / Extended Language Support
  - Tree-sitter 統合による高度な構文解析
  - より多くの言語への対応

- **UI実装** / UI Implementation
  - WPF または Avalonia UI のデスクトップアプリ
  - プログレスバーとリアルタイムプレビュー

- **テストカバレッジ** / Test Coverage
  - 単体テスト
  - 統合テスト
  - E2Eテスト

### 優先度: 中 / Medium Priority

- **LLM統合強化** / Enhanced LLM Integration
  - ローカルLLM対応（Ollama等）
  - カスタムプロンプトテンプレート

- **差分生成** / Diff Generation
  - 前回との比較機能
  - 変更点の自動検出

- **パフォーマンス最適化** / Performance Optimization
  - 並列処理
  - ストリーミング処理
  - メモリ効率化

### 優先度: 低 / Low Priority

- **追加機能** / Additional Features
  - コードメトリクス（複雑度、カバレッジ）
  - グラフ生成（依存関係図）
  - 他の出力フォーマット（PDF、Markdown）

## 開発環境のセットアップ / Development Setup

```bash
# 1. クローン
git clone https://github.com/sck-AkihitoKumagae/TestRepo8.git
cd TestRepo8

# 2. 依存関係の復元
dotnet restore

# 3. ビルド
dotnet build

# 4. テスト実行（今後追加予定）
dotnet test

# 5. 実行
cd src/SpecDocGenerator.CLI
dotnet run
```

## バグ報告 / Bug Reports

Issue を作成する際は、以下の情報を含めてください：

- **環境情報** / Environment
  - OS (Windows/Linux/Mac)
  - .NET バージョン
  - アプリケーションバージョン

- **再現手順** / Steps to Reproduce
  1. 具体的な手順
  2. 使用したコマンド
  3. 入力ファイル（可能であれば）

- **期待される動作** / Expected Behavior
- **実際の動作** / Actual Behavior
- **エラーメッセージ** / Error Messages
- **ログ** / Logs

## コードレビュー / Code Review

すべてのプルリクエストは以下の観点でレビューされます：

- コードの品質と可読性
- テストの網羅性
- ドキュメントの更新
- パフォーマンスへの影響
- セキュリティの考慮

## コミュニティ / Community

- 礼儀正しく、建設的なコミュニケーションを心がけましょう
- 他の貢献者を尊重しましょう
- オープンマインドでフィードバックを受け入れましょう

## ライセンス / License

貢献されたコードは MIT ライセンスの下で公開されます。

---

ご貢献ありがとうございます！ / Thank you for contributing!
