using System.Diagnostics;
using System.Drawing.Drawing2D;
using MergePdf.Models;
using MergePdf.Services;
using MergePdf.Utils;

namespace MergePdf;

/// <summary>
/// 主視窗，負責 UI 互動、PDF 預覽與呼叫服務層執行合併。
/// </summary>
public partial class MainForm : Form
{
    private const string AppTitle = "PDF 合併工具";
    private const string MergeSuccessMessage = "合併完成！已產生 {0}";
    private const string MergeErrorMessage = "合併過程發生錯誤：\n{0}";
    private const string InvalidFilesMessage = "以下檔案無法加入：\n{0}";
    private const string ProcessingMessage = "合併中，請稍候…";
    private const string PreviewErrorMessage = "預覽失敗：{0}";
    private const string DefaultOutputPrefix = "Merged_";

    private readonly PdfMergeService _mergeService = new();
    private readonly PdfPreviewService _previewService = new();
    private readonly BindingSource _bindingSource = new();
    private readonly List<PdfPageItem> _pageItems = new();
    private int _previewIndex = -1;

    // ─── 繪圖狀態 ───
    private enum DrawMode { Select, Text, Line, Arrow, Rect, Mosaic, Ellipse }
    private DrawMode _currentDrawMode = DrawMode.Select;
    private Color _currentDrawColor = Color.Red;
    private Font _currentDrawFont = new Font("Microsoft JhengHei UI", 24);
    
    private BaseAnnotation? _selectedAnnotation;
    private BaseAnnotation? _drawingAnnotation;
    private bool _isDragging;
    private bool _isResizing;
    private PointF _dragStartPoint;
    private RectangleF _dragStartBounds;
    private PointF _dragStartLineStart;
    private PointF _dragStartLineEnd;
    private const float ResizeHandleSize = 12f;

    // 文字就地編輯控件
    private TextBox? _inlineTextBox;
    private TextAnnotation? _editingTextAnn;

    public MainForm()
    {
        InitializeComponent();
        InitializeEventHandlers();
        _bindingSource.DataSource = _pageItems;
        listBoxPages.DataSource = _bindingSource;
    }

    /// <summary>
    /// 註冊所有事件處理器。
    /// </summary>
    private void InitializeEventHandlers()
    {
        // 拖曳事件
        panelDragDrop.DragEnter += OnDragEnter;
        panelDragDrop.DragDrop += OnDragDrop;
        listBoxPages.DragEnter += OnDragEnter;
        listBoxPages.DragDrop += OnDragDrop;
        listBoxPages.AllowDrop = true;

        // 列表選取事件 → 觸發預覽
        listBoxPages.SelectedIndexChanged += ListBoxPages_SelectedIndexChanged;

        // 按鈕事件
        btnMoveUp.Click += BtnMoveUp_Click;
        btnMoveDown.Click += BtnMoveDown_Click;
        btnRemoveSelected.Click += BtnRemoveSelected_Click;
        btnClearAll.Click += BtnClearAll_Click;
        btnMerge.Click += BtnMerge_Click;

        // 預覽導覽
        btnPrevPage.Click += BtnPrevPage_Click;
        btnNextPage.Click += BtnNextPage_Click;
        btnRemovePage.Click += BtnRemovePage_Click;

        // 畫布與工具列事件
        InitializeDrawingEvents();
    }

