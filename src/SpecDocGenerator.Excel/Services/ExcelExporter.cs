using SpecDocGenerator.Core.Models;
using SpecDocGenerator.Core.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace SpecDocGenerator.Excel.Services;

/// <summary>
/// Excel export service using ClosedXML
/// </summary>
public class ExcelExporter : IExcelExporter
{
    private readonly ILogger<ExcelExporter> _logger;

    public ExcelExporter(ILogger<ExcelExporter> logger)
    {
        _logger = logger;
    }

    public async Task ExportAsync(SpecificationDocument document, string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var workbook = new XLWorkbook();

            // Create all sheets
            CreateOverviewSheet(workbook, document);
            CreateFunctionSpecsSheet(workbook, document);
            CreateApiSpecsSheet(workbook, document);
            CreateDataStructuresSheet(workbook, document);
            CreateDependenciesSheet(workbook, document);
            CreateFileListSheet(workbook, document);
            CreateTestConsiderationsSheet(workbook, document);
            CreateRisksSheet(workbook, document);
            CreateChangeHistorySheet(workbook, document);

            // Save the workbook
            await Task.Run(() => workbook.SaveAs(outputPath), cancellationToken);

            _logger.LogInformation("Successfully exported specification to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting specification to {OutputPath}", outputPath);
            throw;
        }
    }

    private void CreateOverviewSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("01_概要");

        // Add title
        worksheet.Cell(1, 1).Value = "項目";
        worksheet.Cell(1, 2).Value = "内容";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 2);
        FormatHeader(headerRange);

        // Add overview data
        int row = 2;
        foreach (var kvp in document.Overview)
        {
            worksheet.Cell(row, 1).Value = kvp.Key;
            worksheet.Cell(row, 2).Value = kvp.Value;
            worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
            row++;
        }

        // Auto-fit columns
        worksheet.Column(1).AdjustToContents();
        worksheet.Column(2).Width = 80;
    }

    private void CreateFunctionSpecsSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("02_機能仕様");

        // Add headers
        worksheet.Cell(1, 1).Value = "機能ID";
        worksheet.Cell(1, 2).Value = "機能名";
        worksheet.Cell(1, 3).Value = "概要";
        worksheet.Cell(1, 4).Value = "入力";
        worksheet.Cell(1, 5).Value = "出力";
        worksheet.Cell(1, 6).Value = "前提・制約";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 6);
        FormatHeader(headerRange);

        // Add function specs
        int row = 2;
        foreach (var spec in document.FunctionSpecs)
        {
            worksheet.Cell(row, 1).Value = spec.FunctionId;
            worksheet.Cell(row, 2).Value = spec.FunctionName;
            worksheet.Cell(row, 3).Value = spec.Overview;
            worksheet.Cell(row, 4).Value = spec.Input;
            worksheet.Cell(row, 5).Value = spec.Output;
            worksheet.Cell(row, 6).Value = spec.Preconditions;

            // Wrap text for longer content
            for (int col = 3; col <= 6; col++)
            {
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            }

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 6);
    }

    private void CreateApiSpecsSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("03_API仕様");

        // Add headers
        worksheet.Cell(1, 1).Value = "分類";
        worksheet.Cell(1, 2).Value = "名前";
        worksheet.Cell(1, 3).Value = "入力";
        worksheet.Cell(1, 4).Value = "出力";
        worksheet.Cell(1, 5).Value = "説明";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add API specs
        int row = 2;
        foreach (var spec in document.ApiSpecs)
        {
            worksheet.Cell(row, 1).Value = spec.Category;
            worksheet.Cell(row, 2).Value = spec.Name;
            worksheet.Cell(row, 3).Value = spec.Input;
            worksheet.Cell(row, 4).Value = spec.Output;
            worksheet.Cell(row, 5).Value = spec.Description;

            // Wrap text for description
            worksheet.Cell(row, 5).Style.Alignment.WrapText = true;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateDataStructuresSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("04_データ構造");

        // Add headers
        worksheet.Cell(1, 1).Value = "クラス";
        worksheet.Cell(1, 2).Value = "フィールド";
        worksheet.Cell(1, 3).Value = "型";
        worksheet.Cell(1, 4).Value = "説明";
        worksheet.Cell(1, 5).Value = "参照元";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add data structures
        int row = 2;
        foreach (var ds in document.DataStructures)
        {
            worksheet.Cell(row, 1).Value = ds.ClassName;
            worksheet.Cell(row, 2).Value = ds.FieldName;
            worksheet.Cell(row, 3).Value = ds.DataType;
            worksheet.Cell(row, 4).Value = ds.Description;
            worksheet.Cell(row, 5).Value = ds.References;

            worksheet.Cell(row, 4).Style.Alignment.WrapText = true;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateDependenciesSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("05_依存関係");

        // Add headers
        worksheet.Cell(1, 1).Value = "モジュール";
        worksheet.Cell(1, 2).Value = "依存先";
        worksheet.Cell(1, 3).Value = "内部/外部";
        worksheet.Cell(1, 4).Value = "バージョン";
        worksheet.Cell(1, 5).Value = "備考";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add dependencies
        int row = 2;
        foreach (var dep in document.Dependencies)
        {
            worksheet.Cell(row, 1).Value = dep.Module;
            worksheet.Cell(row, 2).Value = dep.DependsOn;
            worksheet.Cell(row, 3).Value = dep.Type;
            worksheet.Cell(row, 4).Value = dep.Version;
            worksheet.Cell(row, 5).Value = dep.Notes;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateFileListSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("06_ファイル一覧");

        // Add headers
        worksheet.Cell(1, 1).Value = "パス";
        worksheet.Cell(1, 2).Value = "言語";
        worksheet.Cell(1, 3).Value = "行数";
        worksheet.Cell(1, 4).Value = "最終更新";
        worksheet.Cell(1, 5).Value = "備考";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add file list
        int row = 2;
        foreach (var file in document.FileList)
        {
            worksheet.Cell(row, 1).Value = file.Path;
            worksheet.Cell(row, 2).Value = file.Language;
            worksheet.Cell(row, 3).Value = file.LineCount;
            worksheet.Cell(row, 4).Value = file.LastModified.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 5).Value = file.Notes;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateTestConsiderationsSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("07_テスト観点");

        // Add headers
        worksheet.Cell(1, 1).Value = "観点ID";
        worksheet.Cell(1, 2).Value = "機能";
        worksheet.Cell(1, 3).Value = "期待結果";
        worksheet.Cell(1, 4).Value = "前提";
        worksheet.Cell(1, 5).Value = "補足";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add test considerations
        int row = 2;
        foreach (var test in document.TestConsiderations)
        {
            worksheet.Cell(row, 1).Value = test.TestId;
            worksheet.Cell(row, 2).Value = test.FunctionName;
            worksheet.Cell(row, 3).Value = test.ExpectedResult;
            worksheet.Cell(row, 4).Value = test.Preconditions;
            worksheet.Cell(row, 5).Value = test.Notes;

            // Wrap text for longer content
            for (int col = 3; col <= 5; col++)
            {
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            }

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateRisksSheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("08_リスク");

        // Add headers
        worksheet.Cell(1, 1).Value = "リスクID";
        worksheet.Cell(1, 2).Value = "内容";
        worksheet.Cell(1, 3).Value = "影響";
        worksheet.Cell(1, 4).Value = "確率";
        worksheet.Cell(1, 5).Value = "対策";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 5);
        FormatHeader(headerRange);

        // Add risks
        int row = 2;
        foreach (var risk in document.Risks)
        {
            worksheet.Cell(row, 1).Value = risk.RiskId;
            worksheet.Cell(row, 2).Value = risk.Content;
            worksheet.Cell(row, 3).Value = risk.Impact;
            worksheet.Cell(row, 4).Value = risk.Probability;
            worksheet.Cell(row, 5).Value = risk.Mitigation;

            // Wrap text for content and mitigation
            worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
            worksheet.Cell(row, 5).Style.Alignment.WrapText = true;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 5);
    }

    private void CreateChangeHistorySheet(XLWorkbook workbook, SpecificationDocument document)
    {
        var worksheet = workbook.Worksheets.Add("09_変更履歴");

        // Add headers
        worksheet.Cell(1, 1).Value = "版";
        worksheet.Cell(1, 2).Value = "日付";
        worksheet.Cell(1, 3).Value = "変更内容";
        worksheet.Cell(1, 4).Value = "担当";

        // Format header
        var headerRange = worksheet.Range(1, 1, 1, 4);
        FormatHeader(headerRange);

        // Add change history
        int row = 2;
        foreach (var change in document.ChangeHistory)
        {
            worksheet.Cell(row, 1).Value = change.Version;
            worksheet.Cell(row, 2).Value = change.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 3).Value = change.Changes;
            worksheet.Cell(row, 4).Value = change.Author;

            worksheet.Cell(row, 3).Style.Alignment.WrapText = true;

            row++;
        }

        // Auto-fit columns
        AdjustColumns(worksheet, 4);
    }

    private void FormatHeader(IXLRange headerRange)
    {
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private void AdjustColumns(IXLWorksheet worksheet, int columnCount)
    {
        for (int i = 1; i <= columnCount; i++)
        {
            var column = worksheet.Column(i);
            column.AdjustToContents(1, 1, 200); // Max width 200
            
            // Set minimum width
            if (column.Width < 10)
            {
                column.Width = 10;
            }
            
            // Set maximum width for readability
            if (column.Width > 80)
            {
                column.Width = 80;
            }
        }
    }
}
