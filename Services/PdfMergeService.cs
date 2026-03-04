using PdfSharpCore = PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace MergePdf.Services;

/// <summary>
/// PDF 合併服務，負責使用 PDFsharp 將多個 PDF 檔案合併為一。
/// </summary>
public class PdfMergeService
{
    private static readonly string LogDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Logs");

    /// <summary>
    /// 非同步合併多個 PDF 檔案至指定輸出路徑。
    /// </summary>
    /// <param name="sourcePaths">來源 PDF 檔案路徑清單（依順序合併）。</param>
    /// <param name="outputPath">合併後輸出檔案路徑。</param>
    /// <exception cref="ArgumentException">來源清單為空時擲出。</exception>
    /// <exception cref="IOException">檔案被占用或無存取權限時擲出。</exception>
    /// <exception cref="PdfSharp.Pdf.IO.PdfReaderException">PDF 損毀時擲出。</exception>
    public async Task MergeAsync(IReadOnlyList<string> sourcePaths, string outputPath)
    {
        if (sourcePaths == null || sourcePaths.Count == 0)
            throw new ArgumentException("至少需要一個來源 PDF 檔案。", nameof(sourcePaths));

        await Task.Run(() =>
        {
            using var outputDocument = new PdfSharpCore.PdfDocument();

            // 來源文件必須保持開啟直到輸出存檔完成，否則 PDFsharp 6.x 會報錯
            var inputDocuments = new List<PdfSharpCore.PdfDocument>();
            try
            {
                foreach (var sourcePath in sourcePaths)
                {
                    Log($"正在處理：{sourcePath}");

                    var inputDocument = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
                    inputDocuments.Add(inputDocument);
                    for (var i = 0; i < inputDocument.PageCount; i++)
                    {
                        outputDocument.AddPage(inputDocument.Pages[i]);
                    }
                }

                var totalPages = outputDocument.PageCount;
                outputDocument.Save(outputPath);
                Log($"合併完成，輸出至：{outputPath}，共 {totalPages} 頁。");
            }
            finally
            {
                foreach (var doc in inputDocuments)
                {
                    doc.Dispose();
                }
            }
        });
    }

    /// <summary>
    /// 簡易 Log 寫入。
    /// </summary>
    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var logFile = Path.Combine(LogDirectory, $"MergePdf_{DateTime.Now:yyyyMMdd}.log");
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(logFile, entry);
        }
        catch
        {
            // Log 寫入失敗不應影響主流程
        }
    }
}
