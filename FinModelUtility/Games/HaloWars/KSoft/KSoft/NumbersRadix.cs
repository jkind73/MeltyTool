
namespace KSoft
{
	/// <summary>Valid numerical bases (radix) that can be used in this library suite</summary>
	[EnumBitEncoderDisable]
	public enum NumeralBase : byte
	{
		BINARY	= 2,
		OCTAL	= 8,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		DECIMAL = 10,
		HEX		= 16,
	};

	/// <summary>Valid numerical bases (radix) that can be used in <see cref="KSoft.Numbers"/></summary>
	[EnumBitEncoderDisable]
	public enum NumbersRadix : byte
	{
		BINARY	= NumeralBase.BINARY,
		OCTAL	= NumeralBase.OCTAL,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		DECIMAL = NumeralBase.DECIMAL,
		HEX		= NumeralBase.HEX,
		BASE36	= 36,
		BASE64	= 64,
	};
}
