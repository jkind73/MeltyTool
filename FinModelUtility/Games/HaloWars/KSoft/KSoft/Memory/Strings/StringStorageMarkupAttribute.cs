using System;
using KSoft.Shell;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory.Strings
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class StringStorageMarkupAttribute : Attribute
	{
		public StringStorage Storage { get; private set; }

		#region Ctor
		/// <summary>Define a string storage markup</summary>
		/// <param name="widthType">Width size of a single character of the string type</param>
		/// <param name="type">Storage method for this string type</param>
		/// <param name="byteOrder"></param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string type</param>
		public StringStorageMarkupAttribute(StringStorageWidthType widthType, StringStorageType type,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.LITTLE, short fixedLength = 0)
		{
			Contract.Requires(fixedLength >= 0);

			this.Storage = new StringStorage(widthType, type, byteOrder, fixedLength);
		}
		/// <summary>Define a string storage markup (in <see cref="EndianFormat.LITTLE"/> byte order)</summary>
		/// <param name="widthType">Width size of a single character of this string type</param>
		/// <param name="type">Storage method for this string type</param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string type</param>
		public StringStorageMarkupAttribute(StringStorageWidthType widthType, StringStorageType type, short fixedLength) :
			this(widthType, type, Shell.EndianFormat.LITTLE, fixedLength)
		{
			Contract.Requires(fixedLength >= 0);
		}
		#endregion
	};

	/// <summary>CString, Ascii</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupAsciiAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupAsciiAttribute()
			: base(StringStorageWidthType.ASCII, StringStorageType.C_STRING) { }
	};
	/// <summary>CString, Unicode</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupUnicodeAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupUnicodeAttribute()
			: base(StringStorageWidthType.UNICODE, StringStorageType.C_STRING) { }
	};
	/// <summary>CString, Unicode-BE</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CStringStorageMarkupUnicodeBeAttribute : StringStorageMarkupAttribute
	{
		public CStringStorageMarkupUnicodeBeAttribute()
			: base(StringStorageWidthType.UNICODE, StringStorageType.C_STRING, Shell.EndianFormat.BIG) { }
	};

	/// <summary>CharArray, Ascii</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupAsciiAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupAsciiAttribute()
			: base(StringStorageWidthType.ASCII, StringStorageType.CHAR_ARRAY) { }
	};
	/// <summary>CharArray, Unicode</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupUnicodeAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupUnicodeAttribute()
			: base(StringStorageWidthType.UNICODE, StringStorageType.CHAR_ARRAY) { }
	};
	/// <summary>CharArray, Unicode-BE</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class StringStorageMarkupUnicodeBeAttribute : StringStorageMarkupAttribute
	{
		public StringStorageMarkupUnicodeBeAttribute()
			: base(StringStorageWidthType.UNICODE, StringStorageType.CHAR_ARRAY, Shell.EndianFormat.BIG) { }
	};
}
