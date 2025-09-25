using f3dzex2.displaylist.opcodes;


namespace f3dzex2.image;

public static class TmemExtensions {
  public static void SetImage(
      this ITmem tmem,
      uint imageSegmentedAddress,
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      ushort width,
      ushort height,
      F3dWrapMode wrapModeS,
      F3dWrapMode wrapModeT,
      uint tileDescriptorIndex = 0) {
    --width;
    --height;

    tmem.SetImage(
        imageSegmentedAddress,
        colorFormat,
        bitsPerTexel,
        width,
        0,
        0,
        width,
        height,
        wrapModeS,
        wrapModeT,
        tileDescriptorIndex);
  }

  public static void SetImage(
      this ITmem tmem,
      uint imageSegmentedAddress,
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      ushort fullWidth,
      ushort uls,
      ushort ult,
      ushort lrs,
      ushort lrt,
      F3dWrapMode wrapModeS,
      F3dWrapMode wrapModeT,
      uint tileDescriptorIndex = 0) {
    var width = (ushort) (lrs - uls);
    var height = (ushort) (lrt - ult);

    tmem.GsSpTexture(1,
                     1,
                     0,
                     TileDescriptorIndex.TX_LOADTILE,
                     TileDescriptorState.ENABLED);

    tmem.GsDpSetTextureImage(colorFormat,
                             bitsPerTexel,
                             fullWidth,
                             imageSegmentedAddress);
    tmem.GsDpSetTile(colorFormat,
                     bitsPerTexel,
                     0,
                     tileDescriptorIndex,
                     TileDescriptorIndex.TX_LOADTILE,
                     0,
                     wrapModeS,
                     0,
                     0,
                     wrapModeT,
                     0,
                     0);
    if (uls == 0 && ult == 0) {
      tmem.GsDpLoadBlock(0,
                         0,
                         TileDescriptorIndex.TX_LOADTILE,
                         (ushort) (width * height),
                         2048);
    } else {
      tmem.GsDpLoadTile(
          TileDescriptorIndex.TX_LOADTILE,
          uls,
          ult,
          lrs,
          lrt);
    }

    tmem.GsDpSetTile(colorFormat,
                     bitsPerTexel,
                     0,
                     tileDescriptorIndex,
                     (TileDescriptorIndex) tileDescriptorIndex,
                     0,
                     wrapModeS,
                     0,
                     0,
                     wrapModeT,
                     0,
                     0);
    tmem.GsDpSetTileSize(uls,
                         ult,
                         (TileDescriptorIndex) tileDescriptorIndex,
                         lrs,
                         lrt);
  }
}