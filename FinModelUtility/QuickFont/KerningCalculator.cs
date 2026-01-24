// Decompiled with JetBrains decompiler
// Type: QuickFont.KerningCalculator
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;

#nullable disable
namespace QuickFont;

internal static class KerningCalculator
{
  private static int Kerning_(
      QFontGlyph g1,
      QFontGlyph g2,
      KerningCalculator.XLimits[] lim1,
      KerningCalculator.XLimits[] lim2,
      QFontKerningConfiguration config,
      IFont font)
  {
    if (font != null && font.HasKerningInformation)
      return font.GetKerning(g1.character, g2.character);
    int yoffset1 = g1.yOffset;
    int yoffset2 = g2.yOffset;
    int num1 = Math.Max(yoffset1, yoffset2);
    int num2 = Math.Min(g1.rect.Height + yoffset1, g2.rect.Height + yoffset2);
    int width = g1.rect.Width;
    int val1 = width;
    for (int index = num1; index < num2; ++index)
      val1 = Math.Min(val1, width - lim1[index - yoffset1].max + lim2[index - yoffset2].min);
    int val2 = Math.Min(Math.Min(val1, g1.rect.Width), g2.rect.Width);
    switch (config.GetOverridingCharacterKerningRuleForPair(g1.character.ToString() + g2.character.ToString()))
    {
      case CharacterKerningRule.ZERO:
        return 1;
      case CharacterKerningRule.NOT_MORE_THAN_HALF:
        return 1 - (int) Math.Min((float) Math.Min(g1.rect.Width, g2.rect.Width) * 0.5f, (float) val2);
      default:
        return 1 - val2;
    }
  }

  public static Dictionary<string, int> CalculateKerning(
      char[] charSet,
      QFontGlyph[] glyphs,
      List<QBitmap> bitmapPages,
      QFontKerningConfiguration config,
      IFont font = null)
  {
    Dictionary<string, int> kerning = new Dictionary<string, int>();
    KerningCalculator.XLimits[][] xlimitsArray1 = new KerningCalculator.XLimits[charSet.Length][];
    int val2 = 0;
    for (int index = 0; index < charSet.Length; ++index)
    {
      Rectangle rect = glyphs[index].rect;
      QBitmap bitmapPage = bitmapPages[glyphs[index].page];
      xlimitsArray1[index] = new KerningCalculator.XLimits[rect.Height + 1];
      val2 = Math.Max(rect.Height, val2);
      int y = rect.Y;
      int num1 = rect.Y + rect.Height;
      int x = rect.X;
      int num2 = rect.X + rect.Width;
      for (int py = y; py <= num1; ++py)
      {
        int num3 = x;
        bool flag = true;
        for (int px = x; px <= num2; ++px)
        {
          if (!QBitmap.EmptyAlphaPixel(bitmapPage.bitmapData, px, py, config.alphaEmptyPixelTolerance))
          {
            if (flag)
            {
              xlimitsArray1[index][py - y].min = px - x;
              flag = false;
            }
            num3 = px;
          }
        }
        xlimitsArray1[index][py - y].max = num3 - x;
        if (flag)
          xlimitsArray1[index][py - y].min = num2 - 1;
      }
    }
    KerningCalculator.XLimits[] xlimitsArray2 = new KerningCalculator.XLimits[val2 + 1];
    for (int index1 = 0; index1 < charSet.Length; ++index1)
    {
      for (int index2 = 0; index2 < xlimitsArray1[index1].Length; ++index2)
        xlimitsArray2[index2] = xlimitsArray1[index1][index2];
      for (int index3 = 0; index3 < xlimitsArray1[index1].Length; ++index3)
      {
        if (index3 != 0)
        {
          xlimitsArray2[index3].min = Math.Min(xlimitsArray1[index1][index3 - 1].min, xlimitsArray2[index3].min);
          xlimitsArray2[index3].max = Math.Max(xlimitsArray1[index1][index3 - 1].max, xlimitsArray2[index3].max);
        }
        if (index3 != xlimitsArray1[index1].Length - 1)
        {
          xlimitsArray2[index3].min = Math.Min(xlimitsArray1[index1][index3 + 1].min, xlimitsArray2[index3].min);
          xlimitsArray2[index3].max = Math.Max(xlimitsArray1[index1][index3 + 1].max, xlimitsArray2[index3].max);
        }
      }
      for (int index4 = 0; index4 < xlimitsArray1[index1].Length; ++index4)
        xlimitsArray1[index1][index4] = xlimitsArray2[index4];
    }
    for (int index5 = 0; index5 < charSet.Length; ++index5)
    {
      for (int index6 = 0; index6 < charSet.Length; ++index6)
        kerning.Add(charSet[index5].ToString() + charSet[index6].ToString(), KerningCalculator.Kerning_(glyphs[index5], glyphs[index6], xlimitsArray1[index5], xlimitsArray1[index6], config, font));
    }
    return kerning;
  }

  private struct XLimits
  {
    public int min;
    public int max;
  }
}