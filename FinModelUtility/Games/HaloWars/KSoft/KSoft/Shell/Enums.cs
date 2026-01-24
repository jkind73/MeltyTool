using System;

namespace KSoft.Shell
{
	/// <summary>The ordering which bytes are interpreted on the processor</summary>
	/// <remarks>
	/// See http://en.wikipedia.org/wiki/Endian for more information.
	///
	/// "Middle-endian" is unsupported.
	///
	/// Most people do not realize that the terms 'big-endian' and 'little-endian' come
	/// from <a href="http://www.jaffebros.com/lee/gulliver/index.html">Gulliver's Travels</a>.
	/// The nations of <a href="http://www.jaffebros.com/lee/gulliver/bk1/chap1-4.html">Lilliput
	/// and Blefuscu</a> were waging a terrible and bloody war over which end one should cut
	/// open on a boiled egg - the little end or the big end.
	/// </remarks>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum EndianFormat : byte
	{
		/// <summary>Least Significant Bit order (lsb)</summary>
		/// <remarks>Right-to-Left</remarks>
		/// <see cref="BitConverter.IsLittleEndian"/>
		LITTLE,
		/// <summary>Most Significant Bit order (msb)</summary>
		/// <remarks>Left-to-Right</remarks>
		BIG,

		/// <remarks>1 bit</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	/// <summary>Supported processor instruction set sizes</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum ProcessorSize : byte
	{
		/// <summary>Processor size used is determined during runtime. Special for managed frameworks like .NET</summary>
		ANY_CPU,

		/// <summary>32-bit processor</summary>
		X32,
		/// <summary>64-bit processor</summary>
		X64,

		#region Reserved
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED3,

#if false
		/// <summary>128-bit processor</summary>
		/// <remarks>http://en.wikipedia.org/wiki/128-bit</remarks>
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x128,
		/// <summary>256-bit processor</summary>
		/// <remarks>http://en.wikipedia.org/wiki/256-bit</remarks>
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x256,
#endif
		#endregion

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	/// <summary>Supported sizes of a processor's logical word</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum ProcessorWordSize : byte
	{
		X8,
		X16,
		X32,
		X64,

		#region Reserved
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED4,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED5,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED6,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED7,

#if false
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x128,
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x256,
#endif
		#endregion

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	/// <summary>Supported processor instruction set types</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum InstructionSet : byte
	{
		/// <summary>Intel based</summary>
		INTEL,
		/// <summary>PowerPC based</summary>
		PPC,

		/// <summary>Common Intermediate Language (.NET)</summary>
		/// <remarks>http://en.wikipedia.org/wiki/Common_Intermediate_Language</remarks>
		CIL,

		/// <summary></summary>
		/// <remarks>http://en.wikipedia.org/wiki/ARM_architecture
		/// Used for:
		///  * iOS (iPhone OS - http://en.wikipedia.org/wiki/IPhone_OS)
		///  * MS Windows Phone
		///  * MS Zune
		///  * MS Windows CE
		/// </remarks>
		ARM,

		/// <summary>Microprocessor without Interlocked Pipeline Stages</summary>
		/// <remarks>http://en.wikipedia.org/wiki/MIPS_architecture</remarks>
		[Obsolete(KSoftConstants.K_UNSUPPORTED_MSG)]
		MIPS,

		#region Reserved
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED5,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED6,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED7,
		#endregion

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};


	/// <summary>Supported Operating System platform types</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum PlatformType : uint
	{
		/// <summary>Undefined platform!</summary>
		UNDEFINED,

		/// <summary>Microsoft Windows</summary>
		WINDOWS,

		/// <summary>Linux based</summary>
		LINUX,
		ANDROID,

		/// <summary>Apple's Macintosh</summary>
		MAC,
		/// <summary>Apple's iPod (a.k.a iPhone OS)</summary>
		/// <remarks>Currently unsupported</remarks>
		I_OS,

		/// <summary>Nintendo non-hand held (e.g., Wii)</summary>
		/// <remarks>Currently unsupported</remarks>
		NINTENDO_CONSOLE,

		/// <summary>Sony's Playstation</summary>
		/// <remarks>Currently unsupported</remarks>
		PLAYSTATION,

		/// <summary>Microsoft's Xbox (Original, 360, Durango)</summary>
		XBOX,

		#region Reserved
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED9,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED10,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED11,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED12,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED13,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED14,
		[Obsolete(KSoftConstants.K_RESERVED_MSG)] Z_UNUSED15,
		#endregion

		/// <remarks>4 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};
}