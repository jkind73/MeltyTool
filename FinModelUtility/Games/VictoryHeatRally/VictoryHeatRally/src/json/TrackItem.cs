namespace vhr.json;

public sealed class TrackItem {
  public float[]? myArray;
  public TrackItemStruct? myStruct;
  public string? type;

  // Background
  public string[] bgindex;
  public int[]? xoff;
  public int[]? xparallax;
  public int[]? yoff;
  public int[]? yparallax;

  // Other
  public string? bgm;
  public string? floortex;
  public int? fogEnabled;
  public int? laps;
  public int? rally;
  public int? startpos;
  public string? sprBarrier;
  public int? timeofday;
  public string? trackName;
}