// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontKerningConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System.Collections.Generic;

#nullable disable
namespace QuickFont.Configuration;

public sealed class QFontKerningConfiguration
{
  private readonly Dictionary<char, CharacterKerningRule> characterKerningRules_ = new Dictionary<char, CharacterKerningRule>();
  public byte alphaEmptyPixelTolerance;

  public void BatchSetCharacterKerningRule(string chars, CharacterKerningRule rule)
  {
    foreach (char key in chars)
      this.characterKerningRules_[key] = rule;
  }

  public void SetCharacterKerningRule(char c, CharacterKerningRule rule)
  {
    this.characterKerningRules_[c] = rule;
  }

  public CharacterKerningRule GetCharacterKerningRule(char c)
  {
    return this.characterKerningRules_.TryGetValue(c, out CharacterKerningRule rule) ? rule : CharacterKerningRule.NORMAL;
  }

  public CharacterKerningRule GetOverridingCharacterKerningRuleForPair(string str)
  {
    if (str.Length < 2)
      return CharacterKerningRule.NORMAL;
    char c1 = str[0];
    char c2 = str[1];
    if (this.GetCharacterKerningRule(c1) == CharacterKerningRule.ZERO || this.GetCharacterKerningRule(c2) == CharacterKerningRule.ZERO)
      return CharacterKerningRule.ZERO;
    return this.GetCharacterKerningRule(c1) == CharacterKerningRule.NOT_MORE_THAN_HALF || this.GetCharacterKerningRule(c2) == CharacterKerningRule.NOT_MORE_THAN_HALF ? CharacterKerningRule.NOT_MORE_THAN_HALF : CharacterKerningRule.NORMAL;
  }

  public QFontKerningConfiguration()
  {
    this.BatchSetCharacterKerningRule("_^", CharacterKerningRule.ZERO);
    this.SetCharacterKerningRule('"', CharacterKerningRule.NOT_MORE_THAN_HALF);
    this.SetCharacterKerningRule('\'', CharacterKerningRule.NOT_MORE_THAN_HALF);
  }
}