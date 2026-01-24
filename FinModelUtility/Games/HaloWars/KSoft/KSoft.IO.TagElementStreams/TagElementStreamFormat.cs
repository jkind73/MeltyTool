namespace KSoft.IO
{
	/// <remarks>If the <see cref="BINARY"/> flag is not set, assume 'Text'</remarks>
	public enum TagElementStreamFormat
	{
		UNDEFINED,

		XML,
		/// <summary>Currently unsupported</summary>
		JSON,
		/// <summary>Currently unsupported</summary>
		YAML,

		K_CUSTOM_START,
		K_CUSTOM_END = 1<<6,
		K_CUSTOM_MAX = K_CUSTOM_END - K_CUSTOM_START,

		BINARY = 1<<7,

		K_TYPE_FLAGS = BINARY,

		/// <summary>Currently unsupported</summary>
		BSON = BINARY | JSON,
	};
}
