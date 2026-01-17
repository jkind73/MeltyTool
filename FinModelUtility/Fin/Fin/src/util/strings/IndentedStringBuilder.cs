using System;
using System.Text;


namespace fin.util.strings;

public class IndentedStringBuilder {
  private readonly StringBuilder impl_ = new();

  private int currentIndentAmount_;
  private bool hasIndentedLine_;
  private bool lastWasBlock_;

  public override string ToString() => this.impl_.ToString();

  public IndentedStringBuilder AppendBlock(ReadOnlySpan<char> prefix,
                                          Action handler) {
    this.Append(prefix);
    this.AppendLine(" {");

    ++this.currentIndentAmount_;

    handler();

    --this.currentIndentAmount_;

    this.Append('}');
    this.lastWasBlock_ = true;

    return this;
  }

  public IndentedStringBuilder Append(char c) {
    if (c is '\n') {
      this.AppendNewline_();
    } else {
      this.IndentIfNeeded_();
      this.impl_.Append(c);
    }

    return this;
  }

  public IndentedStringBuilder Append(string s)
    => this.Append(s.AsSpan());

  public IndentedStringBuilder Append(ReadOnlySpan<char> chars) {
    var l = 0;
    foreach (var line in chars.EnumerateLines()) {
      if (l++ > 0) {
        this.AppendNewline_();
      }

      if (line.Length > 0) {
        this.IndentIfNeeded_();
        this.impl_.Append(line);
      }
    }

    return this;
  }

  public IndentedStringBuilder Append<T>(T value)
    => this.Append(value?.ToString() ?? "");

  public IndentedStringBuilder AppendLine() {
    this.AppendNewline_();
    return this;
  }

  public IndentedStringBuilder AppendLine(char c) {
    this.Append(c);
    this.AppendNewline_();
    return this;
  }

  public IndentedStringBuilder AppendLine(string s) {
    this.Append(s);
    this.AppendNewline_();
    return this;
  }

  public IndentedStringBuilder AppendLine(ReadOnlySpan<char> chars) {
    this.Append(chars);
    this.AppendNewline_();
    return this;
  }

  public IndentedStringBuilder AppendLine<T>(T value) {
    this.Append(value);
    this.AppendNewline_();
    return this;
  }

  private void AppendNewline_() {
    this.impl_.AppendLine();
    this.hasIndentedLine_ = false;
    this.lastWasBlock_ = false;
  }

  private void IndentIfNeeded_() {
    if (this.hasIndentedLine_) {
      return;
    }

    if (this.lastWasBlock_) {
      this.lastWasBlock_ = false;
      this.AppendNewline_();
    }

    this.hasIndentedLine_ = true;
    for (var i = 0; i < this.currentIndentAmount_; ++i) {
      this.impl_.Append("  ");
    }
  }
}