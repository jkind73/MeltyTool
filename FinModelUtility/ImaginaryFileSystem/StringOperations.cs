namespace System.IO.Abstractions.TestingHelpers;

/// <summary>
/// Provides operations against path strings dependeing on the case-senstivity of the runtime platform.
/// </summary>
#if FEATURE_SERIALIZABLE
[Serializable]
#endif
public class StringOperations {
  private readonly bool caseSensitive_;
  private readonly StringComparison comparison_;

  /// <summary>
  /// Creates a new instance.
  /// </summary>
  public StringOperations(bool caseSensitive) {
    this.caseSensitive_ = caseSensitive;
    this.comparison_ = caseSensitive
        ? StringComparison.Ordinal
        : StringComparison.OrdinalIgnoreCase;
  }

  /// <summary>
  /// Provides a string comparer.
  /// </summary>
  public StringComparer Comparer => this.caseSensitive_
      ? StringComparer.Ordinal
      : StringComparer.OrdinalIgnoreCase;

  /// <summary>
  /// Determines whether the given string starts with the given prefix.
  /// </summary>
  public bool StartsWith(string s, string prefix)
    => s.StartsWith(prefix, this.comparison_);

  /// <summary>
  /// Determines whether the given string ends with the given suffix.
  /// </summary>
  public bool EndsWith(string s, string suffix)
    => s.EndsWith(suffix, this.comparison_);

  /// <summary>
  /// Determines whether the given strings are equal.
  /// </summary>
  public bool Equals(string x, string y) => string.Equals(x, y, this.comparison_);

  /// <summary>
  /// Determines whether the given characters are equal.
  /// </summary>
  public bool Equals(char x, char y)
    => this.caseSensitive_ ? x == y : char.ToUpper(x) == char.ToUpper(y);

  /// <summary>
  /// Determines the index of the given substring in the string.
  /// </summary>
  public int IndexOf(string s, string substring)
    => s.IndexOf(substring, this.comparison_);

  /// <summary>
  /// Determines the index of the given substring in the string.
  /// </summary>
  public int IndexOf(string s, string substring, int startIndex)
    => s.IndexOf(substring, startIndex, this.comparison_);

  /// <summary>
  /// Determines whether the given string contains the given substring.
  /// </summary>
  public bool Contains(string s, string substring)
    => s.IndexOf(substring, this.comparison_) >= 0;

  /// <summary>
  /// Replaces a given value by a new value.
  /// </summary>
  public string Replace(string s, string oldValue, string newValue)
    => s.Replace(oldValue, newValue, this.comparison_);

  /// <summary>
  /// Provides the lower-case representation of the given character.
  /// </summary>
  public char ToLower(char c) => this.caseSensitive_ ? c : char.ToLower(c);

  /// <summary>
  /// Provides the upper-case representation of the given character.
  /// </summary>
  public char ToUpper(char c) => this.caseSensitive_ ? c : char.ToUpper(c);

  /// <summary>
  /// Provides the lower-case representation of the given string.
  /// </summary>
  public string ToLower(string s) => this.caseSensitive_ ? s : s.ToLower();

  /// <summary>
  /// Provides the upper-case representation of the given string.
  /// </summary>
  public string ToUpper(string s) => this.caseSensitive_ ? s : s.ToUpper();
}