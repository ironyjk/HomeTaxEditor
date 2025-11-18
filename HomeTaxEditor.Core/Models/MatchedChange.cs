namespace HomeTaxEditor.Core.Models;

/// <summary>
/// 매칭된 변경사항
/// </summary>
public class MatchedChange
{
    public int RowIndex { get; set; }
    public string 공제여부 { get; set; } = "";
    public CardTransactionData ExcelData { get; set; } = null!;
    public WebTableRow WebData { get; set; } = null!;

    // 디버깅용
    public override string ToString()
    {
        return $"[Row {RowIndex}] {ExcelData.승인일자} {ExcelData.합계:N0} => {공제여부}";
    }
}
