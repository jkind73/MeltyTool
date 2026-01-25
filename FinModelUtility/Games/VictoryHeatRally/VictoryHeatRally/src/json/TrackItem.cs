namespace vhr.json;

public sealed class TrackItem {
  public float[]? my_array;
  public TrackItemStruct? my_struct;
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
  public int? fog_enabled;
  public int? laps;
  public int? rally;
  public int? startpos;
  public string? spr_barrier;
  public int? timeofday;
  public string? track_name;
}