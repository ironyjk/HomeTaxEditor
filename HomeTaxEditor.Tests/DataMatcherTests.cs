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
}
