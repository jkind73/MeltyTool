using f3dzex2.image;

using sm64.JSON;
using sm64.LevelInfo;
using sm64.memory;
using sm64.Scripts;

namespace sm64.api;

public static class Sm64LevelImporter {
  public static Level LoadLevel(Sm64LevelSceneFileBundle levelFileBundle) {
    Rom rom = Rom.Instance;

    rom.ClearSegments();
    rom.ReadFile(levelFileBundle.Sm64Rom.FullPath);

    Globals.objectComboEntries.Clear();
    Globals.behaviorNameEntries.Clear();
    BehaviorNameFile.ParseBehaviorNames(
        Globals.GetDefaultBehaviorNamesPath());
    ModelComboFile.ParseObjectCombos(Globals.GetDefaultObjectComboPath());
    var memoryConstants = Globals.MemoryConstants;
    rom.SetSegment(0x15,
                   memoryConstants.Segment15.Offset,
                   memoryConstants.Segment15.Offset + memoryConstants.Segment15.Length,
                   false,
                   null);
    rom.SetSegment(0x02,
                   memoryConstants.Segment2.Offset,
                   memoryConstants.Segment2.Offset + memoryConstants.Segment2.Length,
                   rom.IsSegmentMio0(0x02, null),
                   rom.Seg02IsFakeMio0,
                   rom.Seg02UncompressedOffset,
                   null);

    var sm64Hardware = new N64Hardware<ISm64Memory>();
    sm64Hardware.Memory = new Sm64Memory();
    sm64Hardware.Rdp = new Rdp { Tmem = new NoclipTmem(sm64Hardware) };
    sm64Hardware.Rsp = new Rsp();

    var level = new Level((ushort) levelFileBundle.LevelId, 1);
    LevelScripts.Parse(sm64Hardware, ref level, 0x15, 0);
    level.SortAndAddNoModelEntries();
    level.CurrentAreaId = level.areas[0].AreaId;

    return level;
  }
}