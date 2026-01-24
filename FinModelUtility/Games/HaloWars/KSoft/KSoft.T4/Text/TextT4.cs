using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public static class TextT4
	{
		const int K_MAX_DECIMAL_ = 10;
		const int K_MAX_HEXIDECIMAL_ = 16;

		static bool IsAlphabetOrDigit(int c)
		{
			return
				(c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'Z') ||
				(c >= 'a' && c <= 'z');
		}
		static bool IsHexidecimalDigit(int c)
		{
			return
				(c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'F') ||
				(c >= 'a' && c <= 'f');
		}

		static char ToCharCaseInvariant(char c, bool uppercase)
		{
			return uppercase
				? char.ToUpperInvariant(c)
				: char.ToLowerInvariant(c);
		}

		public abstract class CharLookupCodeGeneratorBase
		{
			protected TextTemplating.TextTransformation File { get; private set; }

			protected CharLookupCodeGeneratorBase(TextTemplating.TextTransformation ttFile)
			{
				this.File = ttFile;
			}

			protected abstract string TableName { get; }
			protected abstract string TableNamePostfix { get; }
			protected virtual string RowHeaderTabString { get { return "\t"; } }

			protected abstract void WriteXmlDoc();
			protected virtual bool LookupUsesCharByte(int charByte)
			{
				return IsAlphabetOrDigit(charByte);
			}
		};

		public abstract class CharLookupTableCodeGeneratorBase
			: CharLookupCodeGeneratorBase
		{
			protected CharLookupTableCodeGeneratorBase(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected abstract PrimitiveCodeDefinition TableElementType { get; }

			protected virtual int OverrideDigitIndex(int charByte, int digitIndex)
			{
				return digitIndex;
			}
			protected virtual string DigitToElementText(int digitIndex)
			{
				if (digitIndex < K_MAX_HEXIDECIMAL_)
				{
					if (digitIndex == -1)
						digitIndex = 0;

					return string.Format(UtilT4.InvariantCultureInfo, "0x{0},",
						digitIndex.ToString("X", UtilT4.InvariantCultureInfo));
				}

				return string.Format(UtilT4.InvariantCultureInfo, "{0}, ", digitIndex);
			}

			void WriteColumnHeader()
			{
				this.File.Write("//\t");
				for (int x = 0; x < K_MAX_HEXIDECIMAL_; x++)
					this.File.Write("{0}{1}", x.ToString("X", UtilT4.InvariantCultureInfo), this.RowHeaderTabString);
				this.File.WriteLine("");
			}
			void WriteColumnHeader_Numbers()
			{
				this.File.Write("//\t");
				for (int x = 0; x < K_MAX_DECIMAL_; x++)
					this.File.Write("{0}{1}", x, this.RowHeaderTabString);
				this.File.WriteLine("");
			}
			void WriteColumnHeader_A_to_O(bool uppercase)
			{
				char min = ToCharCaseInvariant('A', uppercase);
				char max = ToCharCaseInvariant('O', uppercase);

				this.File.Write("//\t");
				for (char x = min; x <= max; x++)
					this.File.Write("{0}{1}", x, this.RowHeaderTabString);
				this.File.WriteLine("");
			}
			void WriteColumnHeader_P_to_Z(bool uppercase)
			{
				char min = ToCharCaseInvariant('P', uppercase);
				char max = ToCharCaseInvariant('Z', uppercase);

				this.File.Write("//\t");
				for (char x = min; x <= max; x++)
					this.File.Write("{0}{1}", x, this.RowHeaderTabString);
				this.File.WriteLine("");
			}
			public void Generate()
			{
				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.WriteXmlDoc();
					this.File.WriteLine("static readonly {0}[] {1}{2} = {{",
					                    this.TableElementType.Keyword,
					                    this.TableName,
					                    this.TableNamePostfix);
				}

				this.WriteColumnHeader();

				for (int x = byte.MinValue, column = 0, row = 0, digitIndex = 0;
					x <= byte.MaxValue;
					x++)
				{
					#region write column headers
						 if (column == 0 && row == 3)
							 this.WriteColumnHeader_Numbers();
					else if (column == 0 && row == 4)
						this.WriteColumnHeader_A_to_O(uppercase:true);
					else if (column == 0 && row == 5)
						this.WriteColumnHeader_P_to_Z(uppercase:true);
					else if (column == 0 && row == 6)
						this.WriteColumnHeader_A_to_O(uppercase:false);
					else if (column == 0 && row == 7)
						this.WriteColumnHeader_P_to_Z(uppercase:false);
					#endregion

					if (column == 0)
						this.File.PushIndent("\t");

					if (this.LookupUsesCharByte(x))
					{
						digitIndex = this.OverrideDigitIndex(x, digitIndex);

						this.File.Write(this.DigitToElementText(digitIndex++));
					}
					else
						this.File.Write(this.DigitToElementText(-1));

					if (column++ == K_MAX_HEXIDECIMAL_-1)
					{
						this.File.PopIndent();

						this.File.WriteLine("// {0}", row.ToString("X", UtilT4.InvariantCultureInfo));
						column = 0;
						row++;
					}
				}

				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.File.WriteLine("};");
				}
			}
		};

		public abstract class CharToByteLookupTableCodeGeneratorBase
			: CharLookupTableCodeGeneratorBase
		{
			protected CharToByteLookupTableCodeGeneratorBase(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override PrimitiveCodeDefinition TableElementType { get { return PrimitiveDefinitions.KByte; } }
			protected override string TableName { get { return "kCharToByteLookup"; } }

			protected override void WriteXmlDoc()
			{
				this.File.WriteXmlDocSummary("Latin-1 lookup table for converting char to a digit");
				this.File.WriteXmlDocRemarks("Supports up to base {0}", this.TableNamePostfix);
			}
		};
		public sealed class CharToByteLookupTable36CodeGenerator
			: CharToByteLookupTableCodeGeneratorBase
		{
			public CharToByteLookupTable36CodeGenerator(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableNamePostfix { get { return 36.ToString(UtilT4.InvariantCultureInfo); } }

			protected override int OverrideDigitIndex(int charByte, int digitIndex)
			{
				if ((char)charByte == 'a')
					digitIndex = ('9' - '0') + 1;

				return digitIndex;
			}
		};
		public sealed class CharToByteLookupTable62CodeGenerator
			: CharToByteLookupTableCodeGeneratorBase
		{
			public CharToByteLookupTable62CodeGenerator(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableNamePostfix { get { return 62.ToString(UtilT4.InvariantCultureInfo); } }
		};
		public sealed class CharToByteLookupTable16CodeGenerator
			: CharToByteLookupTableCodeGeneratorBase
		{
			public CharToByteLookupTable16CodeGenerator(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableNamePostfix { get { return 16.ToString(UtilT4.InvariantCultureInfo); } }

			protected override bool LookupUsesCharByte(int charByte)
			{
				return IsHexidecimalDigit(charByte);
			}
			protected override int OverrideDigitIndex(int charByte, int digitIndex)
			{
				if ((char)charByte == 'a')
					digitIndex = ('9' - '0') + 1;

				return digitIndex;
			}
		};

		public abstract class CharIsDigitLookupTableCodeGeneratorBase
			: CharLookupTableCodeGeneratorBase
		{
			protected CharIsDigitLookupTableCodeGeneratorBase(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override PrimitiveCodeDefinition TableElementType { get { return PrimitiveDefinitions.KBool; } }
			protected override string TableName { get { return "kCharIsDigitLookup"; } }

			protected override string RowHeaderTabString { get { return "\t\t"; } }

			protected override void WriteXmlDoc()
			{
				this.File.WriteXmlDocSummary("Latin-1 lookup table for testing if char is a digit");
				this.File.WriteXmlDocRemarks("Supports up to base {0}", this.TableNamePostfix);
			}

			protected override string DigitToElementText(int digitIndex)
			{
				bool isDigit = digitIndex >= 0;

				return string.Format(UtilT4.InvariantCultureInfo, "{0},\t", isDigit.ToValueKeyword());
			}
		};
		public sealed class CharIsDigitLookupTable62CodeGenerator
			: CharIsDigitLookupTableCodeGeneratorBase
		{
			public CharIsDigitLookupTable62CodeGenerator(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableNamePostfix { get { return 62.ToString(UtilT4.InvariantCultureInfo); } }
		};
		public sealed class CharIsDigitLookupTable16CodeGenerator
			: CharIsDigitLookupTableCodeGeneratorBase
		{
			public CharIsDigitLookupTable16CodeGenerator(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableNamePostfix { get { return 16.ToString(UtilT4.InvariantCultureInfo); } }

			protected override bool LookupUsesCharByte(int charByte)
			{
				return IsHexidecimalDigit(charByte);
			}
		};


		public abstract class CharLookupBitVectorCodeGeneratorBase
			: CharLookupCodeGeneratorBase
		{
			const int K_VECTOR_ELEMENT_BIT_SIZE_ = Bitwise.BitwiseT4.K_BITS_PER_BYTE;
			static readonly PrimitiveCodeDefinition KVectorElementDef = PrimitiveDefinitions.KByte;

			protected CharLookupBitVectorCodeGeneratorBase(TextTemplating.TextTransformation ttFile)
				: base(ttFile)
			{
			}

			protected override string TableName { get { return "kCharIsDigitBitVector"; } }

			protected override void WriteXmlDoc()
			{
				this.File.WriteXmlDocSummary("Latin-1 lookup bitvector for testing if char is a digit");
				this.File.WriteXmlDocRemarks("Supports up to base {0}", this.TableNamePostfix);
			}

			protected virtual string BitArrayElementToElementText(byte element)
			{
				return string.Format(UtilT4.InvariantCultureInfo, "{0},\t", element.ToString("X2", UtilT4.InvariantCultureInfo));
			}

			static int GetBitArrayLength(int bitLength, int wordBitSize)
			{
				if (bitLength <= 0)
					return 0;

				return ((bitLength - 1) / wordBitSize) + 1;
			}
			System.Collections.BitArray BuildBitArray()
			{
				var bits = new System.Collections.BitArray(sizeof(byte) * Bitwise.BitwiseT4.K_BITS_PER_BYTE);

				for (int x = byte.MinValue;
					x <= byte.MaxValue;
					x++)
				{
					if (this.LookupUsesCharByte(x))
					{
						bits[x] = true;
					}
				}

				return bits;
			}
			public void Generate()
			{
				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.WriteXmlDoc();
					this.File.WriteLine("static readonly {0}[] {1}{2} = {{",
					                    KVectorElementDef.Keyword,
					                    this.TableName,
					                    this.TableNamePostfix);
				}

				var bits = this.BuildBitArray();
				var bitvector = new byte[GetBitArrayLength(bits.Length, K_VECTOR_ELEMENT_BIT_SIZE_)];
				bits.CopyTo(bitvector, 0);

				for (int x = 0, column = 0, row = 0;
					x <= bitvector.Length;
					x++)
				{
					if (column == 0)
						this.File.PushIndent("\t");

					this.File.Write(this.BitArrayElementToElementText(bitvector[x]));

					if (column++ == K_MAX_HEXIDECIMAL_-1)
					{
						this.File.PopIndent();

						this.File.WriteLine("// {0}", row.ToString("X", UtilT4.InvariantCultureInfo));
						column = 0;
						row++;
					}
				}

				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.File.WriteLine("};");
				}
			}
		};
	};
}
