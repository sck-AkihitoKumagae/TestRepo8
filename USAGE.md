# プログラム仕様書作成アプリケーション 使用ガイド

## インストール

### 前提条件
- .NET 8.0 SDK 以上がインストールされていること
- （オプション）Azure OpenAI アカウント（高度なLLM解析を使用する場合）

### ビルド手順

```bash
# 1. リポジトリをクローン
git clone https://github.com/sck-AkihitoKumagae/TestRepo8.git
cd TestRepo8

# 2. ビルド
dotnet build

# 3. （オプション）実行可能ファイルを作成
dotnet publish src/SpecDocGenerator.CLI/SpecDocGenerator.CLI.csproj -c Release -o ./publish
```

## 基本的な使い方

### 方法1: インタラクティブモード

```bash
cd src/SpecDocGenerator.CLI
dotnet run
```

プロンプトに従って以下を入力します：
1. 分析対象のフォルダパス
2. 出力先のExcelファイルパス（省略可）

### 方法2: コマンドライン引数

```bash
cd src/SpecDocGenerator.CLI
dotnet run <分析対象フォルダパス> [出力先Excelファイルパス]
```

**例：**
```bash
# カレントディレクトリを解析
dotnet run .

# 特定のフォルダを解析し、カスタムファイル名で保存
dotnet run /path/to/project output/仕様書_v1.0.xlsx

# 相対パスも使用可能
dotnet run ../../my-project ./specs.xlsx
```

### 方法3: 発行済み実行ファイルを使用

```bash
# ビルド後
./publish/SpecDocGenerator.CLI /path/to/project output.xlsx
```

## Azure OpenAI の設定（オプション）

より高度なコード解析とLLMによる要約を使用するには、Azure OpenAI を設定します。

### 環境変数の設定

#### Windows (PowerShell)
```powershell
$env:AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_KEY="your-api-key-here"
$env:AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

#### Windows (コマンドプロンプト)
```cmd
set AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
set AZURE_OPENAI_KEY=your-api-key-here
set AZURE_OPENAI_DEPLOYMENT=gpt-4
```

#### Linux/Mac
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_KEY="your-api-key-here"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

### Azure OpenAI リソースの作成

1. Azure Portal にログイン
2. Azure OpenAI Service リソースを作成
3. デプロイメントを作成（例：gpt-4, gpt-35-turbo）
4. エンドポイントとキーを取得
5. 上記の環境変数に設定

**注意：** Azure OpenAI を設定しない場合でも、基本的なコード解析と仕様書生成は可能です。

## 対応言語

以下の言語のファイルを自動検出し、解析します：

- **C# (.cs)**
- **Python (.py)**
- **JavaScript (.js, .jsx)**
- **TypeScript (.ts, .tsx)**
- **Java (.java)**
- **C++ (.cpp, .cc, .cxx, .hpp)**
- **C (.c, .h)**
- **Go (.go)**
- **Rust (.rs)**
- **Ruby (.rb)**
- **PHP (.php)**
- **Swift (.swift)**
- **Kotlin (.kt, .kts)**
- **Scala (.scala)**
- **HTML (.html, .htm)**
- **CSS (.css)**
- **SQL (.sql)**

## 出力される仕様書の構成

生成される Excel ファイルには以下の9つのシートが含まれます：

1. **01_概要** - プロジェクトの概要情報
   - プロジェクト名、ファイル数、総行数、使用言語など

2. **02_機能仕様** - 関数/メソッドの仕様
   - 機能ID、機能名、概要、入出力、前提条件

3. **03_API仕様** - 公開APIの詳細
   - 分類、名前、入力、出力、説明

4. **04_データ構造** - クラスとデータ構造
   - クラス名、フィールド名、型、説明、参照元

5. **05_依存関係** - モジュール間の依存関係
   - モジュール、依存先、内部/外部、バージョン

6. **06_ファイル一覧** - プロジェクト内のファイル情報
   - パス、言語、行数、最終更新日

7. **07_テスト観点** - テストの観点
   - 観点ID、機能、期待結果、前提条件

8. **08_リスク** - プロジェクトのリスク分析
   - リスクID、内容、影響、確率、対策

9. **09_変更履歴** - 仕様書の変更履歴
   - 版、日付、変更内容、担当者

## 除外されるディレクトリ

以下のディレクトリは自動的に解析対象から除外されます：

- `node_modules` - Node.js 依存関係
- `bin`, `obj` - ビルド出力
- `dist`, `build` - ビルド成果物
- `.git`, `.svn` - バージョン管理
- `vendor`, `packages` - パッケージ
- `__pycache__`, `.venv`, `venv` - Python 環境
- `target` - Rust/Java ビルド

## トラブルシューティング

### エラー: 「有効なフォルダパスを指定してください」

**原因:** 指定されたパスが存在しないか、アクセス権限がありません。

**解決策:**
- パスが正しいことを確認
- 絶対パスを使用してみる
- フォルダへのアクセス権限を確認

### 警告: 「LLM service not configured」

**原因:** Azure OpenAI の環境変数が設定されていません。

**影響:** 基本的な解析のみが実行されます。LLM による高度な要約は生成されません。

**解決策（オプション）:**
- Azure OpenAI の環境変数を設定（上記参照）
- または、基本解析で続行

### エラー: 「Cannot divide by zero」のような解析エラー

**原因:** コードファイルの読み込みまたは解析中にエラーが発生しました。

**解決策:**
- エラーメッセージを確認
- 問題のあるファイルを特定
- 必要に応じてそのファイルを除外またはテキストエンコーディングを確認

### Excel ファイルが開けない

**原因:** 
- ファイルが既に開かれている
- 書き込み権限がない
- ディスク容量不足

**解決策:**
- 他のプログラムで開いている Excel ファイルを閉じる
- 別の場所に保存してみる
- ディスク容量を確認

## 高度な使用方法

### カスタム設定ファイル

`appsettings.json` を編集することで、以下をカスタマイズできます：

```json
{
  "LLM": {
    "Temperature": 0.3,
    "MaxTokens": 4000
  },
  "Analysis": {
    "MaxChunkSize": 4000,
    "ExcludedDirectories": ["custom_exclude"]
  }
}
```

### 複数のプロジェクトを連続処理

バッチスクリプトを作成して複数のプロジェクトを処理：

```bash
#!/bin/bash
projects=("project1" "project2" "project3")
for proj in "${projects[@]}"; do
    dotnet run "./$proj" "specs_$proj.xlsx"
done
```

## パフォーマンスのヒント

- **大規模プロジェクト**: 100MB以上のプロジェクトでは処理に数分かかる場合があります
- **並列処理**: 現在のバージョンは並列処理に対応していませんが、将来のバージョンで追加予定
- **メモリ**: 大量のファイルを解析する場合、十分なメモリを確保してください

## サポートとフィードバック

- **Issues**: [GitHub Issues](https://github.com/sck-AkihitoKumagae/TestRepo8/issues)
- **プルリクエスト**: 貢献を歓迎します！

## ライセンス

MIT License - 詳細は LICENSE ファイルを参照してください。
