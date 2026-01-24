
namespace KSoft.Wwise
{
	public static class AkVersion
	{
		static class K2006
		{
			//2006.2
			//2006.3
			//2006.3.1
		};
		public static class K2007
		{
			public const uint ID = 0x20070000;

			//2007.1
			//2007.1.1
			//2007.2.1
			//2007.3
			//2007.4

			// Based on the changelog for 2007.4, it looks like this is from an earlier build
			public const uint BANK_GENERATOR = 0x1A; // HaloWars alpha2 build
		};
		public static class K2008
		{
			public const uint ID = 0x20080000;

			//2008.1.1
			//2008.1.2
			//2008.2.1
			//2008.3
			//2008.4
			public const uint BANK_GENERATOR = 0x22; //34 HaloWars retail
		};
		public static class K2009
		{
			public const uint ID = 0x20090000;

			//2009.1
			public const uint BANK_GENERATOR_2 = 0x2D; //45 3260
			//2009.2.1
			//2009.3
		};
		public static class K2010
		{
			//2010.1
			//2010.1.1
			public const uint BANK_GENERATOR_1_2 = 0x30; //48 3554
			//2010.1.3
			//2010.2
			//2010.3
			//2010.3.1
			//2010.3.2
			public const uint BANK_GENERATOR_3_3 = 0x35; //53 3773
		};
		public static class K2011
		{
			public const uint ID = 0x20110000;
			public const uint _2_Id = ID | 0x0200; // external files and word-size dependent LUT
			//2011.1
			//2011.1.1
			public const uint BANK_GENERATOR_1_2 = 0x38; //56 3891
			//2011.2
			public const uint BANK_GENERATOR_2_1 = 0x3E; //62 4004
			public const uint BANK_GENERATOR_2_2 = 0x3E; //62 4007
			//2011.3
			//2011.3.1
		};
		public static class K2012
		{
			public const uint ID = 0x20120000;

			//2012.1
			//2012.1.1
			//2012.1.2
			//2012.1.3
			//2012.1.4
			//2012.2
			//2012.2.1
			public const uint K_BANK_GENERATOR_2_2 = 0x48; //72 4430
		};

		// Based on HaloWars's alpha2 build
		internal static bool BankHasOldStid(uint generatorVersion) { return generatorVersion <= K2007.BANK_GENERATOR; }
		internal static bool HasOldBankHeader(uint sdkVersion) { return sdkVersion < K2008.ID; }

		internal static bool HasExternalFiles(uint sdkVersion) { return sdkVersion >= K2011._2_Id; }
		internal static bool HasWordSizeDependentLut(uint sdkVersion) { return sdkVersion >= K2011._2_Id; }

		// TODO: verify when this was added
		internal static bool HircTypeIs8Bit(uint sdkVersion) { return sdkVersion >= K2012.ID; }
	};
}