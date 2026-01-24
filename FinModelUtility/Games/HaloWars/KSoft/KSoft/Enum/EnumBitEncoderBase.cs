using System;
#if CONTRACTS_FULL_SHIM

#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	/// <summary>Nothing public to see here, move along.</summary>
	public abstract class EnumBitEncoderBase
	{
		/// <summary>
		/// Applied to enumeration members <b>kMax</b> and <b>kAll</b> which aren't meant to be used in operational code
		/// </summary>
		public const string K_OBSOLETE_MSG = "For 'KSoft.IO.EnumBitEncoderBase' use only!";

		public const string K_ENUM_MAX_MEMBER_NAME = "kMax";
		public const string K_ENUM_NUMBER_OF_MEMBER_NAME = "kNumberOf";
		public const string K_FLAGS_MAX_MEMBER_NAME = "kAll";

		protected static bool ValidateTypeIsNotEncoderDisabled(Type t)
		{
			var attr = t.GetCustomAttributes(typeof(EnumBitEncoderDisableAttribute), false);

			return attr.Length == 0;
		}

		protected static void InitializeBase(Type t)
		{
			Reflection.EnumUtils.AssertTypeIsEnum(t);

			if (!ValidateTypeIsNotEncoderDisabled(t))
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"EnumBitEncoder can't operate on enumerations with an EnumBitEncoderDisableAttribute! {0}",
					t.FullName));
		}

		[System.Diagnostics.Conditional("TRACE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
		protected static void ProcessMembers_DebugCheckMemberName(Type t, bool isFlags, string memberName)
		{
			if (isFlags && (memberName == K_ENUM_NUMBER_OF_MEMBER_NAME || memberName == K_ENUM_MAX_MEMBER_NAME))
				Debug.Trace.Io.TraceInformation("Flags enum '{0}' has the Enum EnumBitEncoder member. Is this intentional?", t);
			else if (!isFlags && memberName == K_FLAGS_MAX_MEMBER_NAME)
				Debug.Trace.Io.TraceInformation("Enum '{0}' has the Flags EnumBitEncoder member. Is this intentional?", t);
		}
		protected static bool IsMaxMemberName(bool isFlags, string memberName)
		{
			bool result = false;

			if (isFlags)
				result = memberName == K_FLAGS_MAX_MEMBER_NAME;
			else
			{
				result =memberName == K_ENUM_NUMBER_OF_MEMBER_NAME ||
						memberName == K_ENUM_MAX_MEMBER_NAME;
			}

			return result;
		}

		public abstract int BitCountTrait { get; }
	};

	public interface IEnumBitEncoder<TUInt>
	{
		bool IsFlags { get; }
		bool HasNone { get; }
		/// <summary>Max value of the enum. NONE encoding is NOT factored in</summary>
		TUInt MaxValueTrait { get; }
		/// <summary>Masking value that can be used to single out this enumeration's value(s)</summary>
		TUInt BitmaskTrait { get; }
		/// <summary>How many bits the enumeration consumes</summary>
		int BitCountTrait { get; }

		/// <summary>The bit index assumed when one isn't provided</summary>
		int DefaultBitIndex { get; }
	};
}
