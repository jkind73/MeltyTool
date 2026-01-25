using System.Numerics;

using fin.schema.vector;

using schema.binary;

namespace granny3d {
  public sealed class GrannyTransform : IGrannyTransform, IBinaryDeserializable {
    public GrannyTransformFlags Flags { get; private set; }
    public Vector3F Position { get; private set; }
    public Quaternion Orientation { get; private set; }
    public Vector3F[] ScaleShear { get; } = new Vector3F[3];

    public void Read(IBinaryReader br) {
      this.Flags = (GrannyTransformFlags)br.ReadInt32();

      this.Position = br.ReadNew<Vector3F>();

      this.Orientation = new Quaternion(br.ReadSingle(), br.ReadSingle(),
                                        br.ReadSingle(), br.ReadSingle());

      for (var i = 0; i < 3; ++i) {
        this.ScaleShear[i] = br.ReadNew<Vector3F>();
      }
    }
  }
}
