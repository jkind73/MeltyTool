using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using sm64.JSON;
using sm64.LevelInfo;
using sm64.Scripts;

namespace sm64 {
  public sealed class Object3D {
    public enum Flags {
      POSITION_X = 0x1,
      POSITION_Y = 0x2,
      POSITION_Z = 0x4,
      ROTATION_X = 0x8,
      ROTATION_Y = 0x10,
      ROTATION_Z = 0x20,
      ACT1 = 0x40,
      ACT2 = 0x80,
      ACT3 = 0x100,
      ACT4 = 0x200,
      ACT5 = 0x400,
      ACT6 = 0x800,
      ALLACTS = 0x1000,
      BPARAM_1 = 0x2000,
      BPARAM_2 = 0x4000,
      BPARAM_3 = 0x8000,
      BPARAM_4 = 0x10000,
      ALLFLAGS = 0x1FFFF
    }

    public enum FromLsCmd {
      CMD_24,
      CMD_39,
      CMD_2_E_8,
      CMD_2_E_10,
      CMD_2_E_12
    }

    private const ushort NUM_OF_CATERGORIES_ = 7;

    bool isBehaviorReadOnly_ = false;
    bool isModelIdReadOnly_ = false;
    bool isTempHidden_ = false;

    public Object3D() {
      this.mData_ = new ObjectData();
    }

    public Object3D(
        string address,
        ObjectData objectData
    ) {
      this.mData_ = objectData;
      this.Address = address;
      this.UpdateProperties();
    }

    public void ReplaceData(ObjectData newData) {
      this.mData_ = newData;
      this.UpdateProperties();
    }

    private ObjectData mData_ = new ObjectData();

    [Browsable(false)]
    public ObjectData Data {
      get => this.mData_;
    }

    [Browsable(false)]
    public bool CanEditModelId {
      get { return !this.isModelIdReadOnly_; }
    }

    [Browsable(false)]
    public bool CanEditBehavior {
      get { return !this.isBehaviorReadOnly_; }
    }

    [Browsable(false)]
    public FromLsCmd CreatedFromLevelScriptCommand { get; set; }

