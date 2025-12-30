using System.Numerics;

using CommunityToolkit.Diagnostics;

namespace vhr;

public sealed class TrackPath {
  private readonly IReadOnlyList<TrackNode> nodes_;
  private readonly IReadOnlyList<float> segmentLengths_;
  private readonly float totalLength_;

  public TrackPath(IReadOnlyList<TrackNode> nodes) {
    this.nodes_ = nodes;

    var segmentLengths = new float[nodes.Count - 1];
    this.segmentLengths_ = segmentLengths;

    TrackNode previous = nodes[0];
    for (var i = 1; i < nodes.Count; i++) {
      var current = nodes[i];

      segmentLengths[i - 1]
          = (current.Translation - previous.Translation).Length();

      previous = current;
    }

    this.totalLength_ = this.segmentLengths_.Sum();
  }

  public Vector3 GetTranslationAtOffset(float position, float xOffsetPercent) {
    Guard.IsGreaterThanOrEqualTo(position, 0);

    position %= this.totalLength_;

    TrackNode previous = this.nodes_[0];
    for (var i = 1; i < this.nodes_.Count; i++) {
      var current = this.nodes_[i];

      var distance = this.segmentLengths_[i - 1];
      if (position < distance) {
        var fraction = position / distance;

        var forwardAngleDegrees = float.Lerp(previous.ForwardAngleDegrees,
                                             current.ForwardAngleDegrees,
                                             fraction);
        var rightAngleDegrees = forwardAngleDegrees - 90;
        var rightAngleRadians = rightAngleDegrees / 180 * MathF.PI;
        var rightNormal = new Vector3(MathF.Cos(rightAngleRadians),
                                      0,
                                      -MathF.Sin(rightAngleRadians));

        var trackWidth = float.Lerp(previous.Width, current.Width, fraction);

        var translation
            = Vector3.Lerp(previous.Translation, current.Translation, fraction);
        translation += rightNormal * xOffsetPercent * trackWidth;

        return translation;
      }

      position -= distance;
      previous = current;
    }

    throw new InvalidDataException();
  }
}