namespace MergePdf.Utils;

/// <summary>
/// 檔案驗證輔助工具，集中管理驗證邏輯與錯誤訊息。
/// </summary>
public static class FileValidationHelper
{
    // 集中管理的錯誤訊息常數
    private const string ErrorNotPdf = "檔案不是 PDF 格式：{0}";
    private const string ErrorFileNotFound = "檔案不存在：{0}";
    private const string ErrorNoFiles = "請先加入至少一個 PDF 檔案。";
    private const string ErrorOutputNotSet = "請先設定輸出檔案位置。";

    /// <summary>
    /// 取得「非 PDF」的錯誤訊息。
    /// </summary>
    public static string GetNotPdfError(string path) => string.Format(ErrorNotPdf, path);

    /// <summary>
    /// 取得「檔案不存在」的錯誤訊息。
    /// </summary>
    public static string GetFileNotFoundError(string path) => string.Format(ErrorFileNotFound, path);

    /// <summary>
    /// 取得「無檔案」的錯誤訊息。
    /// </summary>
    public static string GetNoFilesError() => ErrorNoFiles;

    /// <summary>
    /// 取得「未設定輸出路徑」的錯誤訊息。
    /// </summary>
    public static string GetOutputNotSetError() => ErrorOutputNotSet;

    /// <summary>
    /// 判斷指定路徑是否為 PDF 檔案（依副檔名）。
    /// </summary>
    public static bool IsPdfFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判斷檔案是否存在。
    /// </summary>
    public static bool FileExists(string path) => File.Exists(path);

    /// <summary>
    /// 批次驗證多個檔案路徑，回傳有效與無效的分類結果。
    /// </summary>
    public static (List<string> ValidFiles, List<string> InvalidMessages) ValidateFiles(IEnumerable<string> paths)
    {
        var valid = new List<string>();
        var invalid = new List<string>();

        foreach (var path in paths)
        {
            if (!IsPdfFile(path))
            {
                invalid.Add(GetNotPdfError(path));
            }
            else if (!FileExists(path))
            {
                invalid.Add(GetFileNotFoundError(path));
            }
            else
            {
                valid.Add(path);
            }
        }

        return (valid, invalid);
    }
}
