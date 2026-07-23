using HomeTaxEditor.Core.Services;
using HomeTaxEditor.Core.Models;
using Xunit;

namespace HomeTaxEditor.Tests;

public class DataMatcherTests
{
    private readonly DataMatcher _matcher;

    public DataMatcherTests()
    {
        _matcher = new DataMatcher();
    }

    [Fact]
    public void MatchData_WithExactMatch_ReturnsMatchedChange()
    {
        // Arrange
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234-5678-9012-3456",
                가맹점사업자번호 = "123-45-67890",
                합계 = 10000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234-5678-9012-3456",
                MrntTxprDscmNoEncCntn = "123-45-67890",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"  // 다름!
            }
        };

        // Act
        var result = _matcher.MatchData(excelData, webData);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].RowIndex);
        Assert.Equal("공제", result[0].공제여부);
    }

    [Fact]
    public void MatchData_WithSameValue_ReturnsEmpty()
    {
        // Arrange
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234-5678-9012-3456",
                가맹점사업자번호 = "123-45-67890",
                합계 = 10000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234-5678-9012-3456",
                MrntTxprDscmNoEncCntn = "123-45-67890",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "공제"  // 같음!
            }
        };

        // Act
        var result = _matcher.MatchData(excelData, webData);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MatchData_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234-5678-9012-3456",
                가맹점사업자번호 = "123-45-67890",
                합계 = 10000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-11-01",  // 날짜 다름
                BusnCrdCardNoEncCntn = "1234-5678-9012-3456",
                MrntTxprDscmNoEncCntn = "123-45-67890",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"
            }
        };

        // Act
        var result = _matcher.MatchData(excelData, webData);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetMatchingStats_ReturnsCorrectStats()
    {
        // Arrange
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234",
                가맹점사업자번호 = "123",
                합계 = 10000,
                공제여부결정 = "공제"
            },
            new CardTransactionData
            {
                승인일자 = "2025-11-01",
                카드번호 = "5678",
                가맹점사업자번호 = "456",
                합계 = 20000,
                공제여부결정 = "불공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234",
                MrntTxprDscmNoEncCntn = "123",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"
            }
        };

        var changes = _matcher.MatchData(excelData, webData);

        // Act
        var (total, matched, needChange) = _matcher.GetMatchingStats(excelData, webData, changes);

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(1, matched);
        Assert.Equal(1, needChange);
    }

    [Fact]
    public void MatchData_WithSimpleTaxpayer_SkipsChange()
    {
        // Arrange - 간이과세자는 변경하지 않음
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234-5678-9012-3456",
                가맹점사업자번호 = "123-45-67890",
                합계 = 10000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234-5678-9012-3456",
                MrntTxprDscmNoEncCntn = "123-45-67890",
                MrntTyp = "간이과세자",  // 간이과세자!
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"  // 다르지만 간이과세자라서 변경 안 함
            }
        };

        // Act
        var result = _matcher.MatchData(excelData, webData);

        // Assert - 간이과세자는 제외되어야 함
        Assert.Empty(result);
    }

    [Fact]
    public void MatchData_WithNonSimpleTaxpayer_AppliesChange()
    {
        // Arrange - 일반과세자는 정상 변경
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234-5678-9012-3456",
                가맹점사업자번호 = "123-45-67890",
                합계 = 10000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234-5678-9012-3456",
                MrntTxprDscmNoEncCntn = "123-45-67890",
                MrntTyp = "일반과세자",  // 일반과세자
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"
            }
        };

        // Act
        var result = _matcher.MatchData(excelData, webData);

        // Assert - 일반과세자는 변경되어야 함
        Assert.Single(result);
        Assert.Equal("공제", result[0].공제여부);
    }

    [Fact]
    public void GetSkippedSimpleTaxpayerCount_WithSimpleTaxpayer_ReturnsCorrectCount()
    {
        // Arrange
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234",
                가맹점사업자번호 = "123",
                합계 = 10000,
                공제여부결정 = "공제"
            },
            new CardTransactionData
            {
                승인일자 = "2025-11-01",
                카드번호 = "5678",
                가맹점사업자번호 = "456",
                합계 = 20000,
                공제여부결정 = "공제"
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234",
                MrntTxprDscmNoEncCntn = "123",
                MrntTyp = "간이과세자",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"  // 다름
            },
            new WebTableRow
            {
                RowIndex = 1,
                AprvDt = "2025-11-01",
                BusnCrdCardNoEncCntn = "5678",
                MrntTxprDscmNoEncCntn = "456",
                MrntTyp = "일반과세자",
                TotaTrsAmt = 20000,
                CurrentDdcYnNm = "불공제"  // 다름
            }
        };

        // Act
        var skippedCount = _matcher.GetSkippedSimpleTaxpayerCount(excelData, webData);

        // Assert - 간이과세자 1건만 제외
        Assert.Equal(1, skippedCount);
    }

    [Fact]
    public void MatchPageOrdered_MixedGroup_AssignsInExcelOrder_AndFlags()
    {
        // Arrange - 같은 날짜·거래처·금액인데 공제/불공제가 갈리는 후불하이패스형 그룹
        //           엑셀 순서: [공제, 불공제] → 웹 순서: [0행, 1행]에 그대로 배정되어야 함
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData { 승인일자 = "2026-06-30", 카드번호 = "AAAA", 가맹점사업자번호 = "214-81-37726", 합계 = 2800, 공제여부결정 = "공제" },
            new CardTransactionData { 승인일자 = "2026-06-30", 카드번호 = "BBBB", 가맹점사업자번호 = "214-81-37726", 합계 = 2800, 공제여부결정 = "불공제" },
        };
        var webData = new List<WebTableRow>
        {
            new WebTableRow { RowIndex = 0, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "214-81-37726", TotaTrsAmt = 2800, CurrentDdcYnNm = "불공제" },
            new WebTableRow { RowIndex = 1, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "214-81-37726", TotaTrsAmt = 2800, CurrentDdcYnNm = "공제" },
        };

        // Act
        var queue = _matcher.BuildExcelQueue(excelData);
        var mixed = _matcher.FindMixedKeys(excelData);
        var result = _matcher.MatchPageOrdered(queue, mixed, webData);

        // Assert
        Assert.Equal(2, result.MatchedCount);
        Assert.Equal(2, result.Changes.Count);
        var c0 = Assert.Single(result.Changes, c => c.RowIndex == 0);
        var c1 = Assert.Single(result.Changes, c => c.RowIndex == 1);
        Assert.Equal("공제", c0.공제여부);      // 웹 0행 ← 엑셀 첫 행(공제)
        Assert.Equal("불공제", c1.공제여부);    // 웹 1행 ← 엑셀 둘째 행(불공제)
        Assert.True(c0.NeedsManualReview);      // 섞인 그룹이므로 확인요망
        Assert.True(c1.NeedsManualReview);
    }

    [Fact]
    public void MatchPageOrdered_UniqueKey_NoManualFlag()
    {
        // Arrange - 겹치지 않는 단일 건은 기존과 동일하게 변경, 확인요망 아님
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData { 승인일자 = "2026-06-30", 가맹점사업자번호 = "111-11-11111", 합계 = 1000, 공제여부결정 = "공제" },
        };
        var webData = new List<WebTableRow>
        {
            new WebTableRow { RowIndex = 5, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "111-11-11111", TotaTrsAmt = 1000, CurrentDdcYnNm = "불공제" },
        };

        // Act
        var result = _matcher.MatchPageOrdered(
            _matcher.BuildExcelQueue(excelData), _matcher.FindMixedKeys(excelData), webData);

        // Assert
        Assert.Single(result.Changes);
        Assert.Equal("공제", result.Changes[0].공제여부);
        Assert.False(result.Changes[0].NeedsManualReview);
    }

    [Fact]
    public void MatchPageOrdered_SameDecisionDuplicates_NotFlagged_AndExtraWebSkipped()
    {
        // Arrange - 같은 키 2건이지만 둘 다 '불공제'(안 섞임) + 웹이 엑셀보다 1건 많음
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData { 승인일자 = "2026-06-30", 가맹점사업자번호 = "222-22-22222", 합계 = 500, 공제여부결정 = "불공제" },
            new CardTransactionData { 승인일자 = "2026-06-30", 가맹점사업자번호 = "222-22-22222", 합계 = 500, 공제여부결정 = "불공제" },
        };
        var webData = new List<WebTableRow>
        {
            new WebTableRow { RowIndex = 0, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "222-22-22222", TotaTrsAmt = 500, CurrentDdcYnNm = "공제" },
            new WebTableRow { RowIndex = 1, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "222-22-22222", TotaTrsAmt = 500, CurrentDdcYnNm = "공제" },
            new WebTableRow { RowIndex = 2, AprvDt = "2026-06-30", MrntTxprDscmNoEncCntn = "222-22-22222", TotaTrsAmt = 500, CurrentDdcYnNm = "공제" }, // 엑셀보다 1건 많음
        };

        // Act
        var result = _matcher.MatchPageOrdered(
            _matcher.BuildExcelQueue(excelData), _matcher.FindMixedKeys(excelData), webData);

        // Assert
        Assert.Equal(2, result.MatchedCount);   // 웹 3건 중 엑셀과 대응된 2건만
        Assert.Equal(2, result.Changes.Count);  // 둘 다 공제→불공제
        Assert.All(result.Changes, c => Assert.False(c.NeedsManualReview)); // 같은 결정이라 확인요망 아님
    }

    [Fact]
    public void GetSkippedSimpleTaxpayerCount_WithSimpleTaxpayerButSameValue_ReturnsZero()
    {
        // Arrange - 간이과세자지만 공제여부가 같으면 카운트 안 함
        var excelData = new List<CardTransactionData>
        {
            new CardTransactionData
            {
                승인일자 = "2025-10-31",
                카드번호 = "1234",
                가맹점사업자번호 = "123",
                합계 = 10000,
                공제여부결정 = "불공제"  // 웹과 같음
            }
        };

        var webData = new List<WebTableRow>
        {
            new WebTableRow
            {
                RowIndex = 0,
                AprvDt = "2025-10-31",
                BusnCrdCardNoEncCntn = "1234",
                MrntTxprDscmNoEncCntn = "123",
                MrntTyp = "간이과세자",
                TotaTrsAmt = 10000,
                CurrentDdcYnNm = "불공제"  // 같음
            }
        };

        // Act
        var skippedCount = _matcher.GetSkippedSimpleTaxpayerCount(excelData, webData);

        // Assert - 공제여부가 같으면 제외 카운트 안 함
        Assert.Equal(0, skippedCount);
    }
}
