// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontDrawingPrimitive
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

#nullable disable
namespace QuickFont;

[DebuggerDisplay("Text = {_DisplayTest_dbg}")]
public sealed class QFontDrawingPrimitive
{
  public Matrix4 modelViewMatrix = Matrix4.Identity;

  public QFontDrawingPrimitive(QFont font, QFontRenderOptions options)
  {
    this.Font = font;
    this.Options = options;
  }

  public QFontDrawingPrimitive(QFont font)
  {
    this.Font = font;
    this.Options = new QFontRenderOptions();
  }

  public Vector3 PrintOffset { get; set; }

  public float LineSpacing
  {
    get
    {
      return (float) Math.Ceiling((double) this.Font.FontData.maxGlyphHeight * (double) this.Options.lineSpacing);
    }
  }

  public bool IsMonospacingActive => this.Font.FontData.IsMonospacingActive(this.Options);

  public float MonoSpaceWidth => this.Font.FontData.GetMonoSpaceWidth(this.Options);

  public QFont Font { get; }

  public QFontRenderOptions Options { get; private set; }

  public SizeF LastSize { get; private set; }

  internal IList<QVertex> CurrentVertexRepr { get; } = (IList<QVertex>) new List<QVertex>();

  internal IList<QVertex> ShadowVertexRepr { get; } = (IList<QVertex>) new List<QVertex>();

  private void RenderDropShadow_(
      float x,
      float y,
      char c,
      QFontGlyph nonShadowGlyph,
      QFont shadowFont,
      ref Rectangle clippingRectangle)
  {
    if (shadowFont == null || !this.Options.dropShadowActive)
      return;
    float num1 = (float) ((double) this.Font.FontData.meanGlyphWidth * (double) this.Options.dropShadowOffset.X + (double) nonShadowGlyph.rect.Width * 0.5);
    float num2 = (float) ((double) this.Font.FontData.meanGlyphWidth * (double) this.Options.dropShadowOffset.Y + (double) nonShadowGlyph.rect.Height * 0.5) + (float) nonShadowGlyph.yOffset;
    this.RenderGlyph(x + num1, y + num2, c, shadowFont, this.ShadowVertexRepr, clippingRectangle);
  }

  private bool ScissorsTest_(
      ref float x,
      ref float y,
      ref float width,
      ref float height,
      ref float u1,
      ref float v1,
      ref float u2,
      ref float v2,
      Rectangle clipRectangle)
  {
    float y1 = (float) clipRectangle.Y;
    if ((double) y > (double) y1 + (double) clipRectangle.Height)
    {
      float num1 = height;
      float num2 = Math.Abs(y - (y1 + (float) clipRectangle.Height));
      y = y1 + (float) clipRectangle.Height;
      height -= num2;
      if ((double) height <= 0.0)
        return true;
      float num3 = Math.Abs(num2 / num1);
      v1 += num3 * (v2 - v1);
    }
    if ((double) y - (double) height < (double) y1)
    {
      float num4 = height;
      float num5 = Math.Abs(y1 - (y - height));
      height -= num5;
      if ((double) height <= 0.0)
        return true;
      float num6 = Math.Abs(num5 / num4);
      v2 -= num6 * (v2 - v1);
    }
    if ((double) x < (double) clipRectangle.X)
    {
      float num7 = width;
      float num8 = (float) clipRectangle.X - x;
      x = (float) clipRectangle.X;
      width -= num8;
      if ((double) width <= 0.0)
        return true;
      float num9 = num8 / num7;
      u1 += num9 * (u2 - u1);
    }
    if ((double) x + (double) width > (double) (clipRectangle.X + clipRectangle.Width))
    {
      float num10 = width;
      float num11 = x + width - (float) (clipRectangle.X + clipRectangle.Width);
      width -= num11;
      if ((double) width <= 0.0)
        return true;
      float num12 = num11 / num10;
      u2 -= num12 * (u2 - u1);
    }
    return false;
  }

