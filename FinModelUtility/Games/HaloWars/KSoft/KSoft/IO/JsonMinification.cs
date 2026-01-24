/* This is a .NET port of the Douglas Crockford's JSMin 'C' project.
 * The author's copyright message is reproduced below.
 */

/* jsmin.c
   2013-03-29
Copyright (c) 2002 Douglas Crockford  (www.crockford.com)
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
The Software shall be used for Good, not Evil.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DouglasCrockford.JsMin
{
	/// <summary>
	/// The exception that is thrown when a minification of asset code by JSMin is failed
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1032")]
	[SuppressMessage("Microsoft.Design", "CA2237")]
	public sealed class JsMinificationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the DouglasCrockford.JsMin.JsMinificationException
		/// class with a specified error message
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public JsMinificationException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Initializes a new instance of the DouglasCrockford.JsMin.JsMinificationException
		/// class with a specified error message and a reference to the inner exception that is the cause of
		/// this exception
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception</param>
		/// <param name="innerException">The exception that is the cause of the current exception</param>
		public JsMinificationException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	};

	/// <summary>
	/// The JavaScript Minifier
	/// </summary>
	public sealed class JsMinifier
		: IDisposable
	{
		/// <summary>
		/// Average compression ratio
		/// </summary>
		const double AVERAGE_COMPRESSION_RATIO_ = 0.6;

		const int EOF_ = -1;

		private StringBuilder sb_;
		[SuppressMessage("Microsoft.Design", "CA2213:DisposableFieldsShouldBeDisposed")]
		private StringReader reader_;
		[SuppressMessage("Microsoft.Design", "CA2213:DisposableFieldsShouldBeDisposed")]
		private StringWriter writer_;

		private int theA_;
		private int theB_;
		private int theLookahead_ = EOF_;
		private int theX_ = EOF_;
		private int theY_ = EOF_;

		/// <summary>
		/// Synchronizer of minification
		/// </summary>
		private readonly object minificationSynchronizer_ = new object();


		public void Dispose()
		{
			KSoft.Util.DisposeAndNull(ref this.reader_);
			KSoft.Util.DisposeAndNull(ref this.writer_);
		}

		/// <summary>
		/// Removes a comments and unnecessary whitespace from JavaScript code
		/// </summary>
		/// <param name="content">JavaScript content</param>
		/// <returns>Minified JavaScript content</returns>
		public String Minify(string content)
		{
			string minifiedContent;

			lock (this.minificationSynchronizer_)
			{
				this.theA_ = 0;
				this.theB_ = 0;
				this.theLookahead_ = EOF_;
				this.theX_ = EOF_;
				this.theY_ = EOF_;

				int estimatedCapacity = (int)Math.Floor(content.Length * AVERAGE_COMPRESSION_RATIO_);
				if (this.sb_ == null)
				{
					this.sb_ = new StringBuilder(estimatedCapacity);
				}
				else
				{
					this.sb_.Capacity = estimatedCapacity;
				}

				this.reader_ = new StringReader(content);
				this.writer_ = new StringWriter(this.sb_);

				try
				{
					this.InnerMinify();
					this.writer_.Flush();

					minifiedContent = TrimStartAndToString(this.sb_);
				}
				catch (JsMinificationException)
				{
					throw;
				}
				finally
				{
					KSoft.Util.DisposeAndNull(ref this.reader_);
					KSoft.Util.DisposeAndNull(ref this.writer_);

					this.sb_.Clear();
				}
			}

			return minifiedContent;
		}

		/// <summary>
		/// Returns a true if the character is a letter, digit, underscore, dollar sign, or non-ASCII character
		/// </summary>
		/// <param name="c">The character</param>
		/// <returns>Result of check</returns>
		private static bool IsAlphanum(int c)
		{
			return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
				c > 126);
		}

		/// <summary>
		/// Returns a next character from input stream. Watch out for lookahead.
		/// If the character is a control character, translate it to a space or linefeed.
		/// </summary>
		/// <returns>The character</returns>
		private int Get()
		{
			int c = this.theLookahead_;
			this.theLookahead_ = EOF_;

			if (c == EOF_)
			{
				c = this.reader_.Read();
			}

			if (c >= ' ' || c == '\n' || c == EOF_)
			{
				return c;
			}

			if (c == '\r')
			{
				return '\n';
			}

			return ' ';
		}

		/// <summary>
		/// Gets a next character without getting it
		/// </summary>
		/// <returns>The character</returns>
		private int Peek()
		{
			this.theLookahead_ = this.Get();

			return this.theLookahead_;
		}

		/// <summary>
		/// Gets a next character, excluding comments.
		/// <code>Peek()</code> is used to see if a '/' is followed by a '/' or '*'.
		/// </summary>
		/// <returns>The character</returns>
		private int Next()
		{
			int c = this.Get();

			if (c == '/')
			{
				switch (this.Peek())
				{
					case '/':
						for (;;)
						{
							c = this.Get();

							if (c <= '\n')
							{
								break;
							}
						}

						break;
					case '*':
						this.Get();

						while (c != ' ')
						{
							switch (this.Get())
							{
								case '*':
									if (this.Peek() == '/')
									{
										this.Get();
										c = ' ';
									}

									break;
								case EOF_:
									throw new JsMinificationException("Unterminated comment.");
							}
						}

						break;
				}
			}

			this.theY_ = this.theX_;
			this.theX_ = c;

			return c;
		}

		/// <summary>
		/// Do something! What you do is determined by the argument:
		///		1 - Output A. Copy B to A. Get the next B.
		///		2 - Copy B to A. Get the next B. (Delete A).
		///		3 - Get the next B. (Delete B).
		/// <code>Action</code> treats a string as a single character.
		/// Wow! <code>Action</code> recognizes a regular expression
		/// if it is preceded by <code>(</code> or , or <code>=</code>.
		/// </summary>
		/// <param name="d">Action type</param>
		private void Action(int d)
		{
			if (d == 1)
			{
				this.Put(this.theA_);

				if (
					(this.theY_ == '\n' || this.theY_ == ' ') &&
					(this.theA_ == '+' || this.theA_ == '-' || this.theA_ == '*' || this.theA_ == '/') &&
					(this.theB_ == '+' || this.theB_ == '-' || this.theB_ == '*' || this.theB_ == '/')
				)
				{
					this.Put(this.theY_);
				}
			}

			if (d <= 2)
			{
				this.theA_ = this.theB_;

				if (this.theA_ == '\'' || this.theA_ == '"' || this.theA_ == '`')
				{
					for (;;)
					{
						this.Put(this.theA_);
						this.theA_ = this.Get();

						if (this.theA_ == this.theB_)
						{
							break;
						}

						if (this.theA_ == '\\')
						{
							this.Put(this.theA_);
							this.theA_ = this.Get();
						}

						if (this.theA_ == EOF_)
						{
							throw new JsMinificationException("Unterminated string literal.");
						}
					}
				}
			}

			if (d <= 3)
			{
				this.theB_ = this.Next();
				if (this.theB_ == '/' && (this.theA_ == '(' ||
				                          this.theA_ == ',' ||
				                          this.theA_ == '=' ||
				                          this.theA_ == ':' ||
				                          this.theA_ == '[' ||
				                          this.theA_ == '!' ||
				                          this.theA_ == '&' ||
				                          this.theA_ == '|' ||
				                          this.theA_ == '?' ||
				                          this.theA_ == '+' ||
				                          this.theA_ == '-' ||
				                          this.theA_ == '~' ||
				                          this.theA_ == '*' ||
				                          this.theA_ == '/' ||
				                          this.theA_ == '{' ||
				                          this.theA_ == '\n'
				))
				{
					this.Put(this.theA_);

					if (this.theA_ == '/' || this.theA_ == '*')
					{
						this.Put(' ');
					}

					this.Put(this.theB_);

					for (;;)
					{
						this.theA_ = this.Get();

						if (this.theA_ == '[')
						{
							for (;;)
							{
								this.Put(this.theA_);
								this.theA_ = this.Get();

								if (this.theA_ == ']')
								{
									break;
								}

								if (this.theA_ == '\\')
								{
									this.Put(this.theA_);
									this.theA_ = this.Get();
								}

								if (this.theA_ == EOF_)
								{
									throw new JsMinificationException("Unterminated set in Regular Expression literal.");
								}
							}
						}
						else if (this.theA_ == '/')
						{
							switch (this.Peek())
							{
								case '/':
								case '*':
									throw new JsMinificationException("Unterminated set in Regular Expression literal.");
							}

							break;
						}
						else if (this.theA_ == '\\')
						{
							this.Put(this.theA_);
							this.theA_ = this.Get();
						}

						if (this.theA_ == EOF_) {
							throw new JsMinificationException("Unterminated Regular Expression literal.");
						}

						this.Put(this.theA_);
					}

					this.theB_ = this.Next();
				}
			}
		}

		/// <summary>
		/// Copies a input to the output, deleting the characters which are insignificant to JavaScript.
		/// Comments will be removed. Tabs will be replaced with spaces.
		/// Carriage returns will be replaced with linefeeds. Most spaces and linefeeds will be removed.
		/// </summary>
		private void InnerMinify()
		{
			if (this.Peek() == 0xEF)
			{
				this.Get();
				this.Get();
				this.Get();
			}

			this.theA_ = '\n';
			this.Action(3);

			while (this.theA_ != EOF_)
			{
				switch (this.theA_)
				{
					case ' ':
						this.Action(IsAlphanum(this.theB_) ? 1 : 2);
						break;
					case '\n':
						switch (this.theB_)
						{
							case '{':
							case '[':
							case '(':
							case '+':
							case '-':
							case '!':
							case '~':
								this.Action(1);
								break;
							case ' ':
								this.Action(3);
								break;
							default:
								this.Action(IsAlphanum(this.theB_) ? 1 : 2);
								break;
						}

						break;
					default:
						switch (this.theB_)
						{
							case ' ':
								this.Action(IsAlphanum(this.theA_) ? 1 : 3);
								break;
							case '\n':
								switch (this.theA_)
								{
									case '}':
									case ']':
									case ')':
									case '+':
									case '-':
									case '"':
									case '\'':
									case '`':
										this.Action(1);
										break;
									default:
										this.Action(IsAlphanum(this.theA_) ? 1 : 3);
										break;
								}

								break;
							default:
								this.Action(1);
								break;
						}

						break;
				}
			}
		}

		#region Methods for substitution methods of the C language

		/// <summary>
		/// Puts a character to output stream
		/// </summary>
		/// <param name="c">The character</param>
		private void Put(int c)
		{
			this.writer_.Write((char)c);
		}

		#endregion

		/// <summary>
		/// Removes the all leading white-space characters from the current <see cref="StringBuilder"/> instance
		/// </summary>
		/// <param name="source">Instance of <see cref="StringBuilder"/></param>
		/// <returns>Instance of <see cref="StringBuilder"/> without leading white-space characters</returns>
		public static string TrimStartAndToString(StringBuilder source)
		{
			int charCount = source.Length;
			if (charCount == 0)
			{
				return source.ToString();
			}

			int charIndex = 0;

			while (charIndex < charCount)
			{
				char charValue = source[charIndex];
				if (!IsWhitespace(charValue))
				{
					break;
				}

				charIndex++;
			}

			if (charIndex > 0)
			{
				source.Remove(0, charIndex);
			}

			return source.ToString(charIndex, source.Length-charIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWhitespace(char source)
		{
			return source == ' ' || (source >= '\t' && source <= '\r');
		}
	};
}
