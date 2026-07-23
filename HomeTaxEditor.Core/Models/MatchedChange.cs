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

    /// <summary>
    /// 같은 날짜·거래처·금액인데 공제/불공제가 갈리는 그룹(후불하이패스 등, 카드로만 구분)을
    /// '순서'로 자동판정한 건. 카드번호가 웹에 안 보여 100% 확실하진 않으므로 사람이 확인해야 함.
    /// </summary>
    public bool NeedsManualReview { get; set; }

    // 디버깅용
    public override string ToString()
    {
        return $"[Row {RowIndex}] {ExcelData.승인일자} {ExcelData.합계:N0} => {공제여부}";
    }
}
