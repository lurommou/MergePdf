using PDFtoImage;

namespace MergePdf.Services;

/// <summary>
/// PDF 預覽服務，使用 PDFtoImage 渲染 PDF 頁面為圖片。
/// </summary>
public class PdfPreviewService
{
    /// <summary>
    /// 取得指定 PDF 檔案的總頁數。
    /// </summary>
    public int GetPageCount(string filePath)
    {
        var pdfBytes = File.ReadAllBytes(filePath);
        return Conversion.GetPageCount(pdfBytes);
    }

    /// <summary>
    /// 非同步渲染指定 PDF 頁面為 Image。
    /// </summary>
    /// <param name="filePath">PDF 檔案路徑。</param>
    /// <param name="pageIndex">頁碼索引（0-based）。</param>
    /// <param name="width">渲染寬度（像素），高度依比例計算。</param>
    public async Task<Image> RenderPageAsync(string filePath, int pageIndex)
    {
        return await Task.Run(() =>
        {
            var pdfBytes = File.ReadAllBytes(filePath);
            using var outputStream = new MemoryStream();
            var options = new RenderOptions
            {
                Dpi = 300,
                AntiAliasing = PdfAntiAliasing.All
            };
            Conversion.SavePng(outputStream, pdfBytes, pageIndex, options: options);
            outputStream.Seek(0, SeekOrigin.Begin);

            // 複製到新 MemoryStream 以便 Image 持有獨立的 stream
            var imageStream = new MemoryStream(outputStream.ToArray());
            return Image.FromStream(imageStream);
        });
    }
}