  internal void RenderGlyph(
      float x,
      float y,
      char c,
      QFont font,
      IList<QVertex> store,
      Rectangle clippingRectangle)
  {
    QFontGlyph nonShadowGlyph = font.FontData.charSetMapping[c];
    if (font.FontData.isDropShadow)
    {
      x -= (float) (int) ((double) nonShadowGlyph.rect.Width * 0.5);
      y -= (float) (int) ((double) nonShadowGlyph.rect.Height * 0.5 + (double) nonShadowGlyph.yOffset);
    }
    else
      this.RenderDropShadow_(x, y, c, nonShadowGlyph, font.FontData.dropShadowFont, ref clippingRectangle);
    y = -y;
    TexturePage page = font.FontData.pages[nonShadowGlyph.page];
    float u1 = (float) nonShadowGlyph.rect.X / (float) page.Width;
    float v1 = (float) nonShadowGlyph.rect.Y / (float) page.Height;
    float u2 = (float) (nonShadowGlyph.rect.X + nonShadowGlyph.rect.Width) / (float) page.Width;
    float v2 = (float) (nonShadowGlyph.rect.Y + nonShadowGlyph.rect.Height) / (float) page.Height;
    float x1 = x + this.PrintOffset.X;
    float y1 = y - (float) nonShadowGlyph.yOffset + this.PrintOffset.Y;
    float width = (float) nonShadowGlyph.rect.Width;
    float height = (float) nonShadowGlyph.rect.Height;
    if (clippingRectangle != new Rectangle() && this.ScissorsTest_(ref x1, ref y1, ref width, ref height, ref u1, ref v1, ref u2, ref v2, clippingRectangle))
      return;
    Vector2 vector21 = new Vector2(u1, v1);
    Vector2 vector22 = new Vector2(u1, v2);
    Vector2 vector23 = new Vector2(u2, v2);
    Vector2 vector24 = new Vector2(u2, v1);
    Vector3 vector31 = new Vector3(x1, y1, this.PrintOffset.Z);
    Vector3 vector32 = new Vector3(x1, y1 - height, this.PrintOffset.Z);
    Vector3 vector33 = new Vector3(x1 + width, y1 - height, this.PrintOffset.Z);
    Vector3 vector34 = new Vector3(x1 + width, y1, this.PrintOffset.Z);
    Vector4 vector4 = Helper.ToVector4(!font.FontData.isDropShadow ? this.Options.colour : this.Options.dropShadowColour);
    IList<QVertex> qvertexList1 = store;
    QVertex qvertex1 = new QVertex();
    qvertex1.Position = vector31;
    qvertex1.TextureCoord = vector21;
    qvertex1.VertexColor = vector4;
    QVertex qvertex2 = qvertex1;
    qvertexList1.Add(qvertex2);
    IList<QVertex> qvertexList2 = store;
    qvertex1 = new QVertex();
    qvertex1.Position = vector32;
    qvertex1.TextureCoord = vector22;
    qvertex1.VertexColor = vector4;
    QVertex qvertex3 = qvertex1;
    qvertexList2.Add(qvertex3);
    IList<QVertex> qvertexList3 = store;
    qvertex1 = new QVertex();
    qvertex1.Position = vector33;
    qvertex1.TextureCoord = vector23;
    qvertex1.VertexColor = vector4;
    QVertex qvertex4 = qvertex1;
    qvertexList3.Add(qvertex4);
    IList<QVertex> qvertexList4 = store;
    qvertex1 = new QVertex();
    qvertex1.Position = vector31;
    qvertex1.TextureCoord = vector21;
    qvertex1.VertexColor = vector4;
    QVertex qvertex5 = qvertex1;
    qvertexList4.Add(qvertex5);
    IList<QVertex> qvertexList5 = store;
    qvertex1 = new QVertex();
    qvertex1.Position = vector33;
    qvertex1.TextureCoord = vector23;
    qvertex1.VertexColor = vector4;
    QVertex qvertex6 = qvertex1;
    qvertexList5.Add(qvertex6);
    IList<QVertex> qvertexList6 = store;
    qvertex1 = new QVertex();
    qvertex1.Position = vector34;
    qvertex1.TextureCoord = vector24;
    qvertex1.VertexColor = vector4;
    QVertex qvertex7 = qvertex1;
    qvertexList6.Add(qvertex7);
  }

