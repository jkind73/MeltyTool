using System.Drawing;

using f3dzex2.image;

namespace UoT.hacks {
  public static class Hacks {
    public static void ApplyHacks(IN64Hardware n64Hardware,
                                  string fileName) {
      var environmentColor = EnvironmentColorHacks.GetColorForObject(fileName);
      n64Hardware.Rsp.EnvironmentColor = environmentColor ?? Color.Magenta;
    }
  }
}