namespace MergePdf.Models;

/// <summary>
/// 代表一個 PDF 檔案項目，用於檔案列表顯示與合併操作。
/// </summary>
public class PdfFileItem
{
    /// <summary>
    /// 檔案名稱（含副檔名）。
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// 檔案完整路徑。
    /// </summary>
    public string FilePath { get; }

    public PdfFileItem(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileName = Path.GetFileName(filePath);
    }

    /// <summary>
    /// 供 ListBox 顯示用。
    /// </summary>
    public override string ToString() => $"{FileName}　({FilePath})";
}
