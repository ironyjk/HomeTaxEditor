namespace HomeTaxEditor.Core.Models;

/// <summary>
/// 한 페이지(웹 20건)에 대한 순서 기반 매칭 결과
/// </summary>
public class PageMatchResult
{
    /// <summary>실제로 적용할 변경 목록</summary>
    public List<MatchedChange> Changes { get; set; } = new();

    /// <summary>엑셀과 대응된 웹 행 수(변경 여부 무관)</summary>
    public int MatchedCount { get; set; }

    /// <summary>간이과세자라서 변경 제외된 건수</summary>
    public int SkippedSimpleTaxpayer { get; set; }
}
