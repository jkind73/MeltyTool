using System.Drawing;

using f3dzex2.image;

using schema.binary;

using sm64.JSON;
using sm64.memory;
using sm64.schema;
using sm64.Scripts;

namespace sm64.LevelInfo {
  public sealed class AreaBackgroundInfo {
    public uint address = 0;
    public ushort idOrColor = 0;
    public bool isEndCakeImage = false;
    public uint romLocation = 0;
    public bool usesFog = false;
    public Color fogColor = Color.White;
    public List<uint> fogColorRomLocation = [];
  }

  public sealed class Area(
      IN64Hardware<ISm64Memory> sm64Hardware,
      ushort areaId,
      uint geoLayoutPointer,
      Level parent) {
    public Level parent = parent;

    public ushort AreaId {
      get { return areaId; }
    }

    public uint GeometryLayoutPointer {
      get { return geoLayoutPointer; }
    }

    public AreaBackgroundInfo bgInfo = new AreaBackgroundInfo();

    public Model3DLods areaModel = new(sm64Hardware);
    public CollisionMap collision = new CollisionMap();

    public List<Object3D> objects = [];
    public List<Object3D> macroObjects = [];
    public List<Object3D> specialObjects = [];
    public List<Warp> warps = [];
    public List<Warp> paintingWarps = [];
    public List<WarpInstant> instantWarps = [];

    private byte? defaultTerrainType_;

    public byte DefaultTerrainType {
      get => this.defaultTerrainType_ ?? 0;
      set {
        if (this.defaultTerrainType_.HasValue) {
          throw new Exception();
        }
        this.defaultTerrainType_ = value;
      }
    }
  }

  public sealed class Level {
    private ushort levelId_;

    public ushort LevelId {
      get { return this.levelId_; }
    }

    private ushort currentAreaId_;

    public ushort CurrentAreaId {
      get { return this.currentAreaId_; }
      set { this.currentAreaId_ = value; }
    }

    public List<Area> areas = [];
    public AreaBackgroundInfo tempBgInfo = new AreaBackgroundInfo();

    public Dictionary<ushort, Model3DLods> modelIDs =
        new Dictionary<ushort, Model3DLods>();

    public List<ObjectComboEntry> levelObjectCombos =
        [];

    public List<PresetMacroEntry> macroObjectPresets =
        [];

    public List<PresetMacroEntry> specialObjectPresets8 =
        [];

    public List<PresetMacroEntry> specialObjectPresets10 =
        [];

    public List<PresetMacroEntry> specialObjectPresets12 =
        [];

    public List<ScriptDumpCommandInfo> levelScriptCommandsForDump =
        [];


    public ObjectComboEntry? GetObjectComboFromData(
        byte modelId,
        uint modelAddress,
        uint behavior,
        out int index) {
      for (int i = 0; i < this.levelObjectCombos.Count; i++) {
        ObjectComboEntry oce = this.levelObjectCombos[i];
        if (oce.ModelId == modelId && oce.ModelSegmentAddress == modelAddress
                                   && oce.Behavior == behavior) {
          index = i;
          return oce;
        }
      }
      index = -1;
      return null;
    }

    private void AddMacroObjectEntries_() {
      this.macroObjectPresets.Clear();
      Rom rom = Rom.Instance;

      using var br = new SchemaBinaryReader(rom.Bytes);
      br.Position = Globals.MemoryConstants.MacroPresetTable;

      ushort pId = 0x1F;
      for (int i = 0; i < 366; i++) {
        var presetMacroEntry = br.ReadNew<PresetMacroEntry>();
        presetMacroEntry.PresetId = pId++;
        this.macroObjectPresets.Add(presetMacroEntry);
      }
    }

    public void AddSpecialObjectPreset_8(ushort presetId,
                                         byte modelId,
                                         uint behavior) {
      this.specialObjectPresets8.Add(
          new PresetMacroEntry(presetId, modelId, behavior));
    }

    public void AddSpecialObjectPreset_10(ushort presetId,
                                          byte modelId,
                                          uint behavior) {
      this.specialObjectPresets10.Add(
          new PresetMacroEntry(presetId, modelId, behavior));
    }

    public void AddSpecialObjectPreset_12(ushort presetId,
                                          byte modelId,
                                          uint behavior,
                                          byte bp1,
                                          byte bp2) {
      this.specialObjectPresets12.Add(
          new PresetMacroEntry(presetId, modelId, behavior, bp1, bp2));
    }

    public void AddObjectCombos(byte modelId, uint modelSegAddress) {
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry oce = Globals.objectComboEntries[i];
        if (oce.ModelId == modelId &&
            oce.ModelSegmentAddress == modelSegAddress)
          this.levelObjectCombos.Add(oce);
      }
    }

    public void SortAndAddNoModelEntries() {
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry oce = Globals.objectComboEntries[i];
        if (oce.ModelId == 0x00)
          this.levelObjectCombos.Add(oce);
      }

      this.levelObjectCombos.Sort((x, y) => string.Compare(x.Name, y.Name));
    }

    public Area GetCurrentArea() {
      foreach (Area a in this.areas)
        if (a.AreaId == this.currentAreaId_)
          return a;
      return this.areas[0]; // return default area
    }

    public void SetAreaBackgroundInfo(ref Area area) {
      area.bgInfo.address = this.tempBgInfo.address;
      area.bgInfo.idOrColor = this.tempBgInfo.idOrColor;
      area.bgInfo.isEndCakeImage = this.tempBgInfo.isEndCakeImage;
      area.bgInfo.romLocation = this.tempBgInfo.romLocation;
      area.bgInfo.usesFog = this.tempBgInfo.usesFog;
      area.bgInfo.fogColor = this.tempBgInfo.fogColor;
      area.bgInfo.fogColorRomLocation = this.tempBgInfo.fogColorRomLocation;
    }

    public Level(ushort levelId, ushort startArea) {
      Rom.Instance.ClearSegments();
      this.levelId_ = levelId;
      this.currentAreaId_ = startArea;
      this.levelObjectCombos.Clear();
      this.levelScriptCommandsForDump.Clear();
      this.AddMacroObjectEntries_();
    }
  }
}