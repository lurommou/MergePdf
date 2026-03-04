using System.Diagnostics;
using MergePdf.Models;
using MergePdf.Services;
using MergePdf.Utils;

namespace MergePdf;

/// <summary>
/// 主視窗，負責 UI 互動與呼叫服務層執行合併。
/// </summary>
public partial class MainForm : Form
{
    private const string AppTitle = "PDF 合併工具";
    private const string MergeSuccessMessage = "合併完成！已產生 {0}";
    private const string MergeErrorMessage = "合併過程發生錯誤：\n{0}";
    private const string InvalidFilesMessage = "以下檔案無法加入：\n{0}";
    private const string ProcessingMessage = "合併中，請稍候…";
    private const string DefaultOutputPrefix = "Merged_";

    private readonly PdfMergeService _mergeService = new();
    private readonly BindingSource _bindingSource = new();
    private readonly List<PdfFileItem> _fileItems = new();

    public MainForm()
    {
        InitializeComponent();
        InitializeEventHandlers();
        _bindingSource.DataSource = _fileItems;
        listBoxFiles.DataSource = _bindingSource;
    }

    /// <summary>
    /// 註冊所有事件處理器。
    /// </summary>
    private void InitializeEventHandlers()
    {
        // 拖曳事件
        panelDragDrop.DragEnter += PanelDragDrop_DragEnter;
        panelDragDrop.DragDrop += PanelDragDrop_DragDrop;
        listBoxFiles.DragEnter += PanelDragDrop_DragEnter;
        listBoxFiles.DragDrop += PanelDragDrop_DragDrop;
        listBoxFiles.AllowDrop = true;

        // 按鈕事件
        btnMoveUp.Click += BtnMoveUp_Click;
        btnMoveDown.Click += BtnMoveDown_Click;
        btnRemoveSelected.Click += BtnRemoveSelected_Click;
        btnClearAll.Click += BtnClearAll_Click;
        btnMerge.Click += BtnMerge_Click;
    }

    // ─── 拖曳處理 ───

    private void PanelDragDrop_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void PanelDragDrop_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files) return;

        var (validFiles, invalidMessages) = FileValidationHelper.ValidateFiles(files);

        foreach (var path in validFiles)
        {
            // 避免重複加入相同檔案
            if (_fileItems.Exists(item => item.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                continue;
            _fileItems.Add(new PdfFileItem(path));
        }

        RefreshList();

        if (invalidMessages.Count > 0)
        {
            MessageBox.Show(
                string.Format(InvalidFilesMessage, string.Join("\n", invalidMessages)),
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ─── 列表操作 ───

    private void BtnMoveUp_Click(object? sender, EventArgs e)
    {
        var index = listBoxFiles.SelectedIndex;
        if (index <= 0) return;

        (_fileItems[index], _fileItems[index - 1]) = (_fileItems[index - 1], _fileItems[index]);
        RefreshList();
        listBoxFiles.ClearSelected();
        listBoxFiles.SelectedIndex = index - 1;
    }

    private void BtnMoveDown_Click(object? sender, EventArgs e)
    {
        var index = listBoxFiles.SelectedIndex;
        if (index < 0 || index >= _fileItems.Count - 1) return;

        (_fileItems[index], _fileItems[index + 1]) = (_fileItems[index + 1], _fileItems[index]);
        RefreshList();
        listBoxFiles.ClearSelected();
        listBoxFiles.SelectedIndex = index + 1;
    }

    private void BtnRemoveSelected_Click(object? sender, EventArgs e)
    {
        // 從後往前移除以避免索引偏移
        var indices = listBoxFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        foreach (var index in indices)
        {
            _fileItems.RemoveAt(index);
        }
        RefreshList();
    }

    private void BtnClearAll_Click(object? sender, EventArgs e)
    {
        _fileItems.Clear();
        RefreshList();
    }

    // ─── 合併操作 ───

    private async void BtnMerge_Click(object? sender, EventArgs e)
    {
        if (_fileItems.Count == 0)
        {
            MessageBox.Show(FileValidationHelper.GetNoFilesError(), AppTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var saveDialog = new SaveFileDialog
        {
            Filter = "PDF 檔案 (*.pdf)|*.pdf",
            DefaultExt = "pdf",
            FileName = $"{DefaultOutputPrefix}{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        };

        if (saveDialog.ShowDialog() != DialogResult.OK) return;

        var outputPath = saveDialog.FileName;
        SetUiBusy(true);

        try
        {
            var sourcePaths = _fileItems.Select(item => item.FilePath).ToList();
            await _mergeService.MergeAsync(sourcePaths, outputPath);

            MessageBox.Show(
                string.Format(MergeSuccessMessage, outputPath),
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 自動開啟合併後的 PDF
            Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format(MergeErrorMessage, ex.Message),
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetUiBusy(false);
        }
    }

    // ─── 輔助方法 ───

    /// <summary>
    /// 重新整理列表並更新狀態列與按鈕狀態。
    /// </summary>
    private void RefreshList()
    {
        _bindingSource.ResetBindings(false);
        UpdateStatusBar();
        btnMerge.Enabled = _fileItems.Count > 0;
    }

    /// <summary>
    /// 設定 UI 忙碌/閒置狀態。
    /// </summary>
    private void SetUiBusy(bool busy)
    {
        btnMerge.Enabled = !busy;
        btnMoveUp.Enabled = !busy;
        btnMoveDown.Enabled = !busy;
        btnRemoveSelected.Enabled = !busy;
        btnClearAll.Enabled = !busy;
        panelDragDrop.AllowDrop = !busy;
        listBoxFiles.AllowDrop = !busy;
        toolStripStatusLabel.Text = busy ? ProcessingMessage : $"共 {_fileItems.Count} 個檔案。";
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    /// <summary>
    /// 更新狀態列文字。
    /// </summary>
    private void UpdateStatusBar()
    {
        toolStripStatusLabel.Text = _fileItems.Count == 0
            ? "就緒，尚無檔案。"
            : $"共 {_fileItems.Count} 個檔案。";
    }
}
