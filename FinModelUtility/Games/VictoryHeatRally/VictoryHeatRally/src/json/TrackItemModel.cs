using Newtonsoft.Json;

namespace vhr.json;

public sealed class TrackItemModel {
  public bool? array;
  public TrackItemCmesh? cmesh;
  public string? model;

  [JsonConverter(typeof(SingleOrArrayConverter<string>))]
  public List<string>? sprite;

  public int? subdiv;
  public int? tilt;

  [JsonConverter(typeof(SingleOrArrayConverter<string>))]
  public List<string>? texture;
}