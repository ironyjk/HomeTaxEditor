namespace HomeTaxEditor.Core.Models;

/// <summary>
/// 웹 테이블에서 추출한 행 데이터
/// </summary>
public class WebTableRow
{
    public int RowIndex { get; set; }
    public string AprvDt { get; set; } = "";
    public string BusnCrdCardNoEncCntn { get; set; } = "";
    public string MrntTxprDscmNoEncCntn { get; set; } = "";
    public string MrntNm { get; set; } = ""; // 가맹점명(상호명)
    public decimal TotaTrsAmt { get; set; }
    public string CurrentDdcYnNm { get; set; } = "";

    // 디버깅용
    public override string ToString()
    {
        return $"[{RowIndex}] {AprvDt} | {BusnCrdCardNoEncCntn} | {MrntNm} | {MrntTxprDscmNoEncCntn} | {TotaTrsAmt:N0} | {CurrentDdcYnNm}";
    }
}
