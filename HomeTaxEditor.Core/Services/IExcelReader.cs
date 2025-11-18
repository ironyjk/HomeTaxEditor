using HomeTaxEditor.Core.Models;

namespace HomeTaxEditor.Core.Services;

public interface IExcelReader
{
    List<CardTransactionData> ReadExcelFile(string filePath);
}
