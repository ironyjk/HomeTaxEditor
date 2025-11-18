using HomeTaxEditor.Core.Services;
using HomeTaxEditor.Core.Models;
using Xunit;

namespace HomeTaxEditor.Tests;

public class ScriptGeneratorTests
{
    private readonly ScriptGenerator _generator;

    public ScriptGeneratorTests()
    {
        _generator = new ScriptGenerator();
    }

    [Fact]
    public void GenerateTableExtractionScript_ReturnsValidJavaScript()
    {
        // Act
        var script = _generator.GenerateTableExtractionScript();

        // Assert
        Assert.NotEmpty(script);
        Assert.Contains("grid_body_row", script);
        Assert.Contains("aprvDt", script);
        Assert.Contains("JSON.stringify", script);
    }

    [Fact]
    public void GenerateApplyChangesScript_WithChanges_ReturnsValidJavaScript()
    {
        // Arrange
        var changes = new List<MatchedChange>
        {
            new MatchedChange
            {
                RowIndex = 0,
                공제여부 = "공제",
                ExcelData = new CardTransactionData(),
                WebData = new WebTableRow()
            },
            new MatchedChange
            {
                RowIndex = 1,
                공제여부 = "불공제",
                ExcelData = new CardTransactionData(),
                WebData = new WebTableRow()
            }
        };

        // Act
        var script = _generator.GenerateApplyChangesScript(changes);

        // Assert
        Assert.NotEmpty(script);
        Assert.Contains("checkbox", script);
        Assert.Contains("selectBox", script);
        Assert.Contains("\"rowIndex\":0", script);
        Assert.Contains("\"rowIndex\":1", script);
        // JSON 직렬화가 한글을 이스케이프하므로 rowIndex로만 검증
    }

    [Fact]
    public void GenerateApplyChangesScript_WithEmptyChanges_ReturnsValidScript()
    {
        // Arrange
        var changes = new List<MatchedChange>();

        // Act
        var script = _generator.GenerateApplyChangesScript(changes);

        // Assert
        Assert.NotEmpty(script);
        Assert.Contains("changes = []", script);
    }
}
