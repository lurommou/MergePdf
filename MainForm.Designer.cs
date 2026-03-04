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
        listBoxFiles = new ListBox();
        btnMoveUp = new Button();
        btnMoveDown = new Button();
        btnRemoveSelected = new Button();
        btnClearAll = new Button();
        btnMerge = new Button();
        statusStrip = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();

        SuspendLayout();

        // === 拖曳區域 ===
        panelDragDrop.AllowDrop = true;
        panelDragDrop.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
        panelDragDrop.BorderStyle = BorderStyle.FixedSingle;
        panelDragDrop.Location = new System.Drawing.Point(12, 12);
        panelDragDrop.Name = "panelDragDrop";
        panelDragDrop.Size = new System.Drawing.Size(560, 80);
        panelDragDrop.TabIndex = 0;

        lblDragHint.AutoSize = false;
        lblDragHint.Dock = DockStyle.Fill;
        lblDragHint.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular);
        lblDragHint.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
        lblDragHint.Name = "lblDragHint";
        lblDragHint.Text = "📂 將 PDF 檔案拖曳至此處";
        lblDragHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        panelDragDrop.Controls.Add(lblDragHint);

        // === 檔案列表 ===
        listBoxFiles.FormattingEnabled = true;
        listBoxFiles.HorizontalScrollbar = true;
        listBoxFiles.Location = new System.Drawing.Point(12, 100);
        listBoxFiles.Name = "listBoxFiles";
        listBoxFiles.SelectionMode = SelectionMode.MultiExtended;
        listBoxFiles.Size = new System.Drawing.Size(560, 290);
        listBoxFiles.TabIndex = 1;

        // === 操作按鈕 ===
        var btnX = 580;
        var btnWidth = 100;

        btnMoveUp.Location = new System.Drawing.Point(btnX, 100);
        btnMoveUp.Name = "btnMoveUp";
        btnMoveUp.Size = new System.Drawing.Size(btnWidth, 36);
        btnMoveUp.TabIndex = 2;
        btnMoveUp.Text = "⬆ 上移";
        btnMoveUp.UseVisualStyleBackColor = true;

        btnMoveDown.Location = new System.Drawing.Point(btnX, 142);
        btnMoveDown.Name = "btnMoveDown";
        btnMoveDown.Size = new System.Drawing.Size(btnWidth, 36);
        btnMoveDown.TabIndex = 3;
        btnMoveDown.Text = "⬇ 下移";
        btnMoveDown.UseVisualStyleBackColor = true;

        btnRemoveSelected.Location = new System.Drawing.Point(btnX, 194);
        btnRemoveSelected.Name = "btnRemoveSelected";
        btnRemoveSelected.Size = new System.Drawing.Size(btnWidth, 36);
        btnRemoveSelected.TabIndex = 4;
        btnRemoveSelected.Text = "移除選取";
        btnRemoveSelected.UseVisualStyleBackColor = true;

        btnClearAll.Location = new System.Drawing.Point(btnX, 236);
        btnClearAll.Name = "btnClearAll";
        btnClearAll.Size = new System.Drawing.Size(btnWidth, 36);
        btnClearAll.TabIndex = 5;
        btnClearAll.Text = "清空列表";
        btnClearAll.UseVisualStyleBackColor = true;

        btnMerge.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
        btnMerge.Location = new System.Drawing.Point(btnX, 340);
        btnMerge.Name = "btnMerge";
        btnMerge.Size = new System.Drawing.Size(btnWidth, 50);
        btnMerge.TabIndex = 6;
        btnMerge.Text = "合併 PDF";
        btnMerge.UseVisualStyleBackColor = true;
        btnMerge.Enabled = false;

        // === 狀態列 ===
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Text = "就緒，尚無檔案。";
        toolStripStatusLabel.Spring = true;
        toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        toolStripStatusLabelBrand = new ToolStripStatusLabel();
        toolStripStatusLabelBrand.Name = "toolStripStatusLabelBrand";
        toolStripStatusLabelBrand.Text = "帥茂出品";
        toolStripStatusLabelBrand.Alignment = ToolStripItemAlignment.Right;
        statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel, toolStripStatusLabelBrand });
        statusStrip.Location = new System.Drawing.Point(0, 408);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new System.Drawing.Size(694, 22);
        statusStrip.TabIndex = 7;

        // === 主視窗 ===
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(694, 430);
        Controls.Add(panelDragDrop);
        Controls.Add(listBoxFiles);
        Controls.Add(btnMoveUp);
        Controls.Add(btnMoveDown);
        Controls.Add(btnRemoveSelected);
        Controls.Add(btnClearAll);
        Controls.Add(btnMerge);
        Controls.Add(statusStrip);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PDF 合併工具";

        ResumeLayout(false);
        PerformLayout();
    }

    private Panel panelDragDrop;
    private Label lblDragHint;
    private ListBox listBoxFiles;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private Button btnRemoveSelected;
    private Button btnClearAll;
    private Button btnMerge;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel toolStripStatusLabel;
    private ToolStripStatusLabel toolStripStatusLabelBrand;
}
