using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinProxyEnvSync.utils;

public class DarkColorTable : ProfessionalColorTable
{
  private readonly Color _backColor = Color.FromArgb(43, 43, 43);
  private readonly Color _borderColor = Color.FromArgb(60, 60, 60);

  public override Color ToolStripDropDownBackground
  {
    get => _backColor;
  }
  public override Color MenuStripGradientBegin
  {
    get => _backColor;
  }
  public override Color MenuStripGradientEnd
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientBegin
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientMiddle
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientEnd
  {
    get => _backColor;
  }
  public override Color MenuItemSelected { get; } = Color.FromArgb(70, 70, 70);
  public override Color MenuItemBorder
  {
    get => Color.Transparent;
  }
  public override Color SeparatorDark
  {
    get => _borderColor;
  }
  public override Color SeparatorLight
  {
    get => _borderColor;
  }
  public override Color MenuBorder
  {
    get => _borderColor;
  }
}

public class LightColorTable : ProfessionalColorTable
{
  private readonly Color _backColor = Color.FromArgb(243, 243, 243);
  private readonly Color _borderColor = Color.FromArgb(218, 218, 218);

  public override Color ToolStripDropDownBackground
  {
    get => _backColor;
  }
  public override Color MenuStripGradientBegin
  {
    get => _backColor;
  }
  public override Color MenuStripGradientEnd
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientBegin
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientMiddle
  {
    get => _backColor;
  }
  public override Color ImageMarginGradientEnd
  {
    get => _backColor;
  }
  public override Color MenuItemSelected { get; } = Color.FromArgb(226, 226, 226);
  public override Color MenuItemBorder
  {
    get => Color.Transparent;
  }
  public override Color SeparatorDark
  {
    get => _borderColor;
  }
  public override Color SeparatorLight
  {
    get => _borderColor;
  }
  public override Color MenuBorder
  {
    get => _borderColor;
  }
}

public class MenuRenderer : ToolStripProfessionalRenderer
{
  private readonly Color _textColor;

  public MenuRenderer(ProfessionalColorTable colorTable, Color textColor) : base(colorTable)
  {
    _textColor = textColor;
  }

  protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
  {
    e.TextColor = _textColor;
    base.OnRenderItemText(e);
  }

  protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
  {
    if (!e.Item.Selected)
    {
      base.OnRenderMenuItemBackground(e);
      return;
    }

    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.HighQuality;

    g.FillPath(new SolidBrush(ColorTable.MenuItemSelected), GetRoundedRect(new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2), 3));
  }

  private static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
  {
    var path = new GraphicsPath();
    path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
    path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
    path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
    path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
    path.CloseFigure();
    return path;
  }
}