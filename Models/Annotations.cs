using System.Drawing;

namespace MergePdf.Models;

/// <summary>
/// 標註基底類別。
/// </summary>
public abstract class BaseAnnotation
{
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>
    /// 定界框（相對於圖形的像素座標，非控制項座標）。
    /// </summary>
    public RectangleF Bounds { get; set; }
    public Color Color { get; set; } = Color.Red;
    public float Thickness { get; set; } = 3f;
    public bool IsSelected { get; set; }

    /// <summary>
    /// 繪製此標註至指定的 Graphics。
    /// </summary>
    public abstract void Draw(Graphics g, float scale);

    /// <summary>
    /// 檢查給定座標是否命中此標註。
    /// </summary>
    public virtual bool HitTest(PointF point, float hitTolerance = 5f)
    {
        var expandedBounds = new RectangleF(
            Bounds.X - hitTolerance,
            Bounds.Y - hitTolerance,
            Bounds.Width + hitTolerance * 2,
            Bounds.Height + hitTolerance * 2);
        return expandedBounds.Contains(point);
    }
}

public class TextAnnotation : BaseAnnotation
{
    public string Text { get; set; } = "輸入文字";
    public string FontName { get; set; } = "Arial";
    public float FontSize { get; set; } = 16f;

    public override void Draw(Graphics g, float scale)
    {
        using var font = new Font(FontName, FontSize * scale);
        using var brush = new SolidBrush(Color);
        var scaledRect = new RectangleF(Bounds.X * scale, Bounds.Y * scale, Bounds.Width * scale, Bounds.Height * scale);
        g.DrawString(Text, font, brush, scaledRect);
    }
}

public class LineAnnotation : BaseAnnotation
{
    public PointF StartPoint { get; set; }
    public PointF EndPoint { get; set; }

    public override void Draw(Graphics g, float scale)
    {
        using var pen = new Pen(Color, Thickness * scale);
        g.DrawLine(pen,
            StartPoint.X * scale, StartPoint.Y * scale,
            EndPoint.X * scale, EndPoint.Y * scale);
    }
}

public class RectAnnotation : BaseAnnotation
{
    public override void Draw(Graphics g, float scale)
    {
        using var pen = new Pen(Color, Thickness * scale);
        g.DrawRectangle(pen,
            Bounds.X * scale, Bounds.Y * scale,
            Bounds.Width * scale, Bounds.Height * scale);
    }
}

public class EllipseAnnotation : BaseAnnotation
{
    public override void Draw(Graphics g, float scale)
    {
        using var pen = new Pen(Color, Thickness * scale);
        g.DrawEllipse(pen,
            Bounds.X * scale, Bounds.Y * scale,
            Bounds.Width * scale, Bounds.Height * scale);
    }
}

public class ArrowAnnotation : LineAnnotation
{
    public override void Draw(Graphics g, float scale)
    {
        using var pen = new Pen(Color, Thickness * scale);
        float sX = StartPoint.X * scale;
        float sY = StartPoint.Y * scale;
        float eX = EndPoint.X * scale;
        float eY = EndPoint.Y * scale;

        g.DrawLine(pen, sX, sY, eX, eY);

        // 繪製箭頭頭部
        float angle = (float)Math.Atan2(eY - sY, eX - sX);
        float arrowSize = 15f * scale;
        float arrowAngle = (float)(Math.PI / 6); // 30 度角

        PointF p1 = new PointF(
            eX - arrowSize * (float)Math.Cos(angle - arrowAngle),
            eY - arrowSize * (float)Math.Sin(angle - arrowAngle));
        PointF p2 = new PointF(
            eX - arrowSize * (float)Math.Cos(angle + arrowAngle),
            eY - arrowSize * (float)Math.Sin(angle + arrowAngle));

        g.DrawLine(pen, eX, eY, p1.X, p1.Y);
        g.DrawLine(pen, eX, eY, p2.X, p2.Y);
    }
}
