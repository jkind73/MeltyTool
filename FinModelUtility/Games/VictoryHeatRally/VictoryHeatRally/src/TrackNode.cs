using System.Numerics;

namespace vhr;

public class TrackNode(float[] myArray) {
  public Vector3 Translation { get; }
    = new(myArray[0], -myArray[2], myArray[1]);

  public float ForwardAngleDegrees => myArray[3];
  public int MaybeTrackType => (int) myArray[4];
  public int MaybeLeftWallType => (int) myArray[6];
  public int MaybeRightWallType => (int) myArray[7];
  public float Width => myArray[10];
}