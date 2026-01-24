
namespace KSoft.Phoenix.HaloWars
{
	public enum GameVersionType
	{
		DEFINITIVE_EDITION,
		XBOX360,
	};

	public enum DefinitiveEditionSku
	{
		UNDEFINED,

		STEAM,
		WINDOWS_STORE,
	};
}

namespace KSoft.Phoenix
{
	public static partial class TypeExtensionsPhx
	{
		public static string GetModManifestPath(this HaloWars.DefinitiveEditionSku sku)
		{
			var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			if (localAppData.IsNullOrEmpty())
				return null;

			string skuSubdir = null;
			switch (sku)
			{
				case HaloWars.DefinitiveEditionSku.STEAM:
					skuSubdir = @"Halo Wars\";
					break;

				case HaloWars.DefinitiveEditionSku.WINDOWS_STORE:
					skuSubdir = @"Packages\Microsoft.BulldogThreshold_8wekyb3d8bbwe\LocalState\";
					break;

				default:
					return null;
			}

			string modmanifestFile = skuSubdir + "ModManifest.txt";
			string path = System.IO.Path.Combine(localAppData, modmanifestFile);

			return path;
		}
	};
}