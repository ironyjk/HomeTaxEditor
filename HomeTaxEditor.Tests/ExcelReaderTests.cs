using HomeTaxEditor.Core.Services;
using HomeTaxEditor.Core.Models;
using Xunit;

namespace HomeTaxEditor.Tests;

public class ExcelReaderTests
{
    private readonly ExcelReader _reader;

    public ExcelReaderTests()
    {
        _reader = new ExcelReader();
    }

    [Fact]
    public void ReadExcelFile_WithUnsupportedExtension_ThrowsNotSupportedException()
    {
        // Arrange
        var filePath = "test.txt";

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _reader.ReadExcelFile(filePath));
    }

    [Fact]
    public void ReadExcelFile_WithMissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = "nonexistent.xlsx";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _reader.ReadExcelFile(filePath));
    }

    // 실제 파일 테스트는 TestData 폴더에 샘플 파일이 있을 때 작성
    // [Fact]
    // public void ReadXlsxFile_WithValidFile_ReturnsData()
    // {
    //     // TODO: 샘플 xlsx 파일 준비 후 테스트
    // }
}
