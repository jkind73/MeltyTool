namespace sm64.JSON {
  public sealed class ObjectComboEntry {
    string name_ = "";
    string behaviorName_ = "";
    ModelCombo modelCombo_;
    uint behavior_ = 0;

    public string Name {
      get { return this.name_; }
      set { this.name_ = value; }
    }

    public string BehaviorName {
      get { return this.behaviorName_; }
      set { this.behaviorName_ = value; }
    }

    public byte ModelId {
      get { return this.modelCombo_.ModelId; }
    }

    public uint ModelSegmentAddress {
      get { return this.modelCombo_.SegmentAddress; }
    }

    public uint Behavior {
      get { return this.behavior_; }
    }

    public ObjectComboEntry(string name,
                            byte modelId,
                            uint modelSegAddress,
                            uint behavior) {
      this.name_ = name;
      this.behavior_ = behavior;
      this.behaviorName_ = Globals.GetBehaviorNameEntryFromSegAddress(behavior).Name;
      this.modelCombo_ = new ModelCombo(modelId, modelSegAddress);
    }

    private string bp1_, bp2_, bp3_, bp4_;

    public string Bp1Name {
      get { return this.bp1_; }
      set { this.bp1_ = value; }
    }

    public string Bp2Name {
      get { return this.bp2_; }
      set { this.bp2_ = value; }
    }

    public string Bp3Name {
      get { return this.bp3_; }
      set { this.bp3_ = value; }
    }

    public string Bp4Name {
      get { return this.bp4_; }
      set { this.bp4_ = value; }
    }

    private string bp1Desc_, bp2Desc_, bp3Desc_, bp4Desc_;

    public string Bp1Description {
      get { return this.bp1Desc_; }
      set { this.bp1Desc_ = value; }
    }

    public string Bp2Description {
      get { return this.bp2Desc_; }
      set { this.bp2Desc_ = value; }
    }

    public string Bp3Description {
      get { return this.bp3Desc_; }
      set { this.bp3Desc_ = value; }
    }

    public string Bp4Description {
      get { return this.bp4Desc_; }
      set { this.bp4Desc_ = value; }
    }

    public override string ToString() {
      return this.name_ + " = [" +
             this.modelCombo_.ToString() + "," +
             this.behaviorName_ +
             " (0x" +
             this.behavior_.ToString("X8") + ")]";
    }
  }
}