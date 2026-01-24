// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontBuilderConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace QuickFont.Configuration;

public sealed class QFontBuilderConfiguration : QFontConfiguration
{
  private const string BASIC_SET_ = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&€°µ";
  private const string FRENCH_QUOTES_ = "«»‹›";
  private const string SPANISH_QEST_EX_ = "¡¿";
  private const string CYRILLIC_SET_ = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяљњќћџЉЊЌЋЏ";
  private const string EXTENDED_LATIN_ = "ÀŠŽŸžÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ";
  private const string GREEK_ALPHABET_ = "ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ";
  private const string TURKISH_I_ = "ıİŞ";
  private const string HEBREW_ALPHABET_ = "אבגדהוזחטיכךלמםנןסעפףצץקרשת";
  private const string ARABIC_ALPHABET_ = "ںکگپچژڈ¯؛ہءآأؤإئابةتثجحخدذرزسشصض×طظعغـفقكàلâمنهوçèéêëىيîï؟";
  private const string THAI_KHMER_ALPHABET_ = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำิีึืฺุู฿เแโใไๅๆ็่้๊๋์ํ๎๏๐๑๒๓๔๕๖๗๘๙๚๛";
  private const string HIRAGANA_ = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ\u3097\u3098゙゛゜ゝゞゟ";
  private const string JAP_DIGITS_ = "㆐㆑\u3192\u3193\u3194\u3195㆖㆗㆘㆙㆚㆛㆜㆝㆞㆟";
  private const string ASIAN_QUOTES_ = "「」";
  private const string ESSENTIAL_KANJI_ = "⽇⽉";
  private const string KATAKANA_ = "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ・ーヽヾヿ";
  public int superSampleLevels = 1;
  public int pageMaxTextureSize = 4096;
  public int glyphMargin = 2;
  public string charSet = QFontBuilderConfiguration.BuildCharacterSet_(QFontBuilderConfiguration.FigureOutBestCharacterSet_());
  private CharacterSet characters_;
  public TextGenerationRenderHint textGenerationRenderHint = TextGenerationRenderHint.SIZE_DEPENDENT;

  public CharacterSet Characters
  {
    get => this.characters_;
    set
    {
      this.characters_ = value;
      this.charSet = QFontBuilderConfiguration.BuildCharacterSet_(this.characters_);
    }
  }

  public QFontBuilderConfiguration()
  {
  }

  public QFontBuilderConfiguration(bool addDropShadow, bool transformToOrthogProjection = false)
      : base(addDropShadow, transformToOrthogProjection)
  {
  }

  public QFontBuilderConfiguration(QFontConfiguration fontConfiguration)
  {
    this.shadowConfig = fontConfiguration.shadowConfig;
    this.kerningConfig = fontConfiguration.kerningConfig;
    this.transformToCurrentOrthogProjection = fontConfiguration.transformToCurrentOrthogProjection;
  }

  private static string BuildCharacterSet_(CharacterSet set)
  {
    Array values = Enum.GetValues(typeof (CharacterSet));
    string str = "";
    foreach (CharacterSet flag in values)
    {
      if (set.HasFlag((Enum) flag))
      {
        switch (flag)
        {
          case CharacterSet.BASIC_SET:
            str += "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&€°µ";
            continue;
          case CharacterSet.FRENCH_QUOTES:
            str += "«»‹›";
            continue;
          case CharacterSet.SPANISH_QUEST_EX:
            str += "¡¿";
            continue;
          case CharacterSet.CYRILLIC_SET:
            str += "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяљњќћџЉЊЌЋЏ";
            continue;
          case CharacterSet.EXTENDED_LATIN:
            str += "ÀŠŽŸžÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ";
            continue;
          case CharacterSet.GREEK_ALPHABET:
            str += "ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ";
            continue;
          case CharacterSet.TURKISH_I:
            str += "ıİŞ";
            continue;
          case CharacterSet.HEBREW_ALPHABET:
            str += "אבגדהוזחטיכךלמםנןסעפףצץקרשת";
            continue;
          case CharacterSet.ARABIC_ALPHABET:
            str += "ںکگپچژڈ¯؛ہءآأؤإئابةتثجحخدذرزسشصض×طظعغـفقكàلâمنهوçèéêëىيîï؟";
            continue;
          case CharacterSet.THAI_KHMER_ALPHABET:
            str += "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำิีึืฺุู฿เแโใไๅๆ็่้๊๋์ํ๎๏๐๑๒๓๔๕๖๗๘๙๚๛";
            continue;
          case CharacterSet.HIRAGANA:
            str += "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ\u3097\u3098゙゛゜ゝゞゟ";
            continue;
          case CharacterSet.JAP_DIGITS:
            str += "㆐㆑\u3192\u3193\u3194\u3195㆖㆗㆘㆙㆚㆛㆜㆝㆞㆟";
            continue;
          case CharacterSet.ASIAN_QUOTES:
            str += "「」";
            continue;
          case CharacterSet.ESSENTIAL_KANJI:
            str += "⽇⽉";
            continue;
          case CharacterSet.KATAKANA:
            str += "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ・ーヽヾヿ";
            continue;
          default:
            continue;
        }
      }
    }
    HashSet<char> source = [];
    foreach (char ch in str)
      source.Add(ch);
    return new string(source.ToArray<char>());
  }

  private static CharacterSet FigureOutBestCharacterSet_()
  {
    switch (CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
    {
      case 874:
        return CharacterSet.THAI;
      case 932:
        return CharacterSet.JAPANESE;
      case 1251:
        return CharacterSet.CYRILLIC;
      case 1252:
      case 1257:
        return CharacterSet.GENERAL;
      case 1253:
        return CharacterSet.GREEK;
      case 1254:
        return CharacterSet.TURKISH;
      case 1255:
        return CharacterSet.HEBREW_ALPHABET;
      case 1256:
        return CharacterSet.ARABIC;
      default:
        return CharacterSet.BASIC_SET;
    }
  }
}