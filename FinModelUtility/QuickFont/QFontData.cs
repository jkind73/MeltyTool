// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontData
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Drawing;

#nullable disable
namespace QuickFont;

internal class QFontData
{
  public Dictionary<string, int> kerningPairs;
  public TexturePage[] pages;
  public Dictionary<char, QFontGlyph> charSetMapping;
  public float meanGlyphWidth;
  public int maxGlyphHeight;
  public int maxLineHeight;
  public QFont dropShadowFont;
  public bool isDropShadow;
  public bool naturallyMonospaced;

  public bool IsMonospacingActive(QFontRenderOptions options)
  {
    return options.monospacing == QFontMonospacing.NATURAL && this.naturallyMonospaced || options.monospacing == QFontMonospacing.YES;
  }

  public float GetMonoSpaceWidth(QFontRenderOptions options)
  {
    return (float) Math.Ceiling(1.0 + (1.0 + (double) options.characterSpacing) * (double) this.meanGlyphWidth);
  }

  public List<string> Serialize()
  {
    List<string> stringList1 = [];
    List<string> stringList2 = stringList1;
    int num = this.pages.Length;
    string str1 = num.ToString() ?? "";
    stringList2.Add(str1);
    List<string> stringList3 = stringList1;
    num = this.charSetMapping.Count;
    string str2 = num.ToString() ?? "";
    stringList3.Add(str2);
    foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.charSetMapping)
    {
      char key = keyValuePair.Key;
      QFontGlyph qfontGlyph = keyValuePair.Value;
      List<string> stringList4 = stringList1;
      string[] strArray = new string[13];
      strArray[0] = key.ToString();
      strArray[1] = " ";
      strArray[2] = qfontGlyph.page.ToString();
      strArray[3] = " ";
      num = qfontGlyph.rect.X;
      strArray[4] = num.ToString();
      strArray[5] = " ";
      num = qfontGlyph.rect.Y;
      strArray[6] = num.ToString();
      strArray[7] = " ";
      num = qfontGlyph.rect.Width;
      strArray[8] = num.ToString();
      strArray[9] = " ";
      num = qfontGlyph.rect.Height;
      strArray[10] = num.ToString();
      strArray[11] = " ";
      strArray[12] = qfontGlyph.yOffset.ToString();
      string str3 = string.Concat(strArray);
      stringList4.Add(str3);
    }
    return stringList1;
  }

  public void Deserialize(List<string> input, out int pageCount, out char[] charSet)
  {
    this.charSetMapping = new Dictionary<char, QFontGlyph>();
    List<char> charList = [];
    try
    {
      pageCount = int.Parse(input[0]);
      int num = int.Parse(input[1]);
      for (int index = 0; index < num; ++index)
      {
        string[] strArray = input[2 + index].Split(' ');
        QFontGlyph qfontGlyph = new QFontGlyph(int.Parse(strArray[1]), new Rectangle(int.Parse(strArray[2]), int.Parse(strArray[3]), int.Parse(strArray[4]), int.Parse(strArray[5])), int.Parse(strArray[6]), strArray[0][0]);
        this.charSetMapping.Add(strArray[0][0], qfontGlyph);
        charList.Add(strArray[0][0]);
      }
    }
    catch (Exception ex)
    {
      throw new Exception("Failed to parse qfont file. Invalid format.", ex);
    }
    charSet = charList.ToArray();
  }

  public void CalculateMeanWidth()
  {
    this.meanGlyphWidth = 0.0f;
    foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.charSetMapping)
      this.meanGlyphWidth += (float) keyValuePair.Value.rect.Width;
    this.meanGlyphWidth /= (float) this.charSetMapping.Count;
  }

  public void CalculateMaxHeight()
  {
    this.maxGlyphHeight = 0;
    this.maxLineHeight = 0;
    foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.charSetMapping)
    {
      this.maxGlyphHeight = Math.Max(keyValuePair.Value.rect.Height, this.maxGlyphHeight);
      this.maxLineHeight = Math.Max(keyValuePair.Value.rect.Height + keyValuePair.Value.yOffset, this.maxLineHeight);
    }
  }

  public int GetKerningPairCorrection(int index, string text, TextNode textNode)
  {
    if (this.kerningPairs == null)
      return 0;
    char[] chArray = new char[2];
    if (index + 1 == text.Length)
    {
      if (textNode == null || textNode.next == null || textNode.next.type != TextNodeType.WORD)
        return 0;
      chArray[1] = textNode.next.text[0];
    }
    else
      chArray[1] = text[index + 1];
    chArray[0] = text[index];
    string key = new string(chArray);
    return this.kerningPairs.ContainsKey(key) ? this.kerningPairs[key] : 0;
  }

  public void Dispose()
  {
    foreach (TexturePage page in this.pages)
      page.Dispose();
    this.dropShadowFont?.Dispose();
  }
}