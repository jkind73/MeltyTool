using System;
using System.Collections.Generic;
using System.Linq;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	public enum BitOperation
	{
		CLEAR,
		SET,
		TOGGLE,
		TEST,

		K_FIRST = CLEAR,
		K_LAST = TEST
	};

	public static partial class BitwiseT4
	{
		public const int K_BITS_PER_BYTE = sizeof(byte) * 8;

		// Get the keyword used to define general constants for integer types (bit count, etc)
		public static string GetConstantKeyword(this NumberCodeDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			switch (def.Code)
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					return "Byte";

				case TypeCode.UInt16:
				case TypeCode.Int16:
					return "Int16";

				case TypeCode.UInt32:
				case TypeCode.Int32:
					return "Int32";

				case TypeCode.UInt64:
				case TypeCode.Int64:
					return "Int64";

				default:
					throw new InvalidOperationException(def.Code.ToString());
			}
		}
		public static string GetVectorsSuffix(this NumberCodeDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			if (!def.IsByte)
				return GetConstantKeyword(def);

			return "Bytes";
		}

		#region Bitstream related
		/// <summary>	The integer type to use for bitstream cache operations. </summary>
		public static NumberCodeDefinition BitStreamCacheWord { get {
			return PrimitiveDefinitions.KUInt32;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleIntegerTypes { get {
			yield return PrimitiveDefinitions.KChar;

			foreach (var numType in PrimitiveDefinitions.Numbers)
				if (numType.IsInteger)
					yield return numType;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleNonIntegerTypes { get {
			yield return PrimitiveDefinitions.KBool;

			yield return PrimitiveDefinitions.KSingle;
			yield return PrimitiveDefinitions.KDouble;
		} }
		#endregion

		#region BitOperation extensions
		public static string FlagsMethod(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.CLEAR:	return "Bitwise.Flags.Remove";
			case BitOperation.SET:		return "Bitwise.Flags.Add";
			case BitOperation.TOGGLE:	return "Bitwise.Flags.Toggle";
			case BitOperation.TEST:		return "Bitwise.Flags.TestAny";

			default: throw new InvalidOperationException(op.ToString());
			}
		}
		public static string FlagsMethodBitsPrefix(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.CLEAR:
			case BitOperation.SET:
			case BitOperation.TOGGLE:
				return "ref";

			default:
				return "";
			}
		}
		public static string ResultType(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.TEST:
				return "bool";

			default:
				return "void";
			}
		}
		public static string ResultDefault(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.TEST:
				return "false";

			default:
				return "";
			}
		}
		public static bool IsNotPure(this BitOperation op)
		{
			return op != BitOperation.TEST;
		}
		public static bool RequiresCardinalityReUpdate(this BitOperation op)
		{
			return op != BitOperation.TEST;
		}
		#endregion

		#region BittableTypes
		static readonly IReadOnlyList<NumberCodeDefinition> KBittableTypesUnsigned = new List<NumberCodeDefinition> {
			PrimitiveDefinitions.KByte,
			PrimitiveDefinitions.KUInt16,
			PrimitiveDefinitions.KUInt32,
			PrimitiveDefinitions.KUInt64,
		};
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public static IReadOnlyList<NumberCodeDefinition> BittableTypesUnsigned { get {
			return KBittableTypesUnsigned;
		} }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public static IEnumerable<NumberCodeDefinition> BittableTypesMajorWords { get {
			yield return PrimitiveDefinitions.KUInt32;
			yield return PrimitiveDefinitions.KUInt64;
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypes { get {
			return PrimitiveDefinitions.Numbers.Where(numType => numType.IsInteger);
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypesInt32 { get {
			yield return PrimitiveDefinitions.KUInt32;
			yield return PrimitiveDefinitions.KInt32;
		} }
		public static IEnumerable<NumberCodeDefinition> BittableTypesInt32And64 { get {
			yield return PrimitiveDefinitions.KUInt32;
			yield return PrimitiveDefinitions.KInt32;
			yield return PrimitiveDefinitions.KUInt64;
			yield return PrimitiveDefinitions.KInt64;
		} }
		#endregion

		public sealed class IntegerByteAccessCodeGenerator
		{
			static readonly string KByteKeyword = NumberCodeDefinition.TypeCodeToKeyword(TypeCode.Byte);

			readonly TextTemplating.TextTransformation mFile_;
			//readonly NumberCodeDefinition mDef;
			readonly int mSizeOfInBits_;
			readonly int mSizeOfInBytes_;
			readonly string mByteName_;

			readonly string mBufferName_;
			readonly string mOffsetName_; // NOTE: offset variable must be mutable!

			internal IntegerByteAccessCodeGenerator(TextTemplating.TextTransformation ttFile,
				NumberCodeDefinition def,
				string byteName, string bufferName, string offsetName = null,
				int bitCount = -1)
			{
				this.mFile_ = ttFile;
				//mDef = def;
				this.mSizeOfInBits_ = bitCount == -1
					? def.SizeOfInBits
					: bitCount;
				this.mSizeOfInBytes_ = bitCount == -1
					? def.SizeOfInBytes
					: bitCount / K_BITS_PER_BYTE;
				this.mByteName_ = byteName;

				this.mBufferName_ = bufferName;
				this.mOffsetName_ = offsetName;
			}

			void GenerateByteDeclarationsCode()
			{
				this.mFile_.Write("{0} ", KByteKeyword);
				for (int x = 0; x < this.mSizeOfInBytes_; x++)
				{
					this.mFile_.Write("{0}{1}", this.mByteName_, x);

					if (x < (this.mSizeOfInBytes_ - 1))
						this.mFile_.Write(", ");
				}

				this.mFile_.WriteLine(";");
			}
			public void GenerateByteDeclarations()
			{
				// indent to method code body's indention level
				using (this.mFile_.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateByteDeclarationsCode();
				}
			}

			void GenerateBytesFromBufferCode()
			{
				Debug.Assert(this.mOffsetName_ != null, "generator not setup for read/write from/to a buffer");

				for (int x = 0; x < this.mSizeOfInBytes_; x++, this.mFile_.WriteLine(";"))
				{
					this.mFile_.Write("{0}{1} = ", this.mByteName_, x);
					this.mFile_.Write("{0}[{1}++]", this.mBufferName_, this.mOffsetName_);
				}
			}
			public void GenerateBytesFromBuffer()
			{
				// indent to method code body's indention level
				using (this.mFile_.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateBytesFromBufferCode();
				}
			}

			void GenerateBytesFromValueCode(string valueName, bool littleEndian)
			{
				int bitOffset = !littleEndian
					? this.mSizeOfInBits_
					: 0 - K_BITS_PER_BYTE;
				int bitAdjustment = !littleEndian
					? -K_BITS_PER_BYTE
					: +K_BITS_PER_BYTE;

				for (int x = 0; x < this.mSizeOfInBytes_; x++, this.mFile_.WriteLine(";"))
				{
					this.mFile_.Write("{0}{1} = ", this.mByteName_, x);
					this.mFile_.Write("({0})({1} >> {2,2})", KByteKeyword, valueName, bitOffset += bitAdjustment);
				}
			}
			public void GenerateBytesFromValue(string valueName, bool littleEndian = true)
			{
				// indent to method code body's indention level, plus one (assumed we are in a if-statement)
				using (this.mFile_.EnterCodeBlock(indentCount: 3+1))
				{
					this.GenerateBytesFromValueCode(valueName, littleEndian);
				}
			}

			void GenerateWriteBytesToBufferCode(bool useSwapFormat)
			{
				const string kSwapFormat =		"{0}[--{1}] = ";
				const string kReplacementFormat =	"{0}[{1}++] = ";

				Debug.Assert(this.mOffsetName_ != null);

				string format = useSwapFormat
					? kSwapFormat
					: kReplacementFormat;

				for (int x = 0; x < this.mSizeOfInBytes_; x++, this.mFile_.WriteLine(";"))
				{
					this.mFile_.Write(format, this.mBufferName_, this.mOffsetName_);
					this.mFile_.Write("{0}{1}", this.mByteName_, x);
				}
			}
			public void GenerateWriteBytesToBuffer(bool useSwapFormat = true)
			{
				// indent to method code body's indention level
				using (this.mFile_.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateWriteBytesToBufferCode(useSwapFormat);
				}
			}
		};

		public abstract class BitUtilCodeGenerator
		{
			protected TextTemplating.TextTransformation File { get; private set; }
			protected NumberCodeDefinition Def { get; private set; }

			protected BitUtilCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
			{
				this.File = ttFile;
				this.Def = def;
			}

			public void Generate()
			{
				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.GenerateXmlDoc();
					this.GenerateMethodSignature();

					using (this.File.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
					{
						this.GeneratePrologue();
						this.GenerateCode();
						this.GenerateEpilogue();
					}
				}
			}

			protected abstract void GenerateXmlDoc();
			protected abstract void GenerateMethodSignature();
			protected abstract void GeneratePrologue();
			protected abstract void GenerateEpilogue();
			protected abstract void GenerateCode();
		};
	};
}
