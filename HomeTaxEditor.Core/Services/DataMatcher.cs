using System.Globalization;
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

    // ── 순서 기반(하이브리드) 매칭 ──────────────────────────────────────────
    //  키(날짜|사업자번호|금액)가 겹치는 건(후불하이패스 등)은, 카드번호가 웹 화면에
    //  안 보여 어느 웹 행이 어느 엑셀 행인지 값으로는 구분이 안 된다. 그래서 같은 키가
    //  여러 건이면 엑셀 "파일 순서대로" 웹 행에 1:1 배정한다(엑셀=홈택스 조회순서 가정).
    //  그런 그룹은 자동판정이 100% 확실하진 않으므로 NeedsManualReview로 표시한다.

    /// <summary>키(날짜|사업자번호|금액)를 정규화해 생성. 웹/엑셀 양쪽 동일 규칙.</summary>
    private static string MakeKey(string aprvDt, string bizNo, decimal amount)
        => $"{aprvDt}|{bizNo}|{amount.ToString("0.####", CultureInfo.InvariantCulture)}";

    /// <summary>
    /// 엑셀 전체를 키별 큐로 묶는다(각 큐는 파일 순서 유지).
    /// 페이지를 넘나들며 같은 키를 만날 때마다 순서대로 하나씩 꺼내 쓰기 위함.
    /// </summary>
    public Dictionary<string, Queue<CardTransactionData>> BuildExcelQueue(List<CardTransactionData> excelData)
    {
        var dict = new Dictionary<string, Queue<CardTransactionData>>();
        foreach (var e in excelData)
        {
            var key = MakeKey(e.승인일자, e.가맹점사업자번호, e.합계);
            if (!dict.TryGetValue(key, out var q))
            {
                q = new Queue<CardTransactionData>();
                dict[key] = q;
            }
            q.Enqueue(e);
        }
        return dict;
    }

    /// <summary>
    /// 같은 키인데 공제/불공제가 섞여 있는 키 집합('확인요망' 대상).
    /// </summary>
    public HashSet<string> FindMixedKeys(List<CardTransactionData> excelData)
    {
        var byKey = new Dictionary<string, HashSet<string>>();
        foreach (var e in excelData)
        {
            var key = MakeKey(e.승인일자, e.가맹점사업자번호, e.합계);
            if (!byKey.TryGetValue(key, out var set))
            {
                set = new HashSet<string>();
                byKey[key] = set;
            }
            set.Add(e.공제여부결정);
        }
        return byKey.Where(kv => kv.Value.Count > 1).Select(kv => kv.Key).ToHashSet();
    }

    /// <summary>
    /// 한 페이지(웹 행들)를 웹 순서대로 훑으며, 각 웹 행에 키가 같은 엑셀 행을 큐에서
    /// 순서대로 꺼내 배정한다. 엑셀에 대응이 없는 웹 행(여분)은 건너뛴다.
    /// excelQueue는 페이지 간 상태가 유지되어야 하므로 호출부에서 한 번만 만들어 계속 넘긴다.
    /// </summary>
    public PageMatchResult MatchPageOrdered(
        Dictionary<string, Queue<CardTransactionData>> excelQueue,
        HashSet<string> mixedKeys,
        List<WebTableRow> pageWebData)
    {
        var result = new PageMatchResult();

        foreach (var web in pageWebData)
        {
            var key = MakeKey(web.AprvDt, web.MrntTxprDscmNoEncCntn, web.TotaTrsAmt);
            if (!excelQueue.TryGetValue(key, out var queue) || queue.Count == 0)
            {
                continue; // 엑셀에 없는 웹 여분 행 → 건드리지 않음
            }

            var excel = queue.Dequeue(); // 같은 키 다건이면 엑셀 순서대로 소비
            result.MatchedCount++;

            // 간이과세자는 공제여부 변경 불가
            bool isSimple = !string.IsNullOrEmpty(web.MrntTyp) && web.MrntTyp.Contains("간이");

            if (web.CurrentDdcYnNm != excel.공제여부결정)
            {
                if (isSimple)
                {
                    result.SkippedSimpleTaxpayer++;
                    continue;
                }

                result.Changes.Add(new MatchedChange
                {
                    RowIndex = web.RowIndex,
                    공제여부 = excel.공제여부결정,
                    ExcelData = excel,
                    WebData = web,
                    NeedsManualReview = mixedKeys.Contains(key)
                });
            }
        }

        return result;
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
