// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontRenderOptions
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Mathematics;
using System.Drawing;

#nullable disable
namespace QuickFont;

public sealed class QFontRenderOptions
{
  public Color colour = Color.White;
  public float characterSpacing = 0.05f;
  public float wordSpacing = 0.9f;
  public float lineSpacing = 1f;
  public bool dropShadowActive;
  public Vector2 dropShadowOffset = new Vector2(0.16f, 0.16f);
  public Color dropShadowColour = Color.FromArgb(128, Color.Black);
  public QFontMonospacing monospacing;
  public Viewport? transformToViewport;
  public bool lockToPixel;
  public float lockToPixelRatio = 1f;
  public bool useDefaultBlendFunction = true;
  public bool wordWrap = true;
  private float justifyCharWeightForExpand_ = 0.5f;
  private float justifyCharWeightForContract_ = 0.2f;
  public float justifyCapExpand = 0.5f;
  public float justifyCapContract = 0.1f;
  public float justifyContractionPenalty = 2f;
  public Rectangle clippingRectangle;

  public float DropShadowOpacity
  {
    set
    {
      this.dropShadowColour = Color.FromArgb((int) (byte) ((double) value * (double) byte.MaxValue), Color.Black);
    }
  }

  public float JustifyCharacterWeightForExpand
  {
    get => this.justifyCharWeightForExpand_;
    set => this.justifyCharWeightForExpand_ = MathHelper.Clamp(value, 0.0f, 1f);
  }

  public float JustifyCharacterWeightForContract
  {
    get => this.justifyCharWeightForContract_;
    set => this.justifyCharWeightForContract_ = MathHelper.Clamp(value, 0.0f, 1f);
  }

  public QFontRenderOptions CreateClone()
  {
    return new QFontRenderOptions()
    {
        colour = this.colour,
        characterSpacing = this.characterSpacing,
        wordSpacing = this.wordSpacing,
        lineSpacing = this.lineSpacing,
        dropShadowActive = this.dropShadowActive,
        dropShadowOffset = this.dropShadowOffset,
        dropShadowColour = this.dropShadowColour,
        monospacing = this.monospacing,
        transformToViewport = this.transformToViewport,
        lockToPixel = this.lockToPixel,
        lockToPixelRatio = this.lockToPixelRatio,
        useDefaultBlendFunction = this.useDefaultBlendFunction,
        JustifyCharacterWeightForExpand = this.JustifyCharacterWeightForExpand,
        justifyCharWeightForExpand_ = this.justifyCharWeightForExpand_,
        JustifyCharacterWeightForContract = this.JustifyCharacterWeightForContract,
        justifyCharWeightForContract_ = this.justifyCharWeightForContract_,
        justifyCapExpand = this.justifyCapExpand,
        justifyCapContract = this.justifyCapContract,
        justifyContractionPenalty = this.justifyContractionPenalty,
        clippingRectangle = this.clippingRectangle
    };
  }
}