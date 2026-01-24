// Decompiled with JetBrains decompiler
// Type: QuickFont.TextNode
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

#nullable disable
namespace QuickFont;

internal class TextNode
{
  public TextNodeType type;
  public string text;
  public float length;
  public float lengthTweak;
  public float height;
  public TextNode next;
  public TextNode previous;

  public float ModifiedLength => this.length + this.lengthTweak;

  public TextNode(TextNodeType type, string text)
  {
    this.type = type;
    this.text = text;
  }
}