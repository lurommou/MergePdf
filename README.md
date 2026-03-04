# PDF 合併工具 (MergePdf)

Windows 桌面 PDF 合併工具，使用 .NET 8 + WinForms + PDFsharp 建置。

## 功能

- 拖曳多個 PDF 檔案至視窗
- 檔案排序（上移 / 下移）
- 多選移除 / 一鍵清空列表
- 自訂輸出檔名與位置
- 合併後自動開啟 PDF
- 簡易 Log 記錄（於 `Logs/` 目錄）

## 建置與執行

```bash
# 還原套件
dotnet restore

# 偵錯執行
dotnet run

# 發佈為單一 EXE（Self-contained，不需安裝 .NET Runtime）
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

發佈後的 EXE 位於：
```
bin\Release\net8.0-windows\win-x64\publish\MergePdf.exe
```

直接複製此 EXE 至目標機器即可使用，**不需安裝 .NET Runtime**。

## App Icon 設定

1. 準備 `.ico` 格式的圖示檔（如 `app.ico`），放置於專案根目錄。
2. 在 `MergePdf.csproj` 的 `<PropertyGroup>` 內加入：
   ```xml
   <ApplicationIcon>app.ico</ApplicationIcon>
   ```
3. 重新發佈即可套用。

## 授權聲明

- 本專案僅使用 **MIT License** 授權的套件（PDFsharp）
- **可免費商用**
- 無 AGPL 或其他 Copyleft 授權依賴
