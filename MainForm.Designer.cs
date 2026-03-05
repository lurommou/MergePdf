namespace MergePdf;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        panelDragDrop = new Panel();
        lblDragHint = new Label();
        listBoxPages = new ListBox();
        btnMoveUp = new Button();
        btnMoveDown = new Button();
        btnRemoveSelected = new Button();
        btnClearAll = new Button();
        btnMerge = new Button();
        pictureBoxPreview = new PictureBox();
        panelPreviewNav = new Panel();
        btnPrevPage = new Button();
        btnNextPage = new Button();
        lblPageInfo = new Label();
        btnRemovePage = new Button();
        statusStrip = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();
        toolStripStatusLabelBrand = new ToolStripStatusLabel();

        toolStripDrawing = new ToolStrip();
        btnDrawSelect = new ToolStripButton();
        btnDrawText = new ToolStripButton();
        btnDrawLine = new ToolStripButton();
        btnDrawArrow = new ToolStripButton();
        btnDrawRect = new ToolStripButton();
        btnDrawEllipse = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        btnDrawColor = new ToolStripButton();
        btnDrawFont = new ToolStripButton();
        toolStripSeparator2 = new ToolStripSeparator();
        btnDrawDelete = new ToolStripButton();
        btnDrawClear = new ToolStripButton();

        ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
        SuspendLayout();

        var leftWidth = 340;
        var btnColX = leftWidth + 16;
        var btnWidth = 80;
        var previewX = btnColX + btnWidth + 10;
        var previewWidth = 480;
        var totalWidth = previewX + previewWidth + 16;

        // === 拖曳區域 ===
        panelDragDrop.AllowDrop = true;
        panelDragDrop.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
        panelDragDrop.BorderStyle = BorderStyle.FixedSingle;
        panelDragDrop.Location = new System.Drawing.Point(12, 12);
        panelDragDrop.Name = "panelDragDrop";
        panelDragDrop.Size = new System.Drawing.Size(leftWidth, 60);
        panelDragDrop.TabIndex = 0;
        panelDragDrop.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        lblDragHint.AutoSize = false;
        lblDragHint.Dock = DockStyle.Fill;
        lblDragHint.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11F);
        lblDragHint.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
        lblDragHint.Name = "lblDragHint";
        lblDragHint.Text = "📂 將 PDF 檔案拖曳至此處";
        lblDragHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        panelDragDrop.Controls.Add(lblDragHint);

        // === 頁面列表 ===
        listBoxPages.FormattingEnabled = true;
        listBoxPages.HorizontalScrollbar = true;
        listBoxPages.Location = new System.Drawing.Point(12, 80);
        listBoxPages.Name = "listBoxPages";
        listBoxPages.SelectionMode = SelectionMode.MultiExtended;
        listBoxPages.Size = new System.Drawing.Size(leftWidth, 410);
        listBoxPages.TabIndex = 1;
        listBoxPages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

        // === 操作按鈕 ===
        btnMoveUp.Location = new System.Drawing.Point(btnColX, 80);
        btnMoveUp.Name = "btnMoveUp";
        btnMoveUp.Size = new System.Drawing.Size(btnWidth, 34);
        btnMoveUp.TabIndex = 2;
        btnMoveUp.Text = "⬆ 上移";
        btnMoveUp.UseVisualStyleBackColor = true;

        btnMoveDown.Location = new System.Drawing.Point(btnColX, 118);
        btnMoveDown.Name = "btnMoveDown";
        btnMoveDown.Size = new System.Drawing.Size(btnWidth, 34);
        btnMoveDown.TabIndex = 3;
        btnMoveDown.Text = "⬇ 下移";
        btnMoveDown.UseVisualStyleBackColor = true;

        btnRemoveSelected.Location = new System.Drawing.Point(btnColX, 166);
        btnRemoveSelected.Name = "btnRemoveSelected";
        btnRemoveSelected.Size = new System.Drawing.Size(btnWidth, 34);
        btnRemoveSelected.TabIndex = 4;
        btnRemoveSelected.Text = "移除選取";
        btnRemoveSelected.UseVisualStyleBackColor = true;

        btnClearAll.Location = new System.Drawing.Point(btnColX, 204);
        btnClearAll.Name = "btnClearAll";
        btnClearAll.Size = new System.Drawing.Size(btnWidth, 34);
        btnClearAll.TabIndex = 5;
        btnClearAll.Text = "清空列表";
        btnClearAll.UseVisualStyleBackColor = true;

        btnMerge.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
        btnMerge.Location = new System.Drawing.Point(btnColX, 440);
        btnMerge.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnMerge.Name = "btnMerge";
        btnMerge.Size = new System.Drawing.Size(btnWidth, 50);
        btnMerge.TabIndex = 6;
        btnMerge.Text = "合併 PDF";
        btnMerge.UseVisualStyleBackColor = true;
        btnMerge.Enabled = false;

        // === 繪圖工具列 ===
        toolStripDrawing.Dock = DockStyle.None;
        toolStripDrawing.Location = new System.Drawing.Point(previewX, 12);
        toolStripDrawing.Name = "toolStripDrawing";
        toolStripDrawing.Size = new System.Drawing.Size(previewWidth, 60);
        toolStripDrawing.TabIndex = 10;
        toolStripDrawing.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        toolStripDrawing.AutoSize = false;
        toolStripDrawing.GripStyle = ToolStripGripStyle.Hidden;
        toolStripDrawing.Items.AddRange(new ToolStripItem[] {
            btnDrawSelect,
            btnDrawText,
            btnDrawLine,
            btnDrawArrow,
            btnDrawRect,
            btnDrawEllipse,
            toolStripSeparator1,
            btnDrawColor,
            btnDrawFont,
            toolStripSeparator2,
            btnDrawDelete,
            btnDrawClear
        });

        // 設定工具列按鈕樣式 (文字顯示)
        foreach (var item in toolStripDrawing.Items.OfType<ToolStripButton>())
        {
            item.DisplayStyle = ToolStripItemDisplayStyle.Text;
            item.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F);
            item.Padding = new Padding(4);
        }

        btnDrawSelect.Text = "👆 選取";
        btnDrawText.Text = "🔤 文字";
        btnDrawLine.Text = "📏 直線";
        btnDrawArrow.Text = "↗ 箭頭";
        btnDrawRect.Text = "⏹ 矩形";
        btnDrawEllipse.Text = "⭕ 圓形";

        btnDrawColor.Text = "🎨 色彩";
        btnDrawFont.Text = "🔠 字型";
        
        btnDrawDelete.Text = "🗑 刪除選取";
        btnDrawClear.Text = "🧹 清空這頁";

        // === 預覽區域 ===
        pictureBoxPreview.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        pictureBoxPreview.BorderStyle = BorderStyle.FixedSingle;
        pictureBoxPreview.Location = new System.Drawing.Point(previewX, 80);
        pictureBoxPreview.Name = "pictureBoxPreview";
        pictureBoxPreview.Size = new System.Drawing.Size(previewWidth, 370);
        pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBoxPreview.TabIndex = 7;
        pictureBoxPreview.TabStop = true;   // 可接收焦點，確保點擊時觸發 TextBox.Leave
        pictureBoxPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        // === 預覽導覽列 ===
        panelPreviewNav.Location = new System.Drawing.Point(previewX, 456);
        panelPreviewNav.Name = "panelPreviewNav";
        panelPreviewNav.Size = new System.Drawing.Size(previewWidth, 34);
        panelPreviewNav.TabIndex = 8;
        panelPreviewNav.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        btnPrevPage.Location = new System.Drawing.Point(0, 0);
        btnPrevPage.Name = "btnPrevPage";
        btnPrevPage.Size = new System.Drawing.Size(50, 34);
        btnPrevPage.TabIndex = 0;
        btnPrevPage.Text = "◀";
        btnPrevPage.UseVisualStyleBackColor = true;

        lblPageInfo.AutoSize = false;
        lblPageInfo.Location = new System.Drawing.Point(55, 0);
        lblPageInfo.Name = "lblPageInfo";
        lblPageInfo.Size = new System.Drawing.Size(previewWidth - 110 - 100, 34); // 動態填補中間空位
        lblPageInfo.Text = "";
        lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblPageInfo.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9.5F);
        lblPageInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        btnNextPage.Location = new System.Drawing.Point(previewWidth - 150 - 5, 0);
        btnNextPage.Name = "btnNextPage";
        btnNextPage.Size = new System.Drawing.Size(50, 34);
        btnNextPage.TabIndex = 2;
        btnNextPage.Text = "▶";
        btnNextPage.UseVisualStyleBackColor = true;
        btnNextPage.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnRemovePage.Location = new System.Drawing.Point(previewWidth - 100, 0);
        btnRemovePage.Name = "btnRemovePage";
        btnRemovePage.Size = new System.Drawing.Size(100, 34);
        btnRemovePage.TabIndex = 3;
        btnRemovePage.Text = "移除此頁";
        btnRemovePage.UseVisualStyleBackColor = true;
        btnRemovePage.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        panelPreviewNav.Controls.Add(btnPrevPage);
        panelPreviewNav.Controls.Add(lblPageInfo);
        panelPreviewNav.Controls.Add(btnNextPage);
        panelPreviewNav.Controls.Add(btnRemovePage);

        // === 狀態列 ===
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Text = "就緒，尚無檔案。";
        toolStripStatusLabel.Spring = true;
        toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        toolStripStatusLabelBrand.Name = "toolStripStatusLabelBrand";
        toolStripStatusLabelBrand.Text = "帥茂出品";
        toolStripStatusLabelBrand.Alignment = ToolStripItemAlignment.Right;
        statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel, toolStripStatusLabelBrand });
        statusStrip.Location = new System.Drawing.Point(0, 498);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new System.Drawing.Size(totalWidth, 22);
        statusStrip.TabIndex = 9;

        // === 主視窗 ===
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(totalWidth, 520);
        Controls.Add(panelDragDrop);
        Controls.Add(listBoxPages);
        Controls.Add(btnMoveUp);
        Controls.Add(btnMoveDown);
        Controls.Add(btnRemoveSelected);
        Controls.Add(btnClearAll);
        Controls.Add(btnMerge);
        Controls.Add(toolStripDrawing);
        Controls.Add(pictureBoxPreview);
        Controls.Add(panelPreviewNav);
        Controls.Add(statusStrip);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new System.Drawing.Size(totalWidth + 16, 520 + 39);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PDF 合併工具";

        ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private Panel panelDragDrop;
    private Label lblDragHint;
    private ListBox listBoxPages;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private Button btnRemoveSelected;
    private Button btnClearAll;
    private Button btnMerge;
    private PictureBox pictureBoxPreview;
    private Panel panelPreviewNav;
    private Button btnPrevPage;
    private Button btnNextPage;
    private Label lblPageInfo;
    private Button btnRemovePage;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel toolStripStatusLabel;
    private ToolStripStatusLabel toolStripStatusLabelBrand;

    // 繪圖工具列宣告
    private ToolStrip toolStripDrawing;
    private ToolStripButton btnDrawSelect;
    private ToolStripButton btnDrawText;
    private ToolStripButton btnDrawLine;
    private ToolStripButton btnDrawArrow;
    private ToolStripButton btnDrawRect;
    private ToolStripButton btnDrawEllipse;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton btnDrawColor;
    private ToolStripButton btnDrawFont;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton btnDrawDelete;
    private ToolStripButton btnDrawClear;
}
