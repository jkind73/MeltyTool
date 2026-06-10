using System;
using System.Collections.Generic;

namespace fin.util.strings;

/// <summary>
/// Creates a string comparer with natural sorting functionality
/// which allows it to sort numbers inside the strings as numbers, not as letters.
/// (e.g. "1", "2", "10" instead of "1", "10", "2").
/// It uses either a <seealso cref="StringComparison" /> (preferred) or arbitrary
/// <see cref="System.Collections.Generic.IComparer{T}" /> string comparer for the comparisons.
/// </summary>
public sealed class NaturalSortComparer : IComparer<string> {
  /// <summary>
  /// String comparison used for comparing strings.
  /// Used if <see cref="stringComparer_" /> is null.
  /// </summary>
  private readonly StringComparison stringComparison_;

  // Token values (not an enum as a performance micro-optimization)
  private const byte TOKEN_NONE_ = 0;
  private const byte TOKEN_OTHER_ = 1;
  private const byte TOKEN_DIGITS_ = 2;
  private const byte TOKEN_LETTERS_ = 3;

  /// <summary>
  /// Constructs comparer with a <seealso cref="StringComparison" /> as the inner mechanism.
  /// Prefer this to <see cref="NaturalSortComparer(System.Collections.Generic.IComparer{string})" /> if possible.
  /// </summary>
  /// <param name="stringComparison">String comparison to use</param>
  public NaturalSortComparer(StringComparison stringComparison)
    => this.stringComparison_ = stringComparison;

  /// <inheritdoc />
  public int Compare(string? str1, string? str2) {
    switch (str1 == null, str2 == null) {
      case (true, true):  return 0;
      case (false, true): return -1;
      case (true, false): return -1;
    }

    return this.Compare(str1.AsSpan(), str2.AsSpan());
  }

  /// <inheritdoc />
  public int Compare(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2) {
    if (str1.SequenceEqual(str2)) {
      return 0;
    }

    var strLength1 = str1.Length;
    var strLength2 = str2.Length;

    var startIndex1 = 0;
    var startIndex2 = 0;

    while (true) {
      // get next token from string 1
      var endIndex1 = startIndex1;
      var token1 = TOKEN_NONE_;
      while (endIndex1 < strLength1) {
        var charToken = GetTokenFromChar_(str1[endIndex1]);
        if (token1 == TOKEN_NONE_) {
          token1 = charToken;
        } else if (token1 != charToken) {
          break;
        }

        endIndex1++;
      }

      // get next token from string 2
      var endIndex2 = startIndex2;
      var token2 = TOKEN_NONE_;
      while (endIndex2 < strLength2) {
        var charToken = GetTokenFromChar_(str2[endIndex2]);
        if (token2 == TOKEN_NONE_) {
          token2 = charToken;
        } else if (token2 != charToken) {
          break;
        }

        endIndex2++;
      }

      // if the token kinds are different, compare just the token kind
      var tokenCompare = token1.CompareTo(token2);
      if (tokenCompare != 0)
        return tokenCompare;

      // now we know that both tokens are the same kind

      // didn't find any more tokens, return that they're equal
      if (token1 == TOKEN_NONE_)
        return 0;

      var rangeLength1 = endIndex1 - startIndex1;
      var rangeLength2 = endIndex2 - startIndex2;

      if (token1 == TOKEN_DIGITS_) {
        // compare both tokens as numbers
        var maxLength = Math.Max(rangeLength1, rangeLength2);

        // both spans will get padded by zeroes on the left to be the same length
        const char paddingChar = '0';
        var paddingLength1 = maxLength - rangeLength1;
        var paddingLength2 = maxLength - rangeLength2;

        for (var i = 0; i < maxLength; i++) {
          var digit1 = i < paddingLength1
              ? paddingChar
              : str1[startIndex1 + i - paddingLength1];
          var digit2 = i < paddingLength2
              ? paddingChar
              : str2[startIndex2 + i - paddingLength2];

          if (digit1 is >= '0' and <= '9' && digit2 is >= '0' and <= '9') {
            // both digits are ordinary 0 to 9
            var digitCompare = digit1.CompareTo(digit2);
            if (digitCompare != 0)
              return digitCompare;
          } else {
            // one or both digits is unicode, compare parsed numeric values, and only if they are same, compare as char
            var digitNumeric1 = char.GetNumericValue(digit1);
            var digitNumeric2 = char.GetNumericValue(digit2);
            var digitNumericCompare = digitNumeric1.CompareTo(digitNumeric2);
            if (digitNumericCompare != 0)
              return digitNumericCompare;

            var digitCompare = digit1.CompareTo(digit2);
            if (digitCompare != 0)
              return digitCompare;
          }
        }

        // if the numbers are equal, we compare how much we padded the strings
        var paddingCompare = paddingLength1.CompareTo(paddingLength2);
        if (paddingCompare != 0)
          return paddingCompare;
      } else {
        // only compare non-numeric tokens up to the shorter of their lengths
        if (rangeLength1 < rangeLength2) {
          rangeLength2 = rangeLength1;
          endIndex2 = startIndex2 + rangeLength2;
        } else if (rangeLength2 < rangeLength1) {
          rangeLength1 = rangeLength2;
          endIndex1 = startIndex1 + rangeLength1;
        }


        // use string comparison
        var stringCompare
            = str1.Slice(startIndex1, rangeLength1)
                  .CompareTo(
                      str2.Slice(startIndex2, rangeLength1),
                      this.stringComparison_);
        if (stringCompare != 0)
          return stringCompare;
      }

      startIndex1 = endIndex1;
      startIndex2 = endIndex2;
    }
  }

  private static byte GetTokenFromChar_(char c) {
    return c switch {
        >= 'a' and <= 'z'            => TOKEN_LETTERS_,
        >= 'a' when c < 128          => TOKEN_OTHER_,
        >= 'a' when char.IsLetter(c) => TOKEN_LETTERS_,
        >= 'a' when char.IsDigit(c)  => TOKEN_DIGITS_,
        >= 'a'                       => TOKEN_OTHER_,
        >= 'A' and <= 'Z'            => TOKEN_LETTERS_,
        >= 'A'                       => TOKEN_OTHER_,
        >= '0' and <= '9'            => TOKEN_DIGITS_,
        _                            => TOKEN_OTHER_,
    };
  }
}