    [CustomSortedCategory("Info", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [Description("Name of the object combo")]
    [DisplayName("Combo Name")]
    [ReadOnly(true)]
    public string Title { get; set; }

    [CustomSortedCategory("Info", 1, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [Description("Location inside the ROM file")]
    [DisplayName("Address")]
    [ReadOnly(true)]
    public string Address { get; set; }

    [CustomSortedCategory("Model", 2, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [Description("Model identifer used by the object")]
    [DisplayName("Model ID")]
    public byte ModelId {
      get { return this.mData_.ModelId; }
      set { this.mData_.ModelId = value; }
    }

    [CustomSortedCategory("Model", 2, NUM_OF_CATERGORIES_)]
    [Browsable(false)]
    [Description("Model identifer used by the object")]
    [DisplayName("Model ID")]
    [ReadOnly(true)]
    public byte ModelIdReadOnly {
      get { return this.mData_.ModelId; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("X")]
    public short XPos {
      get { return this.mData_.X; }
      set { this.mData_.X = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Y")]
    public short YPos {
      get { return this.mData_.Y; }
      set { this.mData_.Y = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Z")]
    public short ZPos {
      get { return this.mData_.Z; }
      set { this.mData_.Z = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("RX")]
    public short XRot {
      get { return this.mData_.Rx; }
      set { this.mData_.Rx = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("RY")]
    public short YRot {
      get { return this.mData_.Ry; }
      set { this.mData_.Ry = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("RZ")]
    public short ZRot {
      get { return this.mData_.Rz; }
      set { this.mData_.Rz = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Behavior")]
    public string Behavior {
      get => "0x" + this.mData_.Behaviour.ToString("X8");
      set {
        this.mData_.Behaviour =
            uint.Parse(value[2..], NumberStyles.HexNumber);
      }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(false)]
    [DisplayName("Behavior")]
    [ReadOnly(true)]
    public string BehaviorReadOnly => this.Behavior;

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Beh. Name")]
    [ReadOnly(true)]
    public string BehaviorName => Globals
                                   .GetBehaviorNameEntryFromSegAddress(this.mData_.Behaviour)
                                   .Name;

    public new string ToString() => this.BehaviorName;

    // default names
    private const string BP_1DNAME_ = "B.Param 1";
    private const string BP_2DNAME_ = "B.Param 2";
    private const string BP_3DNAME_ = "B.Param 3";
    private const string BP_4DNAME_ = "B.Param 4";

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName(BP_1DNAME_)]
    [Description("")]
    public byte BehaviorParameter1 {
      get { return this.mData_.BehaviourArgs[0]; }
      set { this.mData_.BehaviourArgs[0] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName(BP_2DNAME_)]
    [Description("")]
    public byte BehaviorParameter2 {
      get { return this.mData_.BehaviourArgs[1]; }
      set { this.mData_.BehaviourArgs[1] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName(BP_3DNAME_)]
    [Description("")]
    public byte BehaviorParameter3 {
      get { return this.mData_.BehaviourArgs[2]; }
      set { this.mData_.BehaviourArgs[2] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName(BP_4DNAME_)]
    [Description("")]
    public byte BehaviorParameter4 {
      get { return this.mData_.BehaviourArgs[3]; }
      set { this.mData_.BehaviourArgs[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("All Acts")]
    public bool AllActs {
      get { return this.mData_.AllActs; }
      set { this.mData_.AllActs = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 1")]
    public bool Act1 {
      get { return this.mData_.Acts[0]; }
      set { this.mData_.Acts[0] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 2")]
    public bool Act2 {
      get { return this.mData_.Acts[1]; }
      set { this.mData_.Acts[1] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 3")]
    public bool Act3 {
      get { return this.mData_.Acts[2]; }
      set { this.mData_.Acts[2] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 4")]
    public bool Act4 {
      get { return this.mData_.Acts[3]; }
      set { this.mData_.Acts[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 5")]
    public bool Act5 {
      get { return this.mData_.Acts[4]; }
      set { this.mData_.Acts[4] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES_)]
    [Browsable(true)]
    [DisplayName("Act 6")]
    public bool Act6 {
      get { return this.mData_.Acts[5]; }
      set { this.mData_.Acts[5] = value; }
    }

    private ulong flags_ = 0;

    private bool isReadOnly_ = false;

    [CustomSortedCategory("Misc", NUM_OF_CATERGORIES_, NUM_OF_CATERGORIES_)]
    [DisplayName("Read-Only")]
    [Browsable(false)]
    public bool IsReadOnly {
      get { return this.isReadOnly_; }
    }

    /**************************************************************************************/

    [Browsable(false)]
    public Level Level { get; set; }

    private ObjectComboEntry objectComboEntry_;
    private ushort presetId_;

    public int GetRomAddress() {
      return int.Parse(this.Address[2..], NumberStyles.HexNumber);
    }

    public uint GetRomUnsignedAddress() {
      return uint.Parse(this.Address[2..], NumberStyles.HexNumber);
    }

    public void SetPresetId(ushort id) {
      this.presetId_ = id;
    }

    public byte GetActMask() {
      byte actMask = 0;
      if (this.Act1) actMask |= 0x1;
      if (this.Act2) actMask |= 0x2;
      if (this.Act3) actMask |= 0x4;
      if (this.Act4) actMask |= 0x8;
      if (this.Act5) actMask |= 0x10;
      if (this.Act6) actMask |= 0x20;
      return actMask;
    }

    public void SetBehaviorFromAddress(uint address) {
      this.mData_.Behaviour = address;
    }

    public uint GetBehaviorAddress() => this.mData_.Behaviour;

    public List<ScriptDumpCommandInfo> ParseBehavior() {
      var script = new List<ScriptDumpCommandInfo>();
      BehaviorScripts.Parse(ref script, this.GetBehaviorAddress());
      return script;
    }

    public void MakeBehaviorReadOnly(bool isReadOnly) {
      this.isBehaviorReadOnly_ = isReadOnly;
    }

    public void MakeModelIDReadOnly(bool isReadOnly) {
      this.isModelIdReadOnly_ = isReadOnly;
    }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
      this.isReadOnly_ = true;
    }

    private void HideShowProperty_(string property, bool show) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      BrowsableAttribute attrib =
          (BrowsableAttribute) descriptor.Attributes[
              typeof(BrowsableAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("browsable",
                          BindingFlags.NonPublic | BindingFlags.Instance);

      if (isBrow != null)
        isBrow.SetValue(attrib, show);
    }

    private bool IsHidden_(Flags flag) {
      return (this.flags_ & (ulong) flag) == (ulong) flag;
    }

    private void UpdateProperty_(string property, Flags flag) {
      if (this.IsHidden_(flag))
        this.HideShowProperty_(property, false);
      else
        this.HideShowProperty_(property, true);
    }

    private void updateReadOnlyProperty(string property, bool isReadOnly) {
      if (isReadOnly) {
        this.HideShowProperty_(property, false);
        this.HideShowProperty_(property + "_ReadOnly", true);
      } else {
        this.HideShowProperty_(property, true);
        this.HideShowProperty_(property + "_ReadOnly", false);
      }
    }

    private void ChangePropertyName_(string property, string name) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DisplayNameAttribute attrib =
          (DisplayNameAttribute) descriptor.Attributes[
              typeof(DisplayNameAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("_displayName",
                          BindingFlags.NonPublic | BindingFlags.Instance);

      if (isBrow != null)
        isBrow.SetValue(attrib, name);
    }

    private string? GetPropertyDisplayName_(string property) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DisplayNameAttribute attrib =
          (DisplayNameAttribute) descriptor.Attributes[
              typeof(DisplayNameAttribute)];

      return (string?) attrib.GetType()
                   .GetField("_displayName",
                             BindingFlags.NonPublic |
                             BindingFlags.Instance)
                   ?.GetValue(attrib);
    }

    private void
        ChangePropertyDescription_(string property, string description) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DescriptionAttribute attrib =
          (DescriptionAttribute) descriptor.Attributes[
              typeof(DescriptionAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("description",
                          BindingFlags.NonPublic | BindingFlags.Instance);
      if (isBrow != null)
        isBrow.SetValue(attrib, description);
    }

    private void UpdatePropertyName_(string property,
                                    string oceName,
                                    string otherName) {
      if (oceName != null && !oceName.Equals(""))
        this.ChangePropertyName_(property, oceName);
      else
        this.ChangePropertyName_(property, otherName);
    }

    private void UpdatePropertyDescription_(string property, string oceDesc) {
      if (oceDesc != null && !oceDesc.Equals(""))
        this.ChangePropertyDescription_(property, oceDesc);
      else
        this.ChangePropertyDescription_(property, "");
    }

    private void UpdateObjectComboNames_() {
      if (this.objectComboEntry_ != null) {
        this.UpdatePropertyName_("BehaviorParameter1",
                                this.objectComboEntry_.Bp1Name,
                                BP_1DNAME_);
        this.UpdatePropertyName_("BehaviorParameter2",
                                this.objectComboEntry_.Bp2Name,
                                BP_2DNAME_);
        this.UpdatePropertyName_("BehaviorParameter3",
                                this.objectComboEntry_.Bp3Name,
                                BP_3DNAME_);
        this.UpdatePropertyName_("BehaviorParameter4",
                                this.objectComboEntry_.Bp4Name,
                                BP_4DNAME_);
        this.UpdatePropertyDescription_("BehaviorParameter1",
                                       this.objectComboEntry_.Bp1Description);
        this.UpdatePropertyDescription_("BehaviorParameter2",
                                       this.objectComboEntry_.Bp2Description);
        this.UpdatePropertyDescription_("BehaviorParameter3",
                                       this.objectComboEntry_.Bp3Description);
        this.UpdatePropertyDescription_("BehaviorParameter4",
                                       this.objectComboEntry_.Bp4Description);
      } else {
        this.ChangePropertyName_("BehaviorParameter1", BP_1DNAME_);
        this.ChangePropertyName_("BehaviorParameter2", BP_2DNAME_);
        this.ChangePropertyName_("BehaviorParameter3", BP_3DNAME_);
        this.ChangePropertyName_("BehaviorParameter4", BP_4DNAME_);
        this.ChangePropertyDescription_("BehaviorParameter1", "");
        this.ChangePropertyDescription_("BehaviorParameter2", "");
        this.ChangePropertyDescription_("BehaviorParameter3", "");
        this.ChangePropertyDescription_("BehaviorParameter4", "");
      }
    }

    public void UpdateProperties() {
      this.UpdateProperty_("xPos", Flags.POSITION_X);
      this.UpdateProperty_("yPos", Flags.POSITION_Y);
      this.UpdateProperty_("zPos", Flags.POSITION_Z);
      this.UpdateProperty_("xRot", Flags.ROTATION_X);
      this.UpdateProperty_("yRot", Flags.ROTATION_Y);
      this.UpdateProperty_("zRot", Flags.ROTATION_Z);
      this.UpdateProperty_("Act1", Flags.ACT1);
      this.UpdateProperty_("Act2", Flags.ACT2);
      this.UpdateProperty_("Act3", Flags.ACT3);
      this.UpdateProperty_("Act4", Flags.ACT4);
      this.UpdateProperty_("Act5", Flags.ACT5);
      this.UpdateProperty_("Act6", Flags.ACT6);
      this.UpdateProperty_("AllActs", Flags.ALLACTS);
      this.UpdateProperty_("BehaviorParameter1", Flags.BPARAM_1);
      this.UpdateProperty_("BehaviorParameter2", Flags.BPARAM_2);
      this.UpdateProperty_("BehaviorParameter3", Flags.BPARAM_3);
      this.UpdateProperty_("BehaviorParameter4", Flags.BPARAM_4);
      this.updateReadOnlyProperty("Behavior", this.isBehaviorReadOnly_);
      this.updateReadOnlyProperty("ModelID", this.isModelIdReadOnly_);
      this.UpdateObjectComboNames_();
    }

    Flags tempHideFlags_;

    bool isBehaviorReadOnlyTempTrigger_ = false,
         isModelIdReadOnlyTempTrigger_ = false;

    public void HideFieldsTemporarly(Flags showFlags) {
      this.tempHideFlags_ = (~showFlags & ~(Flags) this.flags_) & Flags.ALLFLAGS;
      //Console.WriteLine(Convert.ToString((int)Flags, 2).PadLeft(32, '0'));
      //Console.WriteLine(Convert.ToString((int)tempHideFlags, 2).PadLeft(32, '0'));

      this.isTempHidden_ = true;
      if (!this.isBehaviorReadOnly_) {
        this.isBehaviorReadOnlyTempTrigger_ = true;
        this.isBehaviorReadOnly_ = true;
      }
      if (!this.isModelIdReadOnly_) {
        this.isModelIdReadOnlyTempTrigger_ = true;
        this.isModelIdReadOnly_ = true;
      }

      this.HideProperty(this.tempHideFlags_);
      this.UpdateProperties();
    }

    public void RevealTemporaryHiddenFields() {
      if (this.isTempHidden_) {
        if (this.isBehaviorReadOnlyTempTrigger_) {
          this.isBehaviorReadOnlyTempTrigger_ = false;
          this.isBehaviorReadOnly_ = false;
        }
        if (this.isModelIdReadOnlyTempTrigger_) {
          this.isModelIdReadOnlyTempTrigger_ = false;
          this.isModelIdReadOnly_ = false;
        }

        this.UnhideProperty(this.tempHideFlags_);
        this.UpdateProperties();
        this.isTempHidden_ = false;
        this.tempHideFlags_ = 0;
      }
    }

    public Flags GetFlagFromDisplayName(string? displayName) {
      if (displayName == this.GetPropertyDisplayName_("xPos"))
        return Flags.POSITION_X;
      if (displayName == this.GetPropertyDisplayName_("yPos"))
        return Flags.POSITION_Y;
      if (displayName == this.GetPropertyDisplayName_("zPos"))
        return Flags.POSITION_Z;
      if (displayName == this.GetPropertyDisplayName_("xRot"))
        return Flags.ROTATION_X;
      if (displayName == this.GetPropertyDisplayName_("yRot"))
        return Flags.ROTATION_Y;
      if (displayName == this.GetPropertyDisplayName_("zRot"))
        return Flags.ROTATION_Z;
      if (displayName == this.GetPropertyDisplayName_("Act1")) return Flags.ACT1;
      if (displayName == this.GetPropertyDisplayName_("Act2")) return Flags.ACT2;
      if (displayName == this.GetPropertyDisplayName_("Act3")) return Flags.ACT3;
      if (displayName == this.GetPropertyDisplayName_("Act4")) return Flags.ACT4;
      if (displayName == this.GetPropertyDisplayName_("Act5")) return Flags.ACT5;
      if (displayName == this.GetPropertyDisplayName_("Act6")) return Flags.ACT6;
      if (displayName == this.GetPropertyDisplayName_("AllActs"))
        return Flags.ALLACTS;
      if (displayName == this.GetPropertyDisplayName_("BehaviorParameter1"))
        return Flags.BPARAM_1;
      if (displayName == this.GetPropertyDisplayName_("BehaviorParameter2"))
        return Flags.BPARAM_2;
      if (displayName == this.GetPropertyDisplayName_("BehaviorParameter3"))
        return Flags.BPARAM_3;
      if (displayName == this.GetPropertyDisplayName_("BehaviorParameter4"))
        return Flags.BPARAM_4;
      return 0;
    }

    public void SetBehaviorParametersToZero() {
      this.BehaviorParameter1 = 0;
      this.BehaviorParameter2 = 0;
      this.BehaviorParameter3 = 0;
      this.BehaviorParameter4 = 0;
    }

    public void DontShowActs() {
      this.flags_ |= (ulong) (
                         Flags.ACT1 | Flags.ACT2 | Flags.ACT3 |
                         Flags.ACT4 | Flags.ACT5 | Flags.ACT6 |
                         Flags.ALLACTS);
    }

    public void ShowHideActs(bool hide) {
      if (hide)
        this.flags_ |= (ulong) (Flags.ACT1 | Flags.ACT2 |
                               Flags.ACT3 | Flags.ACT4 | Flags.ACT5 | Flags.ACT6);
      else
        this.flags_ &= ~(ulong) (Flags.ACT1 | Flags.ACT2 |
                                Flags.ACT3 | Flags.ACT4 | Flags.ACT5 | Flags.ACT6);
      this.UpdateProperties();
    }

    public void HideProperty(Flags flag) {
      this.flags_ |= (ulong) flag;
    }

    public void UnhideProperty(Flags flag) {
      this.flags_ &= ~(ulong) flag;
    }

    public void RenameObjectCombo(string newName) {
      string oldComboName = this.Title;
      this.Title = newName;
      bool undefinedToDefined = oldComboName.StartsWith("Undefined Combo (")
                                && !newName.StartsWith("Undefined Combo (");

      if (!undefinedToDefined) // simple re-define
      {
        if (this.objectComboEntry_ != null)
          this.objectComboEntry_.Name = newName;
      } else {
        uint modelAddress = 0;
        ObjectComboEntry newOce =
            new ObjectComboEntry(newName,
                                 this.ModelId, modelAddress,
                                 this.GetBehaviorAddress());
        this.objectComboEntry_ = newOce;
        Globals.objectComboEntries.Add(newOce);
        this.Level.levelObjectCombos.Add(newOce);
      }

      ModelComboFile.WriteObjectCombosFile(Globals.GetDefaultObjectComboPath());
    }

    public string GetObjectComboName() {
      uint behaviorAddr = this.GetBehaviorAddress();
      uint modelSegmentAddress = 0;
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry entry = Globals.objectComboEntries[i];
        modelSegmentAddress = 0;
        if (entry.ModelId == this.ModelId && entry.Behavior == behaviorAddr
                                          && entry.ModelSegmentAddress ==
                                          modelSegmentAddress) {
          this.objectComboEntry_ = entry;
          this.Title = entry.Name;
          return entry.Name;
        }
      }

      this.objectComboEntry_ = null;
      this.Title = "Undefined Combo (0x" +
                   this.mData_.ModelId.ToString("X2") + ", 0x" +
                   behaviorAddr.ToString("X8") + ")";
      return this.Title;
    }

    public bool IsPropertyShown(Flags flag) {
      return !this.IsHidden_(flag);
    }
  }
}