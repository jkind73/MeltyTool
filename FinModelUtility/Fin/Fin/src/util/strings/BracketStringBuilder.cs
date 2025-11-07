using System;
using System.Text;


namespace fin.util.strings;

public class BracketStringBuilder {
  private readonly StringBuilder impl_ = new();

  private int currentIndentAmount_;
  private bool hasIndentedLine_;

  public BracketStringBuilder AppendBlock(ReadOnlySpan<char> prefix,
                                          Action handler) {
    this.Append(prefix);
    this.AppendLine(" {");

    ++this.currentIndentAmount_;

    handler();

    --this.currentIndentAmount_;

    this.AppendLine('}');

    return this;
  }

  public BracketStringBuilder ExitBlock() {
    this.currentIndentAmount_--;
    this.Append('}');
    this.AppendNewline_();
    return this;
  }

  public BracketStringBuilder Append(char c) {
    if (c is '\n') {
      this.AppendNewline_();
    } else {
      this.IndentIfNeeded_();
      this.impl_.Append(c);
    }

    return this;
  }

  public BracketStringBuilder Append(ReadOnlySpan<char> chars) {
    var l = 0;
    foreach (var lineRange in chars.Split('\n')) {
      if (l++ > 0) {
        this.AppendNewline_();
      }

      this.IndentIfNeeded_();
      this.impl_.Append(chars[lineRange]);
    }

    return this;
  }

  public BracketStringBuilder AppendLine() {
    this.AppendNewline_();
    return this;
  }

  public BracketStringBuilder AppendLine(char c) {
    this.Append(c);
    this.AppendNewline_();
    return this;
  }

  public BracketStringBuilder AppendLine(ReadOnlySpan<char> chars) {
    this.Append(chars);
    this.AppendNewline_();
    return this;
  }

  private void AppendNewline_() {
    this.impl_.AppendLine();
    this.hasIndentedLine_ = false;
  }

  private void IndentIfNeeded_() {
    if (this.hasIndentedLine_) {
      return;
    }

    this.hasIndentedLine_ = true;
    for (var i = 0; i < this.currentIndentAmount_; ++i) {
      this.impl_.Append("  ");
    }
  }
}