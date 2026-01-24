using System.ComponentModel;

namespace sm64.LevelInfo {
  public sealed class Warp {
    public Warp(bool isPaintingWarp) {
      this.isPaintingWarp_ = isPaintingWarp;
    }

    private const ushort NUM_OF_CATERGORIES_ = 2;
    private bool isPaintingWarp_ = false;

    private byte warpFromId_;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("From ID")]
    public byte WarpFromId {
      get { return this.warpFromId_; }
      set { this.warpFromId_ = value; }
    }

    private byte warpToLevelId_;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("To Level")]
    public byte WarpToLevelId {
      get { return this.warpToLevelId_; }
      set { this.warpToLevelId_ = value; }
    }

    private byte warpToAreaId_;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("To Area")]
    public byte WarpToAreaId {
      get { return this.warpToAreaId_; }
      set { this.warpToAreaId_ = value; }
    }

    private byte warpToWarpId_;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("To ID")]
    public byte WarpToWarpId {
      get { return this.warpToWarpId_; }
      set { this.warpToWarpId_ = value; }
    }

    [CustomSortedCategory("Info", 2, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [Description("Location inside the ROM file")]
    [DisplayName("Address")]
    [ReadOnly(true)]
    public string Address { get; set; }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
    }

    private string GetLevelName_() {
      Rom rom = Rom.Instance;
      foreach (KeyValuePair<string, ushort> entry in rom.levelIDs) {
        if (entry.Value == this.WarpToLevelId)
          return entry.Key + " (" + this.warpToAreaId_ + ")";
      }
      return "Unknown" + " (" + this.warpToAreaId_ + ")";
    }

    private string GetWarpName_() {
      if (this.isPaintingWarp_) {
        return " [to " + this.GetLevelName_() + "]";
      } else {
        switch (this.WarpFromId) {
          case 0xF0:
            return " (Success)" + " [to " + this.GetLevelName_() + "]";
          case 0xF1:
            return " (Failure)" + " [to " + this.GetLevelName_() + "]";
          case 0xF2:
          case 0xF3:
            return " (Special)" + " [to " + this.GetLevelName_() + "]";
          default:
            return " [to " + this.GetLevelName_() + "]";
        }
      }
    }

    public override string ToString() {
      //isPaintingWarp
      string warpName = "Warp 0x";
      if (this.isPaintingWarp_)
        warpName = "Painting 0x";

      warpName += this.WarpFromId.ToString("X2") + this.GetWarpName_();

      return warpName;
    }
  }
}