  private float MeasureNextlineLength_(string text)
  {
    float num = 0.0f;
    for (int index = 0; index < text.Length; ++index)
    {
      char key = text[index];
      switch (key)
      {
        case '\n':
        case '\r':
          goto label_10;
        default:
          if (this.IsMonospacingActive)
          {
            num += this.MonoSpaceWidth;
            continue;
          }
          if (key == ' ')
          {
            num += (float) Math.Ceiling((double) this.Font.FontData.meanGlyphWidth * (double) this.Options.wordSpacing);
            continue;
          }
          if (this.Font.FontData.charSetMapping.ContainsKey(key))
          {
            QFontGlyph qfontGlyph = this.Font.FontData.charSetMapping[key];
            num += (float) Math.Ceiling((double) qfontGlyph.rect.Width + (double) this.Font.FontData.meanGlyphWidth * (double) this.Options.characterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, text, (TextNode) null));
            continue;
          }
          continue;
      }
    }
    label_10:
    return num;
  }

  private Vector2 TransformPositionToViewport_(Vector2 input)
  {
    Viewport? transformToViewport = this.Options.transformToViewport;
    if (!transformToViewport.HasValue)
      return input;
    Viewport? currentViewport = ViewportHelper.CurrentViewport;
    return new Vector2((float) (((double) input.X - (double) transformToViewport.Value.x) * ((double) currentViewport.GetValueOrDefault().width / (double) transformToViewport.Value.width)), (float) (((double) input.Y - (double) transformToViewport.Value.y) * ((double) currentViewport.GetValueOrDefault().height / (double) transformToViewport.Value.height)));
  }

  private static float TransformWidthToViewport_(float input, QFontRenderOptions options)
  {
    Viewport? transformToViewport = options.transformToViewport;
    if (!transformToViewport.HasValue)
      return input;
    Viewport? currentViewport = ViewportHelper.CurrentViewport;
    return input * (currentViewport.GetValueOrDefault().width / transformToViewport.Value.width);
  }

  private SizeF TransformMeasureFromViewport_(SizeF input)
  {
    Viewport? transformToViewport = this.Options.transformToViewport;
    if (!transformToViewport.HasValue)
      return input;
    Viewport? currentViewport = ViewportHelper.CurrentViewport;
    return new SizeF(input.Width * (transformToViewport.Value.width / currentViewport.GetValueOrDefault().width), input.Height * (transformToViewport.Value.height / currentViewport.GetValueOrDefault().height));
  }

  private Vector2 LockToPixel_(Vector2 input)
  {
    if (!this.Options.lockToPixel)
      return input;
    float lockToPixelRatio = this.Options.lockToPixelRatio;
    return new Vector2((float) ((1.0 - (double) lockToPixelRatio) * (double) input.X + (double) lockToPixelRatio * (double) (int) Math.Round((double) input.X)), (float) ((1.0 - (double) lockToPixelRatio) * (double) input.Y + (double) lockToPixelRatio * (double) (int) Math.Round((double) input.Y)));
  }

  private Vector3 TransformToViewport_(Vector3 input)
  {
    return new Vector3(this.LockToPixel_(this.TransformPositionToViewport_(input.Xy)))
    {
        Z = input.Z
    };
  }

  public SizeF Print(
      string text,
      Vector3 position,
      QFontAlignment alignment,
      Rectangle clippingRectangle = default (Rectangle))
  {
    this.PrintOffset = this.TransformToViewport_(position);
    return this.PrintOrMeasure_(text, alignment, false, clippingRectangle);
  }

  public SizeF Print(
      string text,
      Vector3 position,
      QFontAlignment alignment,
      Color color,
      Rectangle clippingRectangle = default (Rectangle))
  {
    this.Options.colour = color;
    this.PrintOffset = this.TransformToViewport_(position);
    return this.PrintOrMeasure_(text, alignment, false, clippingRectangle);
  }

  public SizeF Print(
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      Rectangle clippingRectangle = default (Rectangle))
  {
    return this.Print(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment), this.TransformToViewport_(position), clippingRectangle);
  }

  public SizeF Print(
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      Color colour,
      Rectangle clippingRectangle = default (Rectangle))
  {
    return this.Print(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment), this.TransformToViewport_(position), colour, clippingRectangle);
  }

  public SizeF Print(ProcessedText processedText, Vector3 position, Rectangle clippingRectangle = default (Rectangle))
  {
    this.PrintOffset = this.TransformToViewport_(position);
    return this.PrintOrMeasure_(processedText, false, clippingRectangle);
  }

  public SizeF Print(
      ProcessedText processedText,
      Vector3 position,
      Color colour,
      Rectangle clippingRectangle = default (Rectangle))
  {
    this.Options.colour = colour;
    this.PrintOffset = this.TransformToViewport_(position);
    return this.PrintOrMeasure_(processedText, false, clippingRectangle);
  }

  public SizeF Measure(string text, QFontAlignment alignment = QFontAlignment.LEFT)
  {
    return this.TransformMeasureFromViewport_(this.PrintOrMeasure_(text, alignment, true));
  }

  public SizeF Measure(string text, float maxWidth, QFontAlignment alignment)
  {
    return this.Measure(text, new SizeF(maxWidth, -1f), alignment);
  }

  public SizeF Measure(string text, SizeF maxSize, QFontAlignment alignment)
  {
    return this.Measure(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment));
  }

  public SizeF Measure(ProcessedText processedText)
  {
    return this.TransformMeasureFromViewport_(this.PrintOrMeasure_(processedText, true));
  }

  private SizeF PrintOrMeasure_(
      string text,
      QFontAlignment alignment,
      bool measureOnly,
      Rectangle clippingRectangle = default (Rectangle))
  {
    float width = 0.0f;
    float num = 0.0f;
    float y = 0.0f;
    float val21 = float.MinValue;
    float val22 = float.MaxValue;
    text = text.Replace("\r\n", "\r");
    switch (alignment)
    {
      case QFontAlignment.RIGHT:
        num -= this.MeasureNextlineLength_(text);
        break;
      case QFontAlignment.CENTRE:
        num -= (float) (int) (0.5 * (double) this.MeasureNextlineLength_(text));
        break;
    }
    float val1 = 0.0f;
    for (int index = 0; index < text.Length; ++index)
    {
      char ch = text[index];
      switch (ch)
      {
        case '\n':
        case '\r':
          y += this.LineSpacing;
          val1 = 0.0f;
          num = 0.0f;
          switch (alignment)
          {
            case QFontAlignment.RIGHT:
              num -= this.MeasureNextlineLength_(text[(index + 1)..]);
              continue;
            case QFontAlignment.CENTRE:
              num -= (float) (int) (0.5 * (double) this.MeasureNextlineLength_(text[(index + 1)..]));
              continue;
            default:
              continue;
          }
        default:
          val22 = Math.Min(num, val22);
          if (ch != ' ' && this.Font.FontData.charSetMapping.ContainsKey(ch) && !measureOnly)
            this.RenderGlyph(num, y, ch, this.Font, this.CurrentVertexRepr, clippingRectangle);
          if (this.IsMonospacingActive)
            num += this.MonoSpaceWidth;
          else if (ch == ' ')
            num += (float) Math.Ceiling((double) this.Font.FontData.meanGlyphWidth * (double) this.Options.wordSpacing);
          else if (this.Font.FontData.charSetMapping.ContainsKey(ch))
          {
            QFontGlyph qfontGlyph = this.Font.FontData.charSetMapping[ch];
            num += (float) Math.Ceiling((double) qfontGlyph.rect.Width + (double) this.Font.FontData.meanGlyphWidth * (double) this.Options.characterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, text, (TextNode) null));
            val1 = Math.Max(val1, (float) (qfontGlyph.rect.Height + qfontGlyph.yOffset));
          }
          val21 = Math.Max(num, val21);
          break;
      }
    }
    if ((double) Math.Abs(val22 - float.MaxValue) > 1.4012984643248171E-45)
      width = val21 - val22;
    this.LastSize = new SizeF(width, y + this.LineSpacing);
    return this.LastSize;
  }

  private SizeF PrintOrMeasure_(
      ProcessedText processedText,
      bool measureOnly,
      Rectangle clippingRectangle = default (Rectangle))
  {
    float num1 = 0.0f;
    float val1 = 0.0f;
    float num2 = 0.0f;
    float y = 0.0f;
    float width = processedText.maxSize_.Width;
    QFontAlignment alignment = processedText.alignment_;
    TextNodeList textNodeList = processedText.textNodeList_;
    for (TextNode textNode = textNodeList.head; textNode != null; textNode = textNode.next)
      textNode.lengthTweak = 0.0f;
    switch (alignment)
    {
      case QFontAlignment.RIGHT:
        num2 -= (float) Math.Ceiling((double) this.TextNodeLineLength_(textNodeList.head, width) - (double) width);
        break;
      case QFontAlignment.CENTRE:
        num2 -= (float) Math.Ceiling(0.5 * (double) this.TextNodeLineLength_(textNodeList.head, width));
        break;
      case QFontAlignment.JUSTIFY:
        this.JustifyLine_(textNodeList.head, width);
        break;
    }
    bool flag1 = false;
    float num3 = 0.0f;
    for (TextNode node = textNodeList.head; node != null; node = node.next)
    {
      bool flag2 = false;
      if (node.type == TextNodeType.LINE_BREAK)
        flag2 = true;
      else if (((!this.Options.wordWrap ? 0 : (this.SkipTrailingSpace_(node, num3, width) ? 1 : 0)) & (flag1 ? 1 : 0)) != 0)
        flag2 = true;
      else if ((double) num3 + (double) node.ModifiedLength <= (double) width || !flag1)
      {
        flag1 = true;
        if (!measureOnly)
          this.RenderWord_(num2 + num3, y, node, ref clippingRectangle);
        num3 += node.ModifiedLength;
        val1 = Math.Max(val1, node.height);
        num1 = Math.Max(num3, num1);
      }
      else if (this.Options.wordWrap)
      {
        flag2 = true;
        if (node.previous != null)
          node = node.previous;
      }
      else
        continue;
      if (flag2)
      {
        if ((double) processedText.maxSize_.Height <= 0.0 || (double) y + (double) this.LineSpacing - 0.0 < (double) processedText.maxSize_.Height)
        {
          y += this.LineSpacing;
          num2 = 0.0f;
          num3 = 0.0f;
          flag1 = false;
          if (node.next != null)
          {
            switch (alignment)
            {
              case QFontAlignment.RIGHT:
                num2 -= (float) Math.Ceiling((double) this.TextNodeLineLength_(node.next, width) - (double) width);
                continue;
              case QFontAlignment.CENTRE:
                num2 -= (float) Math.Ceiling(0.5 * (double) this.TextNodeLineLength_(node.next, width));
                continue;
              case QFontAlignment.JUSTIFY:
                this.JustifyLine_(node.next, width);
                continue;
              default:
                continue;
            }
          }
        }
        else
          break;
      }
    }
    this.LastSize = new SizeF(num1, (float) ((double) y + (double) this.LineSpacing - 0.0));
    return this.LastSize;
  }

  private void RenderWord_(float x, float y, TextNode node, ref Rectangle clippingRectangle)
  {
    if (node.type != TextNodeType.WORD)
      return;
    int num1 = node.text.Length - 1;
    if (this.CrumbledWord_(node))
      ++num1;
    int num2 = 0;
    int num3 = 0;
    if (num1 != 0)
    {
      num2 = (int) node.lengthTweak / num1;
      num3 = (int) node.lengthTweak - num2 * num1;
    }
    for (int index = 0; index < node.text.Length; ++index)
    {
      char ch = node.text[index];
      if (this.Font.FontData.charSetMapping.ContainsKey(ch))
      {
        QFontGlyph qfontGlyph = this.Font.FontData.charSetMapping[ch];
        this.RenderGlyph(x, y, ch, this.Font, this.CurrentVertexRepr, clippingRectangle);
        if (this.IsMonospacingActive)
          x += this.MonoSpaceWidth;
        else
          x += (float) (int) Math.Ceiling((double) qfontGlyph.rect.Width + (double) this.Font.FontData.meanGlyphWidth * (double) this.Options.characterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, node.text, node));
        x += (float) num2;
        if (num3 > 0)
        {
          ++x;
          --num3;
        }
        else if (num3 < 0)
        {
          --x;
          ++num3;
        }
      }
    }
  }

  private float TextNodeLineLength_(TextNode node, float maxLength)
  {
    if (node == null)
      return 0.0f;
    bool flag = false;
    float lengthSoFar;
    for (lengthSoFar = 0.0f; node != null && node.type != TextNodeType.LINE_BREAK && !(this.SkipTrailingSpace_(node, lengthSoFar, maxLength) & flag) && ((double) lengthSoFar + (double) node.length <= (double) maxLength || !flag); node = node.next)
    {
      flag = true;
      lengthSoFar += node.length;
    }
    return lengthSoFar;
  }

  private bool CrumbledWord_(TextNode node)
  {
    return node.type == TextNodeType.WORD && node.next != null && node.next.type == TextNodeType.WORD;
  }

  private void JustifyLine_(TextNode node, float targetLength)
  {
    bool flag1 = false;
    if (node == null)
      return;
    TextNode textNode1 = node;
    int num1 = 0;
    int num2 = 0;
    bool flag2 = false;
    float lengthSoFar = 0.0f;
    TextNode textNode2 = node;
    for (; node != null && node.type != TextNodeType.LINE_BREAK; node = node.next)
    {
      if (this.SkipTrailingSpace_(node, lengthSoFar, targetLength) & flag2)
      {
        flag1 = true;
        break;
      }
      if ((double) lengthSoFar + (double) node.length < (double) targetLength || !flag2)
      {
        textNode2 = node;
        if (node.type == TextNodeType.SPACE)
          ++num2;
        if (node.type == TextNodeType.WORD)
        {
          num1 += node.text.Length - 1;
          if (this.CrumbledWord_(node))
            ++num1;
        }
        flag2 = true;
        lengthSoFar += node.length;
      }
      else
      {
        flag1 = true;
        break;
      }
    }
    float num3 = 0.0f;
    int num4 = 0;
    int num5 = 0;
    bool flag3 = false;
    TextNode textNode3 = (TextNode) null;
    for (node = textNode2.next; node != null && node.type != TextNodeType.LINE_BREAK; node = node.next)
    {
      if (node.type == TextNodeType.SPACE)
      {
        num3 += node.length;
        ++num4;
      }
      else if (node.type == TextNodeType.WORD)
      {
        textNode3 = node;
        flag3 = true;
        num3 += node.length;
        num5 += node.text.Length - 1;
        break;
      }
    }
    if (!flag1)
      return;
    bool flag4 = flag3 && ((double) num3 + (double) lengthSoFar - (double) targetLength) * (double) this.Options.justifyContractionPenalty < (double) targetLength - (double) lengthSoFar && ((double) targetLength - ((double) lengthSoFar + (double) num3 + 1.0)) / (double) targetLength > -(double) this.Options.justifyCapContract;
    if ((flag4 || (double) lengthSoFar >= (double) targetLength) && (!flag4 || (double) lengthSoFar + (double) num3 <= (double) targetLength))
      return;
    if (flag4)
    {
      lengthSoFar += num3 + 1f;
      num1 += num5;
      num2 += num4;
    }
    int num6 = (int) ((double) targetLength - (double) lengthSoFar);
    int num7 = 0;
    int num8 = 0;
    if (flag4)
    {
      if ((double) num6 / (double) targetLength < -(double) this.Options.justifyCapContract)
        num6 = (int) (-(double) this.Options.justifyCapContract * (double) targetLength);
    }
    else if ((double) num6 / (double) targetLength > (double) this.Options.justifyCapExpand)
      num6 = (int) ((double) this.Options.justifyCapExpand * (double) targetLength);
    if (num1 == 0)
      num7 = num6;
    else if (num2 == 0)
    {
      num8 = num6;
    }
    else
    {
      num8 = !flag4 ? (int) ((double) num6 * (double) this.Options.JustifyCharacterWeightForExpand * (double) num1 / (double) num2) : (int) ((double) num6 * (double) this.Options.JustifyCharacterWeightForContract * (double) num1 / (double) num2);
      if (!flag4 && num8 > num6 || flag4 && num8 < num6)
        num8 = num6;
      num7 = num6 - num8;
    }
    int num9 = 0;
    int num10 = 0;
    if (num1 != 0)
    {
      num9 = num8 / num1;
      num10 = num8 - num9 * num1;
    }
    int num11 = 0;
    int num12 = 0;
    if (num2 != 0)
    {
      num11 = num7 / num2;
      num12 = num7 - num11 * num2;
    }
    for (node = textNode1; node != null; node = node.next)
    {
      if (node.type == TextNodeType.SPACE)
      {
        node.lengthTweak = (float) num11;
        if (num12 > 0)
        {
          ++node.lengthTweak;
          --num12;
        }
        else if (num12 < 0)
        {
          --node.lengthTweak;
          ++num12;
        }
      }
      else if (node.type == TextNodeType.WORD)
      {
        int num13 = node.text.Length - 1;
        if (this.CrumbledWord_(node))
          ++num13;
        node.lengthTweak = (float) (num13 * num9);
        if (num10 >= num13)
        {
          node.lengthTweak += (float) num13;
          num10 -= num13;
        }
        else if (num10 <= -num13)
        {
          node.lengthTweak -= (float) num13;
          num10 += num13;
        }
        else
        {
          node.lengthTweak += (float) num10;
          num10 = 0;
        }
      }
      if (!flag4 && node == textNode2 || flag4 && node == textNode3)
        break;
    }
  }

  private bool SkipTrailingSpace_(TextNode node, float lengthSoFar, float boundWidth)
  {
    return node.type == TextNodeType.SPACE && node.next != null && node.next.type == TextNodeType.WORD && (double) node.ModifiedLength + (double) node.next.ModifiedLength + (double) lengthSoFar > (double) boundWidth;
  }

  public static ProcessedText ProcessText(
      QFont font,
      QFontRenderOptions options,
      string text,
      SizeF maxSize,
      QFontAlignment alignment)
  {
    maxSize.Width = QFontDrawingPrimitive.TransformWidthToViewport_(maxSize.Width, options);
    TextNodeList textNodeList1 = new TextNodeList(text);
    textNodeList1.MeasureNodes(font.FontData, options);
    List<TextNode> textNodeList2 = [];
    foreach (TextNode textNode in textNodeList1)
    {
      if ((!options.wordWrap || (double) textNode.length >= (double) maxSize.Width) && textNode.type == TextNodeType.WORD)
        textNodeList2.Add(textNode);
    }
    foreach (TextNode node in textNodeList2)
      textNodeList1.Crumble(node, 1);
    textNodeList1.MeasureNodes(font.FontData, options);
    return new ProcessedText()
    {
        textNodeList_ = textNodeList1,
        maxSize_ = maxSize,
        alignment_ = alignment
    };
  }
}