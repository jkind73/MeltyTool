using sm64.JSON;
using sm64.memory;
using sm64.Scripts;

namespace sm64 {
  public sealed class ScriptDumpCommandInfo {
    public byte[] data;
    public uint romAddress, segAddress;
    public string description;

    public BehaviorCommand Command => (BehaviorCommand) this.data[0];

    public new string ToString() => this.description;
  }

  public sealed class Globals {
    public static bool debugPlg = false; // parsing level geometry flag
    public static uint debugPdl = 0x00000000; // parsing display list value
    public static bool debugParsingLevelArea = false;
    public static bool debugParsingDl = false;

    public static MemoryConstants MemoryConstants { get; set; }

    // RAM to ROM conversion for assembly functions in each region
    public static uint raMtoRomJp = 0x80245000; // Japan
    public static uint raMtoRomNa = 0x80245000; // USA 
    public static uint raMtoRomEu = 0x80240800; // Europe
    public static uint raMtoRomJs = 0x80248000; // Japan (Shindou)
    public static uint raMtoRomIq = 0x80248000; // Chinese (IQue)

    // Function location that initalizes segment 0x02 for each region
    public static uint seg02InitJp = 0x80248934; // Japan
    public static uint seg02InitNa = 0x80248964; // USA 
    public static uint seg02InitEu = 0x80244100; // Europe
    public static uint seg02InitJs = 0x8024B958; // Japan (Shindou)
    public static uint seg02InitIq = 0x8024B968; // Chinese (IQue)


    // Function that draws skybox image:
    public static uint skyboxDrawFuncNa = 0x802763D4;

    public static uint GetSkyboxDrawFunction() {
      switch (Rom.Instance.Region) {
        default:
          return skyboxDrawFuncNa;
      }
    }

    // Function that draws the ending cake image:
    public static uint endCakeDrawFuncNa = 0x802D28CC;

    public static List<ObjectComboEntry> objectComboEntries =
        [];

    public static List<BehaviorNameEntry> behaviorNameEntries =
        [];

    public static BehaviorNameEntry GetBehaviorNameEntryFromSegAddress(
        uint address) {
      foreach (BehaviorNameEntry entry in behaviorNameEntries) {
        if (entry.Behavior == address)
          return entry;
      }

      BehaviorNameEntry newBne =
          new BehaviorNameEntry("Undefined Behavior", address);
      behaviorNameEntries.Add(newBne);

      return behaviorNameEntries[behaviorNameEntries.Count - 1];
    }

    public static string GetDefaultObjectComboPath() {
      // Console.WriteLine("ROM.Instance.Region = " + ROM.Instance.Region.ToString());
      switch (Rom.Instance.Region) {
        default:
        case RomRegion.NORTH_AMERICA:
          return "./data/ObjectCombos_NA.json";
        case RomRegion.EUROPE:
          return "./data/ObjectCombos_EU.json";
        case RomRegion.JAPAN:
          return "./data/ObjectCombos_JP.json";
        case RomRegion.JAPAN_SHINDOU:
          return "./data/ObjectCombos_JS.json";
        case RomRegion.CHINESE_IQUE:
          return "./data/ObjectCombos_IQ.json";
      }
    }

    public static string GetDefaultBehaviorNamesPath() {
      // Console.WriteLine("ROM.Instance.Region = " + ROM.Instance.Region.ToString());
      switch (Rom.Instance.Region) {
        default:
          //case ROM_Region.NORTH_AMERICA:
          return "./data/BehaviorNames.json";
        /*case ROM_Region.EUROPE:
            return "./data/ObjectCombos_EU.json";
        case ROM_Region.JAPAN:
            return "./data/ObjectCombos_JP.json";
        case ROM_Region.JAPAN_SHINDOU:
            return "./data/ObjectCombos_JS.json";*/
      }
    }
  }
}