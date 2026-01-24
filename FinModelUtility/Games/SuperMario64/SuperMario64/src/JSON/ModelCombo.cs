namespace sm64.JSON {
  class ModelCombo {
    byte modelId_ = 0;
    uint segmentAddress_ = 0;

    public byte ModelId {
      get { return this.modelId_; }
    }

    public uint SegmentAddress {
      get { return this.segmentAddress_; }
    }

    public ModelCombo(byte modelId, uint segmentAddress) {
      this.modelId_ = modelId;
      this.segmentAddress_ = segmentAddress;
    }

    public override string ToString() {
      return "[0x" +
             this.modelId_.ToString("X2") + ", 0x" +
             this.segmentAddress_.ToString("X8") + "]";
    }
  }
}