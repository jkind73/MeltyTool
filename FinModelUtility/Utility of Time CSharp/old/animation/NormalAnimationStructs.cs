using System.Collections.Generic;

namespace UoT {
  // Based on the structs at:
  // https://wiki.cloudmodding.com/oot/Animation_Format#Normal_Animations

  public sealed class NormalAnimation : IAnimation {
    public ushort[] angles = [];
    public IList<Vec3S> positions;
    public NormalAnimationTrack[] tracks = [];
    public uint trackOffset;
    public uint angleCount;

    public required uint Offset { get; init; }

    public ushort FrameCount { get; set; }

    public int PositionCount => this.positions.Count;
    public Vec3S GetPosition(int i) => this.positions[i];

    public int TrackCount => this.tracks.Length;
    public IAnimationTrack GetTrack(int i) => this.tracks[i];

    public FacialState GetFacialState(int _) => FacialState.@default;
  }

  public sealed class NormalAnimationTrack : IAnimationTrack {
    public int Type { get; set; } // 0 = constant, 1 = keyframe
    public IList<ushort> Frames { get; set; } = [];
  }
}
