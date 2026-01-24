// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontShadowConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

#nullable disable
namespace QuickFont.Configuration;

public sealed class QFontShadowConfiguration
{
  public float scale = 1f;
  public ShadowType type;
  public int blurRadius = 3;
  public int blurPasses = 2;
  public int pageMaxTextureSize = 4096;
  public int glyphMargin = 2;
}