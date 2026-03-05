namespace MergePdf.Models;

/// <summary>
/// 代表一個 PDF 頁面項目，記錄來源檔案與頁碼。
/// </summary>
public class PdfPageItem
{
    /// <summary>
    /// 來源 PDF 檔案完整路徑。
    /// </summary>
    public string SourceFilePath { get; }

    /// <summary>
    /// 來源 PDF 檔案名稱。
    /// </summary>
    public string SourceFileName { get; }

    /// <summary>
    /// 此頁在來源 PDF 中的頁碼索引（0-based）。
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// 該頁面的所有標註集合。
    /// </summary>
    public List<BaseAnnotation> Annotations { get; } = new();

    public PdfPageItem(string sourceFilePath, int pageIndex)
    {
        SourceFilePath = sourceFilePath ?? throw new ArgumentNullException(nameof(sourceFilePath));
        SourceFileName = Path.GetFileName(sourceFilePath);
        PageIndex = pageIndex;
    }

    /// <summary>
    /// 供 ListBox 顯示用。
    /// </summary>
    public override string ToString() => $"{SourceFileName} - 第 {PageIndex + 1} 頁";
}
