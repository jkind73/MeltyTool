using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	// Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
	public enum TexMetadataMiscFlags : uint
	{
		TEXTURE_CUBE = 4,
	};

	// Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
	public enum TextureAlphaMode : uint
	{
		UNKNOWN,
		STRAIGHT,
		PREMULTIPLIED,
		OPAQUE,
		CUSTOM,
	};
	// Subset here matches D3D10_RESOURCE_DIMENSION and D3D11_RESOURCE_DIMENSION
	public enum TextureDimension : uint
	{
		_1D = 2,
		_2D = 3,
		_3D = 4,
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct TexMetadata
	{
		public const uint K_ALPHA_MODE_MASK = 0x7;

		public uint Width;
		public uint Height;
		public uint Depth;
		public uint ArraySize;
		public uint MipLevels;
		public TexMetadataMiscFlags MiscFlags;
		public uint MiscFlags2;
		public DxgiFormat Format;
		public TextureDimension Dimension;

		public static TexMetadata Empty { get { return new TexMetadata(); } }
	};
}
