using System.Diagnostics.CodeAnalysis;

namespace KSoft.Collections
{
	/// <summary>Directions in which collection items may be sorted</summary>
	public enum SortDirection : byte
	{
		/// <summary>Sort data from least to greatest</summary>
		ASCENDING,
		/// <summary>Sort data from greatest to least</summary>
		DESCENDING,

		[System.Reflection.Obfuscation(Exclude=true)]
		[System.Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF
	};

	[System.Flags]
	[SuppressMessage("Microsoft.Design", "CA1714:FlagsEnumsShouldHavePluralNames")]
	public enum TreeTraversalDirection : byte
	{
		PRE_ORDER	= 1,	// Root, Left, Right
		IN_ORDER		= 2,	// Left, Root, Right
		POST_ORDER	= 3,	// Left, Right, Root

		LEFT		= 1 << 2,
		ROOT		= 1 << 3,
		RIGHT		= 1 << 4,

		K_ORDER_MASK	= (1 << 2) - 1,
		K_DIR_MASK	= (1 << 5) - 1 - K_ORDER_MASK,

		[System.Reflection.Obfuscation(Exclude=true)]
		[System.Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_ALL = K_ORDER_MASK | K_DIR_MASK
	};

	[System.Flags]
	public enum TreeTraversalOrders : byte
	{
		PRE_ORDER = 1 << 0,
		IN_ORDER = 1 << 1,
		POST_ORDER = 1 << 2,

		[System.Reflection.Obfuscation(Exclude=true)]
		K_ALL = PRE_ORDER | IN_ORDER | POST_ORDER
	};
}
