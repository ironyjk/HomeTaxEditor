using HomeTaxEditor.Core.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;

namespace HomeTaxEditor.Core.Services;

public class ExcelReader : IExcelReader
{
    public List<CardTransactionData> ReadExcelFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();

        return extension switch
        {
            ".xls" => ReadXlsFile(filePath),
            ".xlsx" => ReadXlsxFile(filePath),
            _ => throw new NotSupportedException($"지원하지 않는 파일 형식입니다: {extension}")
        };
    }

    /// <summary>
    /// .xls 파일 읽기 (NPOI 사용)
    /// </summary>
    private List<CardTransactionData> ReadXlsFile(string filePath)
    {
        var result = new List<CardTransactionData>();

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new HSSFWorkbook(fileStream);
            ISheet sheet = workbook.GetSheetAt(0);

            // 헤더 행 찾기 (첫 10행 내에서 검색)
            int headerRowIndex = -1;
            Dictionary<string, int> columnIndices = new Dictionary<string, int>();

            for (int i = 0; i < Math.Min(10, sheet.LastRowNum + 1); i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var indices = FindColumnIndices(row);
                if (ValidateColumnIndices(indices, out _))
                {
                    headerRowIndex = i;
                    columnIndices = indices;
                    break;
                }
            }

            if (headerRowIndex == -1)
            {
                throw new InvalidDataException($"필수 컬럼이 없습니다: 승인일자, 카드번호, 가맹점사업자번호, 합계, 공제여부결정");
            }

            // 데이터 행 읽기 (헤더 다음 행부터)
            for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                var data = ExtractRowData(row, columnIndices);
                if (data != null)
                {
                    result.Add(data);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// .xlsx 파일 읽기 (NPOI 사용 - XSSF)
    /// </summary>
    private List<CardTransactionData> ReadXlsxFile(string filePath)
    {
        var result = new List<CardTransactionData>();

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fileStream);
            ISheet sheet = workbook.GetSheetAt(0);

            // 헤더 행 찾기 (첫 10행 내에서 검색)
            int headerRowIndex = -1;
            Dictionary<string, int> columnIndices = new Dictionary<string, int>();

            for (int i = 0; i < Math.Min(10, sheet.LastRowNum + 1); i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var indices = FindColumnIndices(row);
                if (ValidateColumnIndices(indices, out _))
                {
                    headerRowIndex = i;
                    columnIndices = indices;
                    break;
                }
            }

            if (headerRowIndex == -1)
            {
                throw new InvalidDataException($"필수 컬럼이 없습니다: 승인일자, 카드번호, 가맹점사업자번호, 합계, 공제여부결정");
            }

            // 데이터 행 읽기 (헤더 다음 행부터)
            for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                var data = ExtractRowData(row, columnIndices);
                if (data != null)
                {
                    result.Add(data);
                }
            }
        }

        return result;
    }

    private Dictionary<string, int> FindColumnIndices(IRow headerRow)
    {
        var indices = new Dictionary<string, int>();

        for (int cellIndex = 0; cellIndex < headerRow.LastCellNum; cellIndex++)
        {
            var cell = headerRow.GetCell(cellIndex);
            if (cell == null) continue;

            var headerValue = cell.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(headerValue)) continue;

            switch (headerValue)
            {
                case "승인일자":
                    indices["승인일자"] = cellIndex;
                    break;
                case "카드번호":
                    indices["카드번호"] = cellIndex;
                    break;
                case "가맹점사업자번호":
                    indices["가맹점사업자번호"] = cellIndex;
                    break;
                case "합계":
                    indices["합계"] = cellIndex;
                    break;
                case "공제여부결정":
                    indices["공제여부결정"] = cellIndex;
                    break;
            }
        }

        return indices;
    }

    private bool ValidateColumnIndices(Dictionary<string, int> indices, out List<string> missingColumns)
    {
        var requiredColumns = new[] { "승인일자", "카드번호", "가맹점사업자번호", "합계", "공제여부결정" };
        missingColumns = requiredColumns.Where(col => !indices.ContainsKey(col)).ToList();
        return missingColumns.Count == 0;
    }

    private CardTransactionData? ExtractRowData(IRow row, Dictionary<string, int> columnIndices)
    {
        try
        {
            var 승인일자 = GetCellValue(row, columnIndices["승인일자"]);
            var 카드번호 = GetCellValue(row, columnIndices["카드번호"]);
            var 가맹점사업자번호 = GetCellValue(row, columnIndices["가맹점사업자번호"]);
            var 합계Text = GetCellValue(row, columnIndices["합계"]);
            var 공제여부결정 = GetCellValue(row, columnIndices["공제여부결정"]);

            // 빈 행 건너뛰기
            if (string.IsNullOrWhiteSpace(승인일자) && string.IsNullOrWhiteSpace(카드번호))
            {
                return null;
            }

            // 합계를 decimal로 변환
            decimal 합계 = 0;
            if (!string.IsNullOrWhiteSpace(합계Text))
            {
                var cleanedText = 합계Text.Replace(",", "").Replace(" ", "");
                if (decimal.TryParse(cleanedText, out decimal parsedValue))
                {
                    합계 = parsedValue;
                }
            }

            return new CardTransactionData
            {
                승인일자 = 승인일자,
                카드번호 = 카드번호,
                가맹점사업자번호 = 가맹점사업자번호,
                합계 = 합계,
                공제여부결정 = 공제여부결정
            };
        }
        catch
        {
            return null;
        }
    }

    private string GetCellValue(IRow row, int cellIndex)
    {
        var cell = row.GetCell(cellIndex);
        if (cell == null) return "";

        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue?.Trim() ?? "",
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => cell.StringCellValue?.Trim() ?? "",
            _ => cell.ToString()?.Trim() ?? ""
        };
    }
}
