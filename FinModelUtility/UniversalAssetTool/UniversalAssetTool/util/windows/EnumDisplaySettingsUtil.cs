using System.Runtime.InteropServices;

namespace uni.util.windows;

public static class EnumDisplaySettingsUtil {
  public static DisplaySettings GetCurrentDisplaySettings() {
    DisplaySettings displaySettings = default;
    EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS_, ref displaySettings);
    return displaySettings;
  }

  public static IEnumerable<DisplaySettings> GetAllPossibleDisplaySettings() {
    DisplaySettings displaySettings = default;
    int i = 0;
    while (EnumDisplaySettings(null, i++, ref displaySettings)) {
      yield return displaySettings;
    }
  }

  public static int GetDisplayFrequency()
    => GetCurrentDisplaySettings().dmDisplayFrequency;

  [DllImport("user32.dll")]
  public static extern bool EnumDisplaySettings(
      string deviceName,
      int modeNum,
      ref DisplaySettings displaySettings);

  const int ENUM_CURRENT_SETTINGS_ = -1;

  /// <summary>Specifies the angle of the screen.</summary>
  public enum ScreenOrientation {
    /// <summary>The screen is oriented at 0 degrees.</summary>
    ANGLE0,

    /// <summary>The screen is oriented at 90 degrees.</summary>
    ANGLE90,

    /// <summary>The screen is oriented at 180 degrees.</summary>
    ANGLE180,

    /// <summary>The screen is oriented at 270 degrees.</summary>
    ANGLE270,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DisplaySettings {
    private const int CCHDEVICENAME_ = 0x20;
    private const int CCHFORMNAME_ = 0x20;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
    public string dmDeviceName;

    public short dmSpecVersion;
    public short dmDriverVersion;
    public short dmSize;
    public short dmDriverExtra;
    public int dmFields;
    public int dmPositionX;
    public int dmPositionY;
    public ScreenOrientation dmDisplayOrientation;
    public int dmDisplayFixedOutput;
    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
    public string dmFormName;

    public short dmLogPixels;
    public int dmBitsPerPel;
    public int dmPelsWidth;
    public int dmPelsHeight;
    public int dmDisplayFlags;
    public int dmDisplayFrequency;
    public int dmICMMethod;
    public int dmICMIntent;
    public int dmMediaType;
    public int dmDitherType;
    public int dmReserved1;
    public int dmReserved2;
    public int dmPanningWidth;
    public int dmPanningHeight;
  }
}