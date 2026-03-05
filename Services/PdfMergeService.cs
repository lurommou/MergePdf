using MergePdf.Models;
using PdfSharpCore = PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System.Drawing;

namespace MergePdf.Services;

/// <summary>
/// PDF 合併服務，負責使用 PDFsharp 將多個 PDF 頁面合併為一。
/// </summary>
public class PdfMergeService
{
    private static readonly string LogDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Logs");

    /// <summary>
    /// 非同步合併指定頁面清單至輸出路徑。
    /// </summary>
    /// <param name="pages">欲合併的頁面清單（依順序）。</param>
    /// <param name="outputPath">合併後輸出檔案路徑。</param>
    public async Task MergeAsync(IReadOnlyList<PdfPageItem> pages, string outputPath)
    {
        if (pages == null || pages.Count == 0)
            throw new ArgumentException("至少需要一個頁面。", nameof(pages));

        await Task.Run(() =>
        {
            using var outputDocument = new PdfSharpCore.PdfDocument();

            // 快取已開啟的來源文件，同一檔案只開啟一次
            var openedDocuments = new Dictionary<string, PdfSharpCore.PdfDocument>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var page in pages)
                {
                    if (!openedDocuments.TryGetValue(page.SourceFilePath, out var inputDocument))
                    {
                        Log($"正在開啟：{page.SourceFilePath}");
                        inputDocument = PdfReader.Open(page.SourceFilePath, PdfDocumentOpenMode.Import);
                        openedDocuments[page.SourceFilePath] = inputDocument;
                    }

                    var newPage = outputDocument.AddPage(inputDocument.Pages[page.PageIndex]);

                    if (page.Annotations.Count > 0)
                    {
                        using var gfx = XGraphics.FromPdfPage(newPage);
                        // PDFsharp 的預設單位為 Point (1/72 inch). 
                        // 我們在預覽畫布中使用的座標，是基於 PDFtoImage 的渲染設定 (Dpi=300).
                        // Dpi=300 表示 1 inch = 300 pixels. 
                        // 因此：Pixel 到 Point 的轉換率為 72 / 300 = 0.24.
                        float scale = 72f / 300f;

                        foreach (var ann in page.Annotations)
                        {
                            DrawAnnotationToPdf(gfx, ann, scale);
                        }
                    }
                }

                var totalPages = outputDocument.PageCount;
                outputDocument.Save(outputPath);
                Log($"合併完成，輸出至：{outputPath}，共 {totalPages} 頁。");
            }
            finally
            {
                foreach (var doc in openedDocuments.Values)
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

    private void DrawAnnotationToPdf(XGraphics gfx, BaseAnnotation ann, float scale)
    {
        var scaledBounds = new XRect(
            ann.Bounds.X * scale,
            ann.Bounds.Y * scale,
            ann.Bounds.Width * scale,
            ann.Bounds.Height * scale);

        var color = XColor.FromArgb(ann.Color.A, ann.Color.R, ann.Color.G, ann.Color.B);
        var pen = new XPen(color, ann.Thickness * scale);
        var brush = new XSolidBrush(color);

        if (ann is TextAnnotation textAnn)
        {
            // PDFsharp 字型大小單位也是 Point，直接使用預覽設定的 Size * 縮放率
            var font = new XFont(textAnn.FontName, textAnn.FontSize * scale, XFontStyleEx.Regular);
            // XGraphics.DrawString 預設以基線 (Baseline) 對齊，而 WinForms 以左上角對齊
            // 因此需要手動指定 StringFormat 為左上角對齊以保持一致
            var format = new XStringFormat { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.Near };
            gfx.DrawString(textAnn.Text, font, brush, scaledBounds, format);
        }
        else if (ann is RectAnnotation)
        {
            gfx.DrawRectangle(pen, scaledBounds);
        }
        else if (ann is EllipseAnnotation)
        {
            gfx.DrawEllipse(pen, scaledBounds);
        }
        else if (ann is ArrowAnnotation arrowAnn)
        {
            float sX = arrowAnn.StartPoint.X * scale;
            float sY = arrowAnn.StartPoint.Y * scale;
            float eX = arrowAnn.EndPoint.X * scale;
            float eY = arrowAnn.EndPoint.Y * scale;

            gfx.DrawLine(pen, sX, sY, eX, eY);

            // 繪製箭頭頭部
            double angle = Math.Atan2(eY - sY, eX - sX);
            double arrowSize = 15.0 * scale;
            double arrowAngle = Math.PI / 6.0;

            XPoint p1 = new XPoint(
                eX - arrowSize * Math.Cos(angle - arrowAngle),
                eY - arrowSize * Math.Sin(angle - arrowAngle));
            XPoint p2 = new XPoint(
                eX - arrowSize * Math.Cos(angle + arrowAngle),
                eY - arrowSize * Math.Sin(angle + arrowAngle));

            gfx.DrawLine(pen, new XPoint(eX, eY), p1);
            gfx.DrawLine(pen, new XPoint(eX, eY), p2);
        }
        else if (ann is LineAnnotation lineAnn)
        {
            gfx.DrawLine(pen, lineAnn.StartPoint.X * scale, lineAnn.StartPoint.Y * scale, 
                              lineAnn.EndPoint.X * scale, lineAnn.EndPoint.Y * scale);
        }
    }
}
