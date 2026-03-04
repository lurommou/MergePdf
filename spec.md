
## 🎯 任務目標

請建立一個 **Windows 桌面 PDF 合併工具**，使用：

- ✅ C#
- ✅ .NET 8 LTS
- ✅ WinForms
- ✅ PDFsharp (MIT License，免費可商用)
- ✅ 可發佈為單一 EXE（Self-contained）
- ✅ 所有使用的技術與套件必須允許免費商用

這是一個可實際部署使用的正式工具，不是教學範例。

---

# 🏗 技術與架構要求

## 1️⃣ 專案設定

- Target Framework: `net8.0-windows`
- Output Type: Windows Application
- 啟用：
  - `<UseWindowsForms>true</UseWindowsForms>`
  - `<PublishSingleFile>true</PublishSingleFile>`
  - `<SelfContained>true</SelfContained>`
  - `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`

- NuGet 套件：
  - PDFsharp (MIT License)

請提供完整 `.csproj` 檔案內容。

---

## 2️⃣ 專案結構

請建立清晰結構：

```
PdfMergeTool/
 ├── Program.cs
 ├── MainForm.cs
 ├── MainForm.Designer.cs
 ├── Services/
 │     └── PdfMergeService.cs
 ├── Models/
 │     └── PdfFileItem.cs
 └── Utils/
       └── FileValidationHelper.cs
```

必須使用分層設計，不可把所有邏輯寫在 Form 內。

---

# 🖥 UI 功能需求

## 主畫面功能：

### ✅ 1. 拖曳區域
- 支援拖曳多個 PDF 檔案
- 僅允許 `.pdf`
- 非 PDF 顯示錯誤提示

### ✅ 2. 檔案列表顯示
- 顯示：
  - 檔名
  - 完整路徑
- 可多選刪除
- 可清空列表

### ✅ 3. 檔案順序控制
- 上移
- 下移
- 影響合併順序

### ✅ 4. 選擇輸出檔案位置
- 使用 SaveFileDialog
- 預設檔名：Merged_yyyyMMdd_HHmmss.pdf

### ✅ 5. 合併按鈕
- 無檔案時不可按
- 合併時禁用按鈕
- 顯示處理中狀態

### ✅ 6. 完成後
- 自動開啟合併後的 PDF
- 顯示成功提示

---

# 📄 PDF 合併規格

使用 PDFsharp：

- 使用 `PdfDocument`
- 逐一打開來源檔案
- 匯入所有頁面
- 保持原始順序
- 使用 `using` 釋放資源
- 錯誤處理：
  - 檔案被占用
  - 損毀 PDF
  - 無存取權限

---

# 🛡 錯誤處理與 UX 規範

- 所有例外不可直接崩潰
- 使用 MessageBox 顯示錯誤
- UI 不可凍結（必要時使用 async/await）
- 禁止未處理例外

---

# 🚀 發佈需求

請提供發佈指令：

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

並說明：

- 輸出位置
- 如何部署
- 是否需要安裝 .NET Runtime（答案應為不需要）

---

# 🎨 程式碼品質要求

- 使用 PascalCase 命名
- 使用 XML 註解
- 每個類別加上說明
- 不可將商業邏輯寫在 UI 層
- 方法需保持單一職責
- 不允許硬編碼字串（集中管理）

---

# 🔐 授權與商用聲明

請在 README 區塊說明：

- 本專案僅使用 MIT License 套件
- 可免費商用
- 無 AGPL 依賴

---

# 📦 最終輸出要求

AI 必須輸出：

1. ✅ 完整專案所有程式碼檔案
2. ✅ .csproj 檔
3. ✅ 發佈說明
4. ✅ 執行說明
5. ✅ 商用授權說明
6. ✅ 專案結構說明

不可只給片段程式碼。

---

# 🎯 額外加分（如果可行）

- 加入簡易 Log 機制
- 加入狀態列顯示目前檔案數
- 加入簡單 App Icon 設定說明

---

# ✅ 成功標準

- 可直接編譯
- 可成功合併 PDF
- 可單一 EXE 執行
- 無商用授權風險

---

