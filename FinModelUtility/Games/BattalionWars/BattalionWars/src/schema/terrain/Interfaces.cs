using fin.data;
using fin.schema;
using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.terrain;

public record BwSection(string Name, int Size, long Offset);

[BinarySchema]
public sealed partial class BwHeightmapMaterial : IBinaryConvertible {
  [StringLengthSource(16)]
  public string Texture1 { get; private set; }

  [StringLengthSource(16)]
  public string Texture2 { get; private set; }

  [Unknown]
  public uint[] Unknown { get; } = new uint[4];
}

public interface IBwTerrain {
  IBwHeightmap Heightmap { get; }
  IList<BwHeightmapMaterial> Materials { get; }
}

public interface IBwHeightmap {
  Grid<IBwHeightmapChunk?> Chunks { get; }
  float GetHeightAtPosition(float x, float y);
}

public interface IBwHeightmapChunk {
  Grid<IBwHeightmapTile> Tiles { get; }
}

public interface IBwHeightmapTile {
  Grid<BwHeightmapPoint> Points { get; }
  uint MatlIndex { get; }

  HeightmapParser.SchemaTile Schema { get; }
}

public struct BwHeightmapPoint {
  public float X { get; set; }
  public float Y { get; set; }
  public float Height { get; set; }
  public Rgba32 LightColor { get; set; }
}