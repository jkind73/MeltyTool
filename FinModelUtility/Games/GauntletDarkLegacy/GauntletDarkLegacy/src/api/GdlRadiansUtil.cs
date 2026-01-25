using System.Numerics;


namespace gdl.api;

public static class GdlRadiansUtil {
  public static Quaternion CreateQuaternion(Vector3 xyz)
    => CreateQuaternion(xyz.X, xyz.Y, xyz.Z);

  public static Quaternion CreateQuaternion(float x, float y, float z) {
    // y is definitely yaw, needs to be negative (e.g. turning side-to-side in idle)
    // x is definitely pitch, needs to be negative (e.g. legs running)
    // z must be roll, needs to be negative (e.g. archer bobbing back and forth in idle)

    var yawQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -y);
    var pitchQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -x);
    var rollQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, z);

    // TODO: Still not quite right, e.g. the warrior model's SHOVE animation
    // has his arms broken

    return yawQuaternion * pitchQuaternion * rollQuaternion;
  }
}