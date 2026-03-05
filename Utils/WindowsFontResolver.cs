using PdfSharp.Fonts;

namespace MergePdf.Utils;

/// <summary>
/// 提供 Windows 系統的字型解析，解決 PDFsharp Core 無法自動載入系統字型的問題。
/// 注意：PDFsharp 對 .ttc (TrueType Collection) 的支援不穩定，此處只使用標準 .ttf 檔案。
/// </summary>
public class WindowsFontResolver : IFontResolver
{
    private static readonly string WindowsFontsDir =
        Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

    // 將 faceName（由 ResolveTypeface 傳入的唯一 key）對應至 Windows 字型檔
    private static readonly Dictionary<string, string> FaceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["arial"] = "arial.ttf",
        ["arial#b"] = "arialbd.ttf",
        ["arial#i"] = "ariali.ttf",
        ["arial#bi"] = "arialbi.ttf",
        ["times"] = "times.ttf",
        ["times#b"] = "timesbd.ttf",
        ["times#i"] = "timesi.ttf",
        ["courier"] = "cour.ttf",
        ["courier#b"] = "courbd.ttf",
        // CJK fallback：使用「標楷體」(kaiu.ttf)，這是 Windows 上確定存在的 .ttf 格式 CJK 字型
        ["kaiu"] = "kaiu.ttf",
        ["default"] = "arial.ttf",
    };

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string lower = familyName.ToLowerInvariant();
        
        // 判斷是否為已知的 ASCII 字型
        string baseKey = lower switch
        {
            var n when n.Contains("arial") => "arial",
            var n when n.Contains("times") => "times",
            var n when n.Contains("courier") => "courier",
            // 微軟正黑體、新細明體、標楷體等 CJK 字型均 fallback 到 kaiu
            _ => "kaiu"
        };

        string suffix = (isBold && isItalic) ? "#bi" : isBold ? "#b" : isItalic ? "#i" : "";
        string faceKey = FaceMap.ContainsKey(baseKey + suffix) ? baseKey + suffix : baseKey;

        return new FontResolverInfo(faceKey);
    }

    public byte[]? GetFont(string faceName)
    {
        if (!FaceMap.TryGetValue(faceName, out var fileName))
            fileName = "arial.ttf"; // 最終預設

        var path = Path.Combine(WindowsFontsDir, fileName);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }
}
