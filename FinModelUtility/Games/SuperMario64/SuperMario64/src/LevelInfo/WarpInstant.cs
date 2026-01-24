using System.ComponentModel;

namespace sm64.LevelInfo {
  public sealed class WarpInstant {
    private const ushort NUM_OF_CATERGORIES_ = 2;

    private byte triggerId_;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Trigger ID")]
    public byte TriggerId {
      get { return this.triggerId_; }
      set { this.triggerId_ = value; }
    }

    private byte areaId_;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("To Area")]
    public byte AreaId {
      get { return this.areaId_; }
      set { this.areaId_ = value; }
    }

    private short teleX_;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Teleport X")]
    public short TeleX {
      get { return this.teleX_; }
      set { this.teleX_ = value; }
    }

    private short teleY_;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Teleport Y")]
    public short TeleY {
      get { return this.teleY_; }
      set { this.teleY_ = value; }
    }

    private short teleZ_;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Teleport Z")]
    public short TeleZ {
      get { return this.teleZ_; }
      set { this.teleZ_ = value; }
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


    private string GetWarpName_() {
      return " [to Area " + this.AreaId + "]";
    }

    public override string ToString() {
      //isPaintingWarp
      string warpName = "Instant Warp 0x";

      warpName += this.TriggerId.ToString("X2") + this.GetWarpName_();

      return warpName;
    }
  }
}