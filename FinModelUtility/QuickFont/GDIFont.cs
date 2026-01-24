// Decompiled with JetBrains decompiler
// Type: QuickFont.GDIFont
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

#nullable disable
namespace QuickFont;

public sealed class GdiFont : IFont, IDisposable
{
  private Font font_;
  private FontFamily fontFamily_;

  public float Size => this.font_.Size;

  public bool HasKerningInformation => false;

  public GdiFont(
      string fontPath,
      float size,
      FontStyle style,
      int superSampleLevels = 1,
      float scale = 1f)
  {
    try
    {
      PrivateFontCollection privateFontCollection = new PrivateFontCollection();
      privateFontCollection.AddFontFile(fontPath);
      this.fontFamily_ = privateFontCollection.Families[0];
    }
    catch (FileNotFoundException ex)
    {
      this.fontFamily_ = ((IEnumerable<FontFamily>) new InstalledFontCollection().Families).FirstOrDefault<FontFamily>((Func<FontFamily, bool>) (family => string.Equals(fontPath, family.Name)));
      if (this.fontFamily_ == null)
        this.fontFamily_ = SystemFonts.DefaultFont.FontFamily;
    }
    if (!this.fontFamily_.IsStyleAvailable(style))
      throw new ArgumentException("Font file: " + fontPath + " does not support style: " + style.ToString());
    this.font_ = new Font(this.fontFamily_, size * scale * (float) superSampleLevels, style);
  }

  public Point DrawString(string s, Graphics graph, Brush color, int x, int y)
  {
    graph.DrawString(s, this.font_, color, (float) x, (float) y);
    return Point.Empty;
  }

  public int GetKerning(char c1, char c2)
  {
    throw new NotImplementedException("Font kerning for GDI Fonts is not implemented. Should be calculated manually.");
  }

  public SizeF MeasureString(string s, Graphics graph) => graph.MeasureString(s, this.font_);

  public override string ToString() => this.font_.Name;

  public void Dispose()
  {
    this.font_.Dispose();
    this.fontFamily_.Dispose();
  }
}