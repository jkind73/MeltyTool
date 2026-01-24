
namespace KSoft.Phoenix.XML
{
	/// <summary>Various flags for <see cref="BCollectionXmlParams"/></summary>
	/// <remarks>
	/// * Intern flags should be set when certain values are strings and are used repeatedly within game data
	/// </remarks>
	[System.Flags]
	public enum BCollectionXmlParamsFlags
	{
		// Only one of these should ever be set
		USE_INNER_TEXT_FOR_DATA = 1<<0,

		USE_ELEMENT_FOR_DATA = 1<<1,

		INTERN_DATA_NAMES = 1<<2,

		TO_LOWER_DATA_NAMES = 1<<3,
		REQUIRES_DATA_NAME_PRELOADING = 1<<4,

		/// <summary>Forces the list code to not stream the root element from the xml document</summary>
		/// <remarks>Needed for when we're reading definitions from game files, but will later write to a app-specific monolithic file</remarks>
		FORCE_NO_ROOT_ELEMENT_STREAMING = 1<<5,
		SUPPORTS_UPDATING = 1<<6,

		DO_NOT_WRITE_UNDEFINED_DATA = 1<<7,

		INTERN_EVERYTHING = INTERN_DATA_NAMES,
	};
}