    // ─── 拖曳處理 ───

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files) return;

        var (validFiles, invalidMessages) = FileValidationHelper.ValidateFiles(files);

        foreach (var path in validFiles)
        {
            try
            {
                var pageCount = _previewService.GetPageCount(path);
                for (var i = 0; i < pageCount; i++)
                {
                    _pageItems.Add(new PdfPageItem(path, i));
                }
            }
            catch (Exception ex)
            {
                invalidMessages.Add($"無法讀取 {Path.GetFileName(path)}：{ex.Message}");
            }
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
        var index = listBoxPages.SelectedIndex;
        if (index <= 0) return;

        (_pageItems[index], _pageItems[index - 1]) = (_pageItems[index - 1], _pageItems[index]);
        RefreshList();
        listBoxPages.ClearSelected();
        listBoxPages.SelectedIndex = index - 1;
    }

    private void BtnMoveDown_Click(object? sender, EventArgs e)
    {
        var index = listBoxPages.SelectedIndex;
        if (index < 0 || index >= _pageItems.Count - 1) return;

        (_pageItems[index], _pageItems[index + 1]) = (_pageItems[index + 1], _pageItems[index]);
        RefreshList();
        listBoxPages.ClearSelected();
        listBoxPages.SelectedIndex = index + 1;
    }

    private void BtnRemoveSelected_Click(object? sender, EventArgs e)
    {
        var indices = listBoxPages.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        if (indices.Count == 0) return;

        var lastSelectedIndex = indices.Min(); // 最上面的被選取項目索引

        foreach (var index in indices)
        {
            _pageItems.RemoveAt(index);
        }

        RefreshList();

        if (_pageItems.Count == 0)
        {
            ClearPreview();
        }
        else
        {
            // 選取最上方移除項的下一個位置，若超過則選最後一項
            var newIndex = lastSelectedIndex >= _pageItems.Count ? _pageItems.Count - 1 : lastSelectedIndex;
            listBoxPages.ClearSelected();
            listBoxPages.SelectedIndex = newIndex;
        }
    }

    private void BtnClearAll_Click(object? sender, EventArgs e)
    {
        _pageItems.Clear();
        RefreshList();
        ClearPreview();
    }

    // ─── 預覽 ───

    private async void ListBoxPages_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var index = listBoxPages.SelectedIndex;
        if (index < 0 || index >= _pageItems.Count)
        {
            ClearPreview();
            return;
        }

        _previewIndex = index;
        await RenderPreviewAsync(index);
    }

    private async Task RenderPreviewAsync(int index)
    {
        if (index < 0 || index >= _pageItems.Count) return;

        var page = _pageItems[index];
        try
        {
            var image = await _previewService.RenderPageAsync(page.SourceFilePath, page.PageIndex);
            // 確保預覽索引未在渲染期間改變
            if (_previewIndex == index)
            {
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = image;
                UpdatePageInfo(index);
            }
            else
            {
                image.Dispose();
            }
        }
        catch (Exception ex)
        {
            pictureBoxPreview.Image = null;
            lblPageInfo.Text = string.Format(PreviewErrorMessage, ex.Message);
        }
    }

    private void UpdatePageInfo(int index)
    {
        lblPageInfo.Text = $"第 {index + 1} 頁 / 共 {_pageItems.Count} 頁";
        btnPrevPage.Enabled = index > 0;
        btnNextPage.Enabled = index < _pageItems.Count - 1;
        btnRemovePage.Enabled = true;
    }

    private void ClearPreview()
    {
        _previewIndex = -1;
        pictureBoxPreview.Image?.Dispose();
        pictureBoxPreview.Image = null;
        lblPageInfo.Text = "";
        btnPrevPage.Enabled = false;
        btnNextPage.Enabled = false;
        btnRemovePage.Enabled = false;
    }

    // ─── 預覽導覽 ───

    private void BtnPrevPage_Click(object? sender, EventArgs e)
    {
        var targetIndex = _previewIndex - 1;
        if (targetIndex >= 0)
        {
            listBoxPages.ClearSelected();
            listBoxPages.SelectedIndex = targetIndex;
        }
    }

    private void BtnNextPage_Click(object? sender, EventArgs e)
    {
        var targetIndex = _previewIndex + 1;
        if (targetIndex > 0 && targetIndex < _pageItems.Count)
        {
            listBoxPages.ClearSelected();
            listBoxPages.SelectedIndex = targetIndex;
        }
    }

    private void BtnRemovePage_Click(object? sender, EventArgs e)
    {
        if (_previewIndex < 0 || _previewIndex >= _pageItems.Count) return;

        var removeIndex = _previewIndex;
        _pageItems.RemoveAt(removeIndex);
        RefreshList();

        if (_pageItems.Count == 0)
        {
            ClearPreview();
            return;
        }

        // 移除後選取下一頁，若已是最後一頁則選前一頁
        var newIndex = removeIndex >= _pageItems.Count ? _pageItems.Count - 1 : removeIndex;
        listBoxPages.ClearSelected();
        listBoxPages.SelectedIndex = newIndex;
    }

    // ─── 合併操作 ───

    private async void BtnMerge_Click(object? sender, EventArgs e)
    {
        if (_pageItems.Count == 0)
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
            await _mergeService.MergeAsync(_pageItems, outputPath);

            MessageBox.Show(
                string.Format(MergeSuccessMessage, outputPath),
                AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

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

    private void RefreshList()
    {
        _bindingSource.ResetBindings(false);
        UpdateStatusBar();
        btnMerge.Enabled = _pageItems.Count > 0;
    }

    private void SetUiBusy(bool busy)
    {
        btnMerge.Enabled = !busy;
        btnMoveUp.Enabled = !busy;
        btnMoveDown.Enabled = !busy;
        btnRemoveSelected.Enabled = !busy;
        btnClearAll.Enabled = !busy;
        btnPrevPage.Enabled = !busy;
        btnNextPage.Enabled = !busy;
        btnRemovePage.Enabled = !busy;
        panelDragDrop.AllowDrop = !busy;
        listBoxPages.AllowDrop = !busy;
        toolStripStatusLabel.Text = busy ? ProcessingMessage : $"共 {_pageItems.Count} 頁。";
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void UpdateStatusBar()
    {
        toolStripStatusLabel.Text = _pageItems.Count == 0
            ? "就緒，尚無檔案。"
            : $"共 {_pageItems.Count} 頁。";
    }

    // ─── 繪圖與畫布互動 ───

    private void InitializeDrawingEvents()
    {
        btnDrawSelect.Click += (s, e) => SetDrawMode(DrawMode.Select, btnDrawSelect);
        btnDrawText.Click += (s, e) => SetDrawMode(DrawMode.Text, btnDrawText);
        btnDrawLine.Click += (s, e) => SetDrawMode(DrawMode.Line, btnDrawLine);
        btnDrawArrow.Click += (s, e) => SetDrawMode(DrawMode.Arrow, btnDrawArrow);
        btnDrawRect.Click += (s, e) => SetDrawMode(DrawMode.Rect, btnDrawRect);
        btnDrawMosaic.Click += (s, e) => SetDrawMode(DrawMode.Mosaic, btnDrawMosaic);
        btnDrawEllipse.Click += (s, e) => SetDrawMode(DrawMode.Ellipse, btnDrawEllipse);

        btnDrawColor.Click += BtnDrawColor_Click;
        btnDrawFont.Click += BtnDrawFont_Click;
        btnDrawDelete.Click += BtnDrawDelete_Click;
        btnDrawClear.Click += BtnDrawClear_Click;

        pictureBoxPreview.Paint += PictureBoxPreview_Paint;
        pictureBoxPreview.MouseDown += PictureBoxPreview_MouseDown;
        pictureBoxPreview.MouseMove += PictureBoxPreview_MouseMove;
        pictureBoxPreview.MouseUp += PictureBoxPreview_MouseUp;
        pictureBoxPreview.DoubleClick += PictureBoxPreview_DoubleClick;

        SetDrawMode(DrawMode.Select, btnDrawSelect);
    }

    private void SetDrawMode(DrawMode mode, ToolStripButton activeButton, bool keepSelection = false)
    {
        _currentDrawMode = mode;
        var buttons = new[] { btnDrawSelect, btnDrawText, btnDrawLine, btnDrawArrow, btnDrawRect, btnDrawMosaic, btnDrawEllipse };
        foreach (var btn in buttons) btn.Checked = (btn == activeButton);
        if (!keepSelection)
        {
            _selectedAnnotation = null;
        }
        UpdateToolBarState();
        pictureBoxPreview.Invalidate();
    }

    /// 資料對「筆別」選項反灰控制：只有文字標註被選取時才可按字型按鈕。
    private void UpdateToolBarState()
    {
        bool hasSelection = _selectedAnnotation != null;
        bool isTextSelected = _selectedAnnotation is TextAnnotation;
        btnDrawFont.Enabled = isTextSelected;
        btnDrawDelete.Enabled = hasSelection;
    }

    private float GetImageScale()
    {
        if (pictureBoxPreview.Image == null) return 1f;
        return Math.Min((float)pictureBoxPreview.Width / pictureBoxPreview.Image.Width, (float)pictureBoxPreview.Height / pictureBoxPreview.Image.Height);
    }

    private PointF GetImageOffset()
    {
        if (pictureBoxPreview.Image == null) return PointF.Empty;
        float scale = GetImageScale();
        return new PointF((pictureBoxPreview.Width - pictureBoxPreview.Image.Width * scale) / 2f,
                          (pictureBoxPreview.Height - pictureBoxPreview.Image.Height * scale) / 2f);
    }

    private PointF ControlToImage(Point controlPoint)
    {
        float scale = GetImageScale();
        var offset = GetImageOffset();
        return new PointF((controlPoint.X - offset.X) / scale, (controlPoint.Y - offset.Y) / scale);
    }

    private RectangleF GetResizeHandleRect(RectangleF bounds, float scale)
    {
        return new RectangleF(bounds.Right * scale - ResizeHandleSize / 2, bounds.Bottom * scale - ResizeHandleSize / 2, ResizeHandleSize, ResizeHandleSize);
    }
    
    private void DrawMosaicPreview(Graphics g, Image? sourceImage, MosaicAnnotation mosaic, float scale)
    {
        if (sourceImage == null)
        {
            mosaic.Draw(g, scale);
            return;
        }

        var imageRect = new RectangleF(0, 0, sourceImage.Width, sourceImage.Height);
        var clipped = RectangleF.Intersect(mosaic.Bounds, imageRect);
        if (clipped.Width <= 0 || clipped.Height <= 0) return;

        var srcRect = Rectangle.Round(clipped);
        if (srcRect.Width <= 0 || srcRect.Height <= 0) return;

        int blockSize = Math.Max(4, mosaic.BlockSize);
        int smallWidth = Math.Max(1, (int)(clipped.Width / blockSize));
        int smallHeight = Math.Max(1, (int)(clipped.Height / blockSize));

        using var small = new Bitmap(smallWidth, smallHeight);
        using (var sg = Graphics.FromImage(small))
        {
            sg.InterpolationMode = InterpolationMode.HighQualityBilinear;
            sg.PixelOffsetMode = PixelOffsetMode.HighQuality;
            sg.DrawImage(sourceImage, new Rectangle(0, 0, smallWidth, smallHeight), srcRect, GraphicsUnit.Pixel);
        }

        var destRect = new RectangleF(clipped.X * scale, clipped.Y * scale, clipped.Width * scale, clipped.Height * scale);
        var oldInterpolation = g.InterpolationMode;
        var oldPixelOffset = g.PixelOffsetMode;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(small, destRect);
        g.InterpolationMode = oldInterpolation;
        g.PixelOffsetMode = oldPixelOffset;
    }

    private void PictureBoxPreview_Paint(object? sender, PaintEventArgs e)
    {
        if (_previewIndex < 0 || _previewIndex >= _pageItems.Count || pictureBoxPreview.Image == null) return;
        
        var page = _pageItems[_previewIndex];
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        float scale = GetImageScale();
        var offset = GetImageOffset();
        e.Graphics.TranslateTransform(offset.X, offset.Y);

        foreach (var ann in page.Annotations)
        {
            // 正在就地編輯的標註不渲染其文字，改由 TextBox 即時顯示
            if (ann == _editingTextAnn) continue;

            if (ann is MosaicAnnotation mosaicAnn)
            {
                DrawMosaicPreview(e.Graphics, pictureBoxPreview.Image, mosaicAnn, scale);
            }
            else
            {
                ann.Draw(e.Graphics, scale);
            }
            if (ann == _selectedAnnotation && _currentDrawMode == DrawMode.Select)
            {
                using var dashPen = new Pen(Color.Blue, 1.5f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                var bounds = new RectangleF(ann.Bounds.X * scale, ann.Bounds.Y * scale, ann.Bounds.Width * scale, ann.Bounds.Height * scale);
                e.Graphics.DrawRectangle(dashPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                
                var handle = GetResizeHandleRect(ann.Bounds, scale);
                e.Graphics.FillRectangle(Brushes.White, handle);
                e.Graphics.DrawRectangle(Pens.Blue, handle.X, handle.Y, handle.Width, handle.Height);
            }
        }

        if (_drawingAnnotation is MosaicAnnotation mosaicDrawing)
        {
            DrawMosaicPreview(e.Graphics, pictureBoxPreview.Image, mosaicDrawing, scale);
        }
        else
        {
            _drawingAnnotation?.Draw(e.Graphics, scale);
        }
    }

    private void PictureBoxPreview_MouseDown(object? sender, MouseEventArgs e)
    {
        // 若有就地輸入框，先提交後再處理點擊
        if (_inlineTextBox != null)
        {
            CommitInlineEdit(cancel: false);
            return;
        }
        if (_previewIndex < 0 || pictureBoxPreview.Image == null) return;
        var page = _pageItems[_previewIndex];
        var imgPoint = ControlToImage(e.Location);

        if (_currentDrawMode == DrawMode.Select)
        {
            if (_selectedAnnotation != null)
            {
                var handleRect = GetResizeHandleRect(_selectedAnnotation.Bounds, GetImageScale());
                // Transform HitTest for handle (which is in screen space relative to scaled offset)
                var handleImgRect = new RectangleF(handleRect.X / GetImageScale(), handleRect.Y / GetImageScale(), handleRect.Width / GetImageScale(), handleRect.Height / GetImageScale());
                if (handleImgRect.Contains(imgPoint))
                {
                    _isResizing = true;
                    _dragStartPoint = imgPoint;
                    _dragStartBounds = _selectedAnnotation.Bounds;
                    return;
                }
            }

            // 反向尋找以點擊最上層
            _selectedAnnotation = null;
            for (int i = page.Annotations.Count - 1; i >= 0; i--)
            {
                if (page.Annotations[i].HitTest(imgPoint))
                {
                    _selectedAnnotation = page.Annotations[i];
                    break;
                }
            }

            if (_selectedAnnotation != null)
            {
                _isDragging = true;
                _dragStartPoint = imgPoint;
                _dragStartBounds = _selectedAnnotation.Bounds;
                if (_selectedAnnotation is LineAnnotation line)
                {
                    _dragStartLineStart = line.StartPoint;
                    _dragStartLineEnd = line.EndPoint;
                }
            }
            UpdateToolBarState();
            pictureBoxPreview.Invalidate();
        }
        else
        {
            _dragStartPoint = imgPoint;
            switch (_currentDrawMode)
            {
                case DrawMode.Text:
                    _drawingAnnotation = new TextAnnotation { Bounds = new RectangleF(imgPoint.X, imgPoint.Y, 150, 50), Color = _currentDrawColor, FontName = _currentDrawFont.Name, FontSize = _currentDrawFont.Size };
                    break;
                case DrawMode.Line:
                    _drawingAnnotation = new LineAnnotation { StartPoint = imgPoint, EndPoint = imgPoint, Color = _currentDrawColor };
                    break;
                case DrawMode.Arrow:
                    _drawingAnnotation = new ArrowAnnotation { StartPoint = imgPoint, EndPoint = imgPoint, Color = _currentDrawColor };
                    break;
                case DrawMode.Rect:
                    _drawingAnnotation = new RectAnnotation { Bounds = new RectangleF(imgPoint.X, imgPoint.Y, 0, 0), Color = _currentDrawColor };
                    break;
                case DrawMode.Mosaic:
                    _drawingAnnotation = new MosaicAnnotation { Bounds = new RectangleF(imgPoint.X, imgPoint.Y, 0, 0) };
                    break;
                case DrawMode.Ellipse:
                    _drawingAnnotation = new EllipseAnnotation { Bounds = new RectangleF(imgPoint.X, imgPoint.Y, 0, 0), Color = _currentDrawColor };
                    break;
            }
            pictureBoxPreview.Invalidate();
        }
    }

    private void PictureBoxPreview_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_previewIndex < 0 || pictureBoxPreview.Image == null) return;
        var imgPoint = ControlToImage(e.Location);

        if (_isResizing && _selectedAnnotation != null)
        {
            float newWidth = Math.Max(10, _dragStartBounds.Width + (imgPoint.X - _dragStartPoint.X));
            float newHeight = Math.Max(10, _dragStartBounds.Height + (imgPoint.Y - _dragStartPoint.Y));
            _selectedAnnotation.Bounds = new RectangleF(_dragStartBounds.X, _dragStartBounds.Y, newWidth, newHeight);
            
            if (_selectedAnnotation is LineAnnotation line)
            {
                line.EndPoint = imgPoint;
                line.Bounds = new RectangleF(Math.Min(line.StartPoint.X, line.EndPoint.X), Math.Min(line.StartPoint.Y, line.EndPoint.Y), 
                                             Math.Abs(line.StartPoint.X - line.EndPoint.X), Math.Abs(line.StartPoint.Y - line.EndPoint.Y));
            }
            pictureBoxPreview.Invalidate();
        }
        else if (_isDragging && _selectedAnnotation != null)
        {
            float dx = imgPoint.X - _dragStartPoint.X;
            float dy = imgPoint.Y - _dragStartPoint.Y;
            _selectedAnnotation.Bounds = new RectangleF(_dragStartBounds.X + dx, _dragStartBounds.Y + dy, _dragStartBounds.Width, _dragStartBounds.Height);
            
            if (_selectedAnnotation is LineAnnotation line)
            {
                line.StartPoint = new PointF(_dragStartLineStart.X + dx, _dragStartLineStart.Y + dy);
                line.EndPoint = new PointF(_dragStartLineEnd.X + dx, _dragStartLineEnd.Y + dy);
            }
            pictureBoxPreview.Invalidate();
        }
        else if (_drawingAnnotation != null)
        {
            float x = Math.Min(_dragStartPoint.X, imgPoint.X);
            float y = Math.Min(_dragStartPoint.Y, imgPoint.Y);
            float w = Math.Abs(imgPoint.X - _dragStartPoint.X);
            float h = Math.Abs(imgPoint.Y - _dragStartPoint.Y);

            if (_drawingAnnotation is LineAnnotation line)
            {
                line.EndPoint = imgPoint;
                line.Bounds = new RectangleF(x, y, w, h);
            }
            else if (_drawingAnnotation is TextAnnotation text)
            {
                 text.Bounds = new RectangleF(x, y, Math.Max(150, w), Math.Max(50, h));
            }
            else
            {
                _drawingAnnotation.Bounds = new RectangleF(x, y, w, h);
            }
            pictureBoxPreview.Invalidate();
        }
    }

    private void PictureBoxPreview_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isResizing || _isDragging)
        {
            _isResizing = false;
            _isDragging = false;
            if (_selectedAnnotation is TextAnnotation textAnn && e.Button == MouseButtons.Right)
            {
                // 右鍵可以快速重新編輯文字
                StartInlineEdit(textAnn);
            }
        }
        else if (_drawingAnnotation != null && _previewIndex >= 0)
        {
            if (_drawingAnnotation is TextAnnotation text && text.Bounds.Width < 10 && text.Bounds.Height < 10)
            {
                 text.Bounds = new RectangleF(_dragStartPoint.X, _dragStartPoint.Y, 200, 50);
            }
            
            // 忽略太小的圖形
            if (_drawingAnnotation is not TextAnnotation && _drawingAnnotation is not LineAnnotation && 
                (_drawingAnnotation.Bounds.Width < 5 || _drawingAnnotation.Bounds.Height < 5))
            {
                _drawingAnnotation = null;
                pictureBoxPreview.Invalidate();
                return;
            }

            _pageItems[_previewIndex].Annotations.Add(_drawingAnnotation);

            _selectedAnnotation = _drawingAnnotation;
            _drawingAnnotation = null;
            // keepSelection=true 讓切回選取模式不清除剛結束繪製的物件
            SetDrawMode(DrawMode.Select, btnDrawSelect, keepSelection: true);

            // 文字標註新增後直接就地編輯
            if (_selectedAnnotation is TextAnnotation textAnn)
            {
                StartInlineEdit(textAnn);
            }
        }
    }

    private void PictureBoxPreview_DoubleClick(object? sender, EventArgs e)
    {
        // 鷢擊選取中的文字標註 → 就地編輯
        if (_selectedAnnotation is TextAnnotation textAnn)
            StartInlineEdit(textAnn);
    }

    /// 在預覽圖上的標註位置顯示漏出式 TextBox 就地編輯 文字標註。
    private void StartInlineEdit(TextAnnotation textAnn)
    {
        CommitInlineEdit(cancel: false); // 若正在編輯其他先確認

        _editingTextAnn = textAnn;
        float scale = GetImageScale();
        var offset = GetImageOffset();

        float sx = textAnn.Bounds.X * scale + offset.X;
        float sy = textAnn.Bounds.Y * scale + offset.Y;
        float sw = Math.Max(textAnn.Bounds.Width * scale, 200);
        float sh = Math.Max(textAnn.Bounds.Height * scale, 60);

        // 轉換成相對於 Form 的座標
        var ptInForm = pictureBoxPreview.PointToScreen(new Point((int)sx, (int)sy));
        ptInForm = PointToClient(ptInForm);

        _inlineTextBox = new TextBox
        {
            Multiline = true,
            WordWrap = true,
            AcceptsReturn = true, // 支援 Enter 換行
            ScrollBars = ScrollBars.None, // 不顯示捲軸
            Location = ptInForm,
            Size = new Size((int)sw, (int)sh),
            Font = new Font(textAnn.FontName, Math.Max(8, textAnn.FontSize * scale)),
            ForeColor = textAnn.Color,
            BackColor = Color.FromArgb(255, 255, 220), 
            BorderStyle = BorderStyle.FixedSingle,
            Text = textAnn.Text == "輸入文字" ? "" : textAnn.Text,
        };
        _inlineTextBox.KeyDown += InlineTextBox_KeyDown;
        _inlineTextBox.Leave += InlineTextBox_Leave;
        Controls.Add(_inlineTextBox);
        _inlineTextBox.BringToFront();
        _inlineTextBox.Focus();
    }

    private void InlineTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            CommitInlineEdit(cancel: true);
        }
    }

    private void InlineTextBox_Leave(object? sender, EventArgs e)
    {
        CommitInlineEdit(cancel: false);
    }

    private void CommitInlineEdit(bool cancel)
    {
        if (_inlineTextBox == null || _editingTextAnn == null) return;

        var textBox = _inlineTextBox;
        var ann = _editingTextAnn;
        
        // 1. 先讀取資料，避免 Dispose 後物件失效
        string newText = textBox.Text;

        // 2. 清理變數與控制項
        _inlineTextBox = null;
        _editingTextAnn = null;
        textBox.Leave -= InlineTextBox_Leave;
        textBox.KeyDown -= InlineTextBox_KeyDown;
        Controls.Remove(textBox);
        textBox.Dispose();

        // 3. 套用變更
        if (!cancel && !string.IsNullOrWhiteSpace(newText))
        {
            ann.Text = newText;
        }
        else if ((cancel || string.IsNullOrWhiteSpace(newText)) && ann.Text == "輸入文字")
        {
            // 若是新增時（Text為預設）取消或打空，則刪除
            _pageItems[_previewIndex].Annotations.Remove(ann);
            if (_selectedAnnotation == ann) _selectedAnnotation = null;
        }

        pictureBoxPreview.Invalidate();
        UpdateToolBarState();
    }

    private void BtnDrawColor_Click(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog { Color = _currentDrawColor };
        if (cd.ShowDialog() == DialogResult.OK)
        {
            _currentDrawColor = cd.Color;
            if (_selectedAnnotation != null)
            {
                _selectedAnnotation.Color = _currentDrawColor;
                pictureBoxPreview.Invalidate();
            }
        }
    }

    private void BtnDrawFont_Click(object? sender, EventArgs e)
    {
        using var fd = new FontDialog { Font = _currentDrawFont };
        if (fd.ShowDialog() == DialogResult.OK)
        {
            _currentDrawFont = fd.Font;
            if (_selectedAnnotation is TextAnnotation text)
            {
                text.FontName = fd.Font.Name;
                text.FontSize = fd.Font.Size;
                pictureBoxPreview.Invalidate();
            }
        }
    }

    private void BtnDrawDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedAnnotation != null && _previewIndex >= 0)
        {
            _pageItems[_previewIndex].Annotations.Remove(_selectedAnnotation);
            _selectedAnnotation = null;
            pictureBoxPreview.Invalidate();
        }
    }

    private void BtnDrawClear_Click(object? sender, EventArgs e)
    {
        if (_previewIndex >= 0)
        {
            _pageItems[_previewIndex].Annotations.Clear();
            _selectedAnnotation = null;
            pictureBoxPreview.Invalidate();
        }
    }
}







