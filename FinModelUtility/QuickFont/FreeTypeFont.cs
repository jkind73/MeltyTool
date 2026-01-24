// Decompiled with JetBrains decompiler
// Type: QuickFont.FreeTypeFont
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Drawing;
using System.IO;

using fin.io;

using Melville.SharpFont;

using SharpFont.Gdi;

#nullable disable
namespace QuickFont;

public class FreeTypeFont : IFont, IDisposable
{
  private static Library fontLibrary_;
  private const uint DPI_ = 96;
  private Face fontFace_;
  private int maxHorizontalBearyingY_;
  private bool disposedValue_;

  public float Size { get; private set; }

  public bool HasKerningInformation => this.fontFace_.HasKerning;

  public static void Init(IReadOnlyTreeDirectory dllDirectory) {
    fontLibrary_ = new Library(dllDirectory.FullPath);
  }

  public FreeTypeFont(
      string fontPath,
      float size,
      FontStyle style,
      int superSampleLevels = 1,
      float scale = 1f) {
    if (!File.Exists(fontPath))
      throw new ArgumentException("The specified font path does not exist", nameof (fontPath));
    StyleFlags fontStyle = (StyleFlags) 0;
    switch (style)
    {
      case FontStyle.Regular:
        fontStyle = (StyleFlags) 0;
        break;
      case FontStyle.Bold:
        fontStyle = (StyleFlags) 2;
        break;
      case FontStyle.Italic:
        fontStyle = (StyleFlags) 1;
        break;
    }
    this.LoadFontFace_(fontPath, size, fontStyle, superSampleLevels, scale);
  }

  public FreeTypeFont(
      byte[] fontData,
      float size,
      FontStyle style,
      int superSampleLevels = 1,
      float scale = 1f)
  {
    StyleFlags fontStyle = (StyleFlags) 0;
    switch (style)
    {
      case FontStyle.Regular:
        fontStyle = (StyleFlags) 0;
        break;
      case FontStyle.Bold:
        fontStyle = (StyleFlags) 2;
        break;
      case FontStyle.Italic:
        fontStyle = (StyleFlags) 1;
        break;
    }
    this.LoadFontFace_(fontData, size, fontStyle, superSampleLevels, scale);
  }

  private void LoadFontFace_(
      string fontPath,
      float size,
      StyleFlags fontStyle,
      int superSampleLevels,
      float scale)
  {
    Face face1 = fontLibrary_.NewFace(fontPath, -1);
    int faceCount = face1.FaceCount;
    face1.Dispose();
    Face face2 = (Face) null;
    for (int index = 0; index < faceCount; ++index)
    {
      face2 = fontLibrary_.NewFace(fontPath, index);
      if (face2.StyleFlags != fontStyle)
      {
        face2.Dispose();
        face2 = (Face) null;
      }
      else
        break;
    }
    if (face2 == null)
      face2 = fontLibrary_.NewFace(fontPath, 0);
    this.fontFace_ = face2;
    this.Size = size * scale * (float) superSampleLevels;
    this.fontFace_.SetCharSize(0, this.Size, 0U, 96U);
  }

  private void LoadFontFace_(
      byte[] fontData,
      float size,
      StyleFlags fontStyle,
      int superSampleLevels,
      float scale)
  {
    Face face1 = fontLibrary_.NewMemoryFace(fontData, -1);
    int faceCount = face1.FaceCount;
    face1.Dispose();
    Face face2 = (Face) null;
    for (int index = 0; index < faceCount; ++index)
    {
      face2 = fontLibrary_.NewMemoryFace(fontData, index);
      if (face2.StyleFlags != fontStyle)
      {
        face2.Dispose();
        face2 = (Face) null;
      }
      else
        break;
    }
    if (face2 == null)
      face2 = fontLibrary_.NewMemoryFace(fontData, 0);
    this.fontFace_ = face2;
    this.Size = size * scale * (float) superSampleLevels;
    this.fontFace_.SetCharSize(0, this.Size, 0U, 96U);
  }

  public override string ToString() => this.fontFace_.FamilyName ?? "";

  public Point DrawString(string s, Graphics graph, Brush color, int x, int y)
  {
    if (s.Length > 1)
      throw new ArgumentOutOfRangeException(nameof (s), "Implementation currently only supports drawing individual characters");
    Color color1 = color is SolidBrush ? ((SolidBrush) color).Color : throw new ArgumentException("Brush is required to be a SolidBrush (single, solid color)", nameof (color));
    this.LoadGlyph_(s[0]);
    this.fontFace_.Glyph.RenderGlyph((RenderMode) 0);
    if (this.fontFace_.Glyph.Bitmap.Width <= 0)
      return Point.Empty;
    Bitmap gdipBitmap = FtBitmapExtensions.ToGdipBitmap(this.fontFace_.Glyph.Bitmap, color1);
    int num1 = y + this.maxHorizontalBearyingY_;
    Graphics graphics = graph;
    Bitmap bitmap = gdipBitmap;
    int x1 = x;
    int num2 = num1;
    Fixed26Dot6 horizontalBearingY1 = this.fontFace_.Glyph.Metrics.HorizontalBearingY;
    int num3 = (int) Math.Ceiling((double) horizontalBearingY1);
    int y1 = num2 - num3;
    graphics.DrawImageUnscaled((Image) bitmap, x1, y1);
    int num4 = num1;
    Fixed26Dot6 horizontalBearingY2 = this.fontFace_.Glyph.Metrics.HorizontalBearingY;
    int num5 = (int) Math.Ceiling((double) horizontalBearingY2);
    return new Point(0, num4 - num5 - 2 * y);
  }

  public int GetKerning(char c1, char c2)
  {
    FTVector26Dot6 kerning = this.fontFace_.GetKerning(this.fontFace_.GetCharIndex((uint) c1), this.fontFace_.GetCharIndex((uint) c2), (KerningMode) 0);
    return (int) Math.Ceiling((double) kerning.X);
  }

  private void LoadGlyph_(char c)
  {
    this.fontFace_.LoadGlyph(this.fontFace_.GetCharIndex((uint) c), (LoadFlags) 0, (LoadTarget) 0);
  }

  public SizeF MeasureString(string s, Graphics graph)
  {
    if (s.Length > 1)
      throw new ArgumentOutOfRangeException(nameof (s), "Implementation currently only supports drawing individual characters");
    this.LoadGlyph_(s[0]);
    GlyphMetrics metrics = this.fontFace_.Glyph.Metrics;
    int num = (int) metrics.HorizontalBearingY;
    if (num > this.maxHorizontalBearyingY_)
      this.maxHorizontalBearyingY_ = num;
    return new SizeF((float) metrics.Width, (float) metrics.Height);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (this.disposedValue_)
      return;
    if (disposing)
    {
      if (this.fontFace_ != null)
      {
        this.fontFace_.Dispose();
        this.fontFace_ = (Face) null;
      }
    }
    this.disposedValue_ = true;
  }

  public void Dispose() => this.Dispose(true);
}