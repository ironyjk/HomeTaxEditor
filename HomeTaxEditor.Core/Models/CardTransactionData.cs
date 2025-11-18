namespace HomeTaxEditor.Core.Models;

/// <summary>
/// 엑셀 파일에서 읽은 카드 거래 데이터
/// </summary>
public class CardTransactionData
{
    public string 승인일자 { get; set; } = "";
    public string 카드번호 { get; set; } = "";
    public string 가맹점사업자번호 { get; set; } = "";
    public decimal 합계 { get; set; }
    public string 공제여부결정 { get; set; } = "";

    // 디버깅용
    public override string ToString()
    {
        return $"{승인일자} | {카드번호} | {가맹점사업자번호} | {합계:N0} | {공제여부결정}";
    }
}
