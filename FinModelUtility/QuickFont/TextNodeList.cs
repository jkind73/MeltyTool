// Decompiled with JetBrains decompiler
// Type: QuickFont.TextNodeList
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections;
using System.Text;

#nullable disable
namespace QuickFont;

internal class TextNodeList : IEnumerable
{
  public TextNode head;
  public TextNode tail;

  public TextNodeList(string text)
  {
    text = text.Replace("\r\n", "\r");
    bool flag = false;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char ch in text)
    {
      switch (ch)
      {
        case '\n':
        case '\r':
        case ' ':
          if (flag)
          {
            this.Add(new TextNode(TextNodeType.WORD, stringBuilder.ToString()));
            flag = false;
          }
          switch (ch)
          {
            case '\n':
            case '\r':
              this.Add(new TextNode(TextNodeType.LINE_BREAK, (string) null));
              continue;
            case ' ':
              this.Add(new TextNode(TextNodeType.SPACE, (string) null));
              continue;
            default:
              continue;
          }
        default:
          if (!flag)
          {
            flag = true;
            stringBuilder = new StringBuilder();
          }
          stringBuilder.Append(ch);
          break;
      }
    }
    if (!flag)
      return;
    this.Add(new TextNode(TextNodeType.WORD, stringBuilder.ToString()));
  }

  public void MeasureNodes(QFontData fontData, QFontRenderOptions options)
  {
    foreach (TextNode node in this)
    {
      if ((double) Math.Abs(node.length) < 1.4012984643248171E-45)
        node.length = this.MeasureTextNodeLength_(node, fontData, options);
    }
  }

  private float MeasureTextNodeLength_(
      TextNode node,
      QFontData fontData,
      QFontRenderOptions options)
  {
    bool flag = fontData.IsMonospacingActive(options);
    float monoSpaceWidth = fontData.GetMonoSpaceWidth(options);
    if (node.type == TextNodeType.SPACE)
      return flag ? monoSpaceWidth : (float) Math.Ceiling((double) fontData.meanGlyphWidth * (double) options.wordSpacing);
    float num = 0.0f;
    float val1 = 0.0f;
    if (node.type == TextNodeType.WORD)
    {
      for (int index = 0; index < node.text.Length; ++index)
      {
        char key = node.text[index];
        if (fontData.charSetMapping.ContainsKey(key))
        {
          QFontGlyph qfontGlyph = fontData.charSetMapping[key];
          if (flag)
            num += monoSpaceWidth;
          else
            num += (float) Math.Ceiling((double) fontData.charSetMapping[key].rect.Width + (double) fontData.meanGlyphWidth * (double) options.characterSpacing + (double) fontData.GetKerningPairCorrection(index, node.text, node));
          val1 = Math.Max(val1, (float) (qfontGlyph.yOffset + qfontGlyph.rect.Height));
        }
      }
    }
    node.height = val1;
    return num;
  }

  public void Crumble(TextNode node, int baseCaseSize)
  {
    if (node.text.Length <= baseCaseSize)
      return;
    TextNode node1 = this.SplitNode(node);
    TextNode next = node1.next;
    this.Crumble(node1, baseCaseSize);
    this.Crumble(next, baseCaseSize);
  }

  public TextNode SplitNode(TextNode node)
  {
    if (node.type != TextNodeType.WORD)
      throw new Exception("Cannot slit text node of type: " + node.type.ToString());
    int num = node.text.Length / 2;
    string text1 = node.text[..num];
    string text2 = node.text.Substring(num, node.text.Length - num);
    TextNode textNode1 = new TextNode(TextNodeType.WORD, text1);
    TextNode textNode2 = new TextNode(TextNodeType.WORD, text2);
    textNode1.next = textNode2;
    textNode2.previous = textNode1;
    if (node.previous == null)
    {
      this.head = textNode1;
    }
    else
    {
      node.previous.next = textNode1;
      textNode1.previous = node.previous;
    }
    if (node.next == null)
    {
      this.tail = textNode2;
    }
    else
    {
      node.next.previous = textNode2;
      textNode2.next = node.next;
    }
    return textNode1;
  }

  public void Add(TextNode node)
  {
    if (this.head == null)
    {
      this.head = node;
      this.tail = node;
    }
    else
    {
      this.tail.next = node;
      node.previous = this.tail;
      this.tail = node;
    }
  }

  public override string ToString()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (TextNode textNode in this)
    {
      if (textNode.type == TextNodeType.SPACE)
        stringBuilder.Append(" ");
      if (textNode.type == TextNodeType.LINE_BREAK)
        stringBuilder.Append(Environment.NewLine);
      if (textNode.type == TextNodeType.WORD)
        stringBuilder.Append(textNode.text ?? "");
    }
    return stringBuilder.ToString();
  }

  public IEnumerator GetEnumerator()
  {
    return (IEnumerator) new TextNodeList.TextNodeListEnumerator(this);
  }

  private sealed class TextNodeListEnumerator : IEnumerator
  {
    private TextNode currentNode_;
    private TextNodeList targetList_;

    public TextNodeListEnumerator(TextNodeList targetList) => this.targetList_ = targetList;

    public object Current => (object) this.currentNode_;

    public bool MoveNext()
    {
      this.currentNode_ = this.currentNode_ != null ? this.currentNode_.next : this.targetList_.head;
      return this.currentNode_ != null;
    }

    public void Reset() => this.currentNode_ = (TextNode) null;
  }
}