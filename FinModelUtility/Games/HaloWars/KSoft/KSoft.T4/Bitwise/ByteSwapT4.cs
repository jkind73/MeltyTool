using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	partial class BitwiseT4
	{
		public sealed class ByteSwapableIntegerDefinition
		{
			readonly NumberCodeDefinition mCodeDef_;
			readonly int mSizeOfInBits_;
			readonly int mSizeOfInBytes_;

			internal ByteSwapableIntegerDefinition(NumberCodeDefinition codeDef, int bitCount)
			{
				this.mCodeDef_ = codeDef;
				this.mSizeOfInBits_ = bitCount;
				this.mSizeOfInBytes_ = bitCount / K_BITS_PER_BYTE;
			}
			internal ByteSwapableIntegerDefinition(NumberCodeDefinition codeDef)
				: this(codeDef, codeDef.SizeOfInBits)
			{
			}

			public NumberCodeDefinition CodeDefinition { get { return this.mCodeDef_; } }
			public int SizeOfInBits { get { return this.mSizeOfInBits_; } }
			public int SizeOfInBytes { get { return this.mSizeOfInBytes_; } }

			/// <summary>Can this integer not be represented via a one of .NET's System.Int types?</summary>
			public bool IsUnnaturalWord { get { return this.SizeOfInBits != this.mCodeDef_.SizeOfInBits; } }

			public string Keyword { get { return this.mCodeDef_.Keyword; } }
			public string SignedKeyword { get { return this.mCodeDef_.SignedKeyword; } }
			public TypeCode Code { get { return this.mCodeDef_.Code; } }
			public TypeCode SignedCode { get { return this.mCodeDef_.SignedCode; } }

			public string ToStringHexFormat { get { return this.mCodeDef_.ToStringHexFormat; } }
			public bool BitOperatorsImplicitlyUpCast { get { return this.mCodeDef_.BitOperatorsImplicitlyUpCast; } }

			public string GetConstantKeyword()
			{
				return this.IsUnnaturalWord
					? "Int" + this.SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: this.mCodeDef_.GetConstantKeyword();
			}

			public NumberCodeDefinition TryGetSignedDefinition() { return this.mCodeDef_.TryGetSignedDefinition(); }

			public string WordTypeNameUnsigned { get {
				return this.IsUnnaturalWord
					? "UInt" + this.SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: this.Code.ToString();
			} }
			public string WordTypeNameSigned { get {
				return this.IsUnnaturalWord
					? "Int" + this.SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: this.SignedCode.ToString();
			} }
			public string GetOverloadSuffixForUnnaturalWord(bool isSigned)
			{
				if (!this.IsUnnaturalWord)
					return "";

				return isSigned
					? this.WordTypeNameSigned
					: this.WordTypeNameUnsigned;
			}

			public string SizeOfCode { get {
				return this.IsUnnaturalWord
					? "kSizeOf" + this.GetConstantKeyword()
					: string.Format(UtilT4.InvariantCultureInfo, "sizeof({0})", this.Keyword);
			} }

			public IntegerByteSwapCodeGenerator NewByteSwapCodeGenerator(TextTemplating.TextTransformation ttFile,
				string valueName = "value")
			{
				return new IntegerByteSwapCodeGenerator(ttFile, this, valueName);
			}

			public IntegerByteAccessCodeGenerator NewByteAccessCodeGenerator(TextTemplating.TextTransformation ttFile,
				string byteName = "b", string bufferName = "buffer", string offsetName = "offset")
			{
				return new IntegerByteAccessCodeGenerator(ttFile, this.mCodeDef_, byteName, bufferName, offsetName, this.mSizeOfInBits_);
			}
		};

		public static IEnumerable<ByteSwapableIntegerDefinition> ByteSwapableIntegers { get {
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.KUInt16);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.KUInt32);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.KUInt64);

			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.KUInt32, 24);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.KUInt64, 40);
		} }

		public sealed class IntegerByteSwapCodeGenerator
		{
			TextTemplating.TextTransformation mFile_;
			ByteSwapableIntegerDefinition mDef_;

			readonly string mValueName_;

			public IntegerByteSwapCodeGenerator(TextTemplating.TextTransformation ttFile,
				ByteSwapableIntegerDefinition def, string valueName)
			{
				this.mFile_ = ttFile;
				this.mDef_ = def;

				this.mValueName_ = valueName;
			}

			void GeneratePrologue(bool cast, bool isSigned)
			{
				// smaller integers are promoted to larger types when bit operated on
				// this is for casting the final result back into the smaller integer type
				if (cast)
				{
					var def = isSigned
						? this.mDef_.TryGetSignedDefinition()
						: this.mDef_.CodeDefinition;

					this.mFile_.WriteLine("({0})( ", def.Keyword);
					this.mFile_.PushIndent(TextTransformationCodeBlockBookmark.K_INDENT);
				}
			}
			void GenerateEpilogue(bool cast)
			{
				// close the cast operation group
				if (cast)
				{
					this.mFile_.PopIndent();
					this.mFile_.Write(")");
				}
			}
			void GenerateStep(bool isSigned, int shift, string mask, bool lastOperation = false)
			{
				// mask is not added when this is the last op in signed expression as the mask will be an unsigned value
				// this isn't the case for unnatural-words, which should consume fewer bits than the MSB/sign-bit
				bool maskOp = this.mDef_.IsUnnaturalWord || !isSigned || !lastOperation;
				if (maskOp)
					this.mFile_.Write("(");
				else
					this.mFile_.Write(" "); // add a space to keep aligned with statements prefixed with '('

				// start the shift operation group
				this.mFile_.Write("({0}", this.mValueName_);

				// LHS with positive numbers, RHS with negative
				if (shift > 0)
					this.mFile_.Write(" << ");
				else
				{
					this.mFile_.Write(" >> ");
					shift = -shift;
				}

				this.mFile_.Write("{0,2}", shift);

				// close the shift operation group
				this.mFile_.Write(")");

				// add the mask operation
				if (maskOp)
				{
					this.mFile_.Write(" & ");
					this.mFile_.Write("0x{0})", mask);
				}

				// not the last operation so OR this with the next operation
				if (!lastOperation)
					this.mFile_.Write(" | ");
			}

			void GenerateCode(bool isSigned)
			{
				// amount to increase 'shift' after each step
				const int kStepShiftInc = K_BITS_PER_BYTE * 2;

				// do we need to cast the result?
				bool cast = this.mDef_.BitOperatorsImplicitlyUpCast;
				string hexFormat = this.mDef_.ToStringHexFormat;

				this.GeneratePrologue(cast, isSigned);

				// GenerateStep's 'shift' is negative when the operation is a RHS (>>), and positive for LHS (<<)
				int shift = (0- this.mDef_.SizeOfInBits) + K_BITS_PER_BYTE;
				// our byte mask for each step
				ulong mask = byte.MaxValue;
				// While 'shift' is negative, we're generating steps to swap the MSB bytes to the LSBs half.
				// Once 'shift' becomes positive, we're generating steps to swap the LSB bytes to the MSBs half.
				for(int x = this.mDef_.SizeOfInBytes-1; x >= 0; x--, shift += kStepShiftInc, mask <<= K_BITS_PER_BYTE)
				{
					this.GenerateStep(isSigned, shift, mask.ToString(hexFormat, UtilT4.InvariantCultureInfo), x == 0);
					this.mFile_.NewLine();
				}

				this.GenerateEpilogue(cast);
			}
			public void Generate(bool isSigned = false)
			{
				// indent to method code body's indention level, plus one (l-value statement should be on the line before)
				using (this.mFile_.EnterCodeBlock(indentCount: 3+1))
				{
					this.GenerateCode(isSigned);

					this.mFile_.EndStmt();
				}
			}
		};
	};
}
