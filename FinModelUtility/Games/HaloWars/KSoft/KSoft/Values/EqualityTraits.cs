using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Describes how a value is compared for equality</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum EqualityTraits : byte
	{
		NOT_EQUAL = 0,
		EQUAL = 1 << 0,			// 1

		LESS_THAN = 2 << 0,		// 2
		GREATER_THAN = 2 << 1,	// 4

		LESS_THAN_EQUAL =			// 3
			EQUAL | LESS_THAN,
		GREATER_THAN_EQUAL =		// 5
			EQUAL | GREATER_THAN,

		[System.Reflection.Obfuscation(Exclude=false)]
		K_EQUALITY_MASK =			// 1
			NOT_EQUAL | EQUAL,
		[System.Reflection.Obfuscation(Exclude=false)]
		K_INEQUALITY_MASK =		// 6
			LESS_THAN | GREATER_THAN,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)]
		K_ALL =					// 7
			K_EQUALITY_MASK | K_INEQUALITY_MASK,
	};
}

namespace KSoft
{
	partial class TypeExtensions
	{
		/// <summary>Valides that LessThan and GreaterThan are not set at the same time</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		static bool ValidInequalityBits(this Values.EqualityTraits value) =>
			(value & Values.EqualityTraits.K_INEQUALITY_MASK) != Values.EqualityTraits.K_INEQUALITY_MASK;

		[Contracts.Pure]
		static Values.EqualityTraits GetEqualityBits(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			return value & Values.EqualityTraits.K_EQUALITY_MASK;
		}
		[Contracts.Pure]
		static Values.EqualityTraits GetInequalityBits(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			return value & Values.EqualityTraits.K_INEQUALITY_MASK;
		}

		/// <summary>Can the comparison be considered not equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsNotEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.NOT_EQUAL;

		/// <summary>Can the comparison be considered equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.EQUAL;

		[Contracts.Pure]
		public static bool IsLessThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the LessThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.LESS_THAN;

		[Contracts.Pure]
		public static bool IsGreaterThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the GreaterThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.GREATER_THAN;

		[Contracts.Pure]
		public static bool IsLessThanOrEqual(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			// Either the Equal or LessThan (or both) bits are set
			return (value & Values.EqualityTraits.LESS_THAN_EQUAL) != 0;
		}
		[Contracts.Pure]
		public static bool IsGreaterThanOrEqual(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			// Either the Equal or GreaterThan (or both) bits are set
			return (value & Values.EqualityTraits.GREATER_THAN_EQUAL) != 0;
		}
	};
}
