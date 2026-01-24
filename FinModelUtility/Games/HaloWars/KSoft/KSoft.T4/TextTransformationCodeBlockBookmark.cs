using System;

namespace KSoft.T4
{
	enum TextTransformationCodeBlockType
	{
		NO_BRACKETS,
		BRACKETS,
		/// <summary>Mainly for a class definition statement</summary>
		BRACKETS_STATEMENT,
	};
	struct TextTransformationCodeBlockBookmark
		: IDisposable
	{
		internal const string K_INDENT = "\t";

		readonly Microsoft.VisualStudio.TextTemplating.TextTransformation mFile_;
		readonly TextTransformationCodeBlockType mType_;
		readonly int mIndentCount_;

		public TextTransformationCodeBlockBookmark(Microsoft.VisualStudio.TextTemplating.TextTransformation file,
			TextTransformationCodeBlockType type, int indentCount = 1)
		{
			this.mFile_ = file;
			this.mType_ = type;
			this.mIndentCount_ = indentCount;
		}

		internal void Enter()
		{
			if (this.mType_ == TextTransformationCodeBlockType.BRACKETS ||
			    this.mType_ == TextTransformationCodeBlockType.BRACKETS_STATEMENT)
				this.mFile_.WriteLine("{");

			for (int x = 0; x < this.mIndentCount_; x++)
				this.mFile_.PushIndent(K_INDENT);
		}

		public void Dispose()
		{
			for (int x = 0; x < this.mIndentCount_; x++)
				this.mFile_.PopIndent();

			if (this.mType_ == TextTransformationCodeBlockType.BRACKETS)
				this.mFile_.WriteLine("}");
			else if (this.mType_ == TextTransformationCodeBlockType.BRACKETS_STATEMENT)
				this.mFile_.WriteLine("};");
		}
	};
}