using HomeTaxEditor.Core.Models;

namespace HomeTaxEditor.Core.Services;

public class DataMatcher
{
    /// <summary>
    /// 엑셀 데이터와 웹 테이블 데이터를 매칭
    /// </summary>
    public List<MatchedChange> MatchData(
        List<CardTransactionData> excelData,
        List<WebTableRow> webData)
    {
        var changes = new List<MatchedChange>();

        foreach (var excelRow in excelData)
        {
            // 3가지 키로 매칭: 승인일자 + 가맹점사업자번호 + 합계
            // 카드번호는 웹 화면에 표시되지 않으므로 제외
            var matchedWebRows = webData.Where(w =>
                w.AprvDt == excelRow.승인일자 &&
                w.MrntTxprDscmNoEncCntn == excelRow.가맹점사업자번호 &&
                w.TotaTrsAmt == excelRow.합계
            ).ToList();

            // 매칭된 모든 행 처리 (중복 매칭 가능)
            foreach (var matchedWebRow in matchedWebRows)
            {
                // 간이과세자는 공제여부 변경 불가 - 건너뛰기
                if (!string.IsNullOrEmpty(matchedWebRow.MrntTyp) && matchedWebRow.MrntTyp.Contains("간이"))
                {
                    continue;
                }

                // 공제여부가 다른 경우에만 변경 목록에 추가
                if (matchedWebRow.CurrentDdcYnNm != excelRow.공제여부결정)
                {
                    changes.Add(new MatchedChange
                    {
                        RowIndex = matchedWebRow.RowIndex,
                        공제여부 = excelRow.공제여부결정,
                        ExcelData = excelRow,
                        WebData = matchedWebRow
                    });
                }
            }
        }

        return changes;
    }

    /// <summary>
    /// 매칭 통계 정보 반환
    /// </summary>
    public (int Total, int Matched, int NeedChange) GetMatchingStats(
        List<CardTransactionData> excelData,
        List<WebTableRow> webData,
        List<MatchedChange> changes)
    {
        var matched = 0;

        foreach (var excelRow in excelData)
        {
            // 3가지 키로 매칭: 승인일자 + 가맹점사업자번호 + 합계
            var hasMatch = webData.Any(w =>
                w.AprvDt == excelRow.승인일자 &&
                w.MrntTxprDscmNoEncCntn == excelRow.가맹점사업자번호 &&
                w.TotaTrsAmt == excelRow.합계
            );

            if (hasMatch) matched++;
        }

        return (excelData.Count, matched, changes.Count);
    }

    /// <summary>
    /// 간이과세자로 인해 제외된 건수 계산
    /// </summary>
    public int GetSkippedSimpleTaxpayerCount(
        List<CardTransactionData> excelData,
        List<WebTableRow> webData)
    {
        var skippedCount = 0;

        foreach (var excelRow in excelData)
        {
            var matchedWebRows = webData.Where(w =>
                w.AprvDt == excelRow.승인일자 &&
                w.MrntTxprDscmNoEncCntn == excelRow.가맹점사업자번호 &&
                w.TotaTrsAmt == excelRow.합계
            ).ToList();

            foreach (var matchedWebRow in matchedWebRows)
            {
                // 간이과세자이면서 공제여부가 다른 경우
                if (!string.IsNullOrEmpty(matchedWebRow.MrntTyp) &&
                    matchedWebRow.MrntTyp.Contains("간이") &&
                    matchedWebRow.CurrentDdcYnNm != excelRow.공제여부결정)
                {
                    skippedCount++;
                }
            }
        }

        return skippedCount;
    }
}
