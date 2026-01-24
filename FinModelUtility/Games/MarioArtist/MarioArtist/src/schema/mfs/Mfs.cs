namespace marioartist.schema.mfs;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/MFS/MFS.cs#L46
/// </summary>
public static class Mfs {
  //MFS Volume
  public const string ROM_ID = "64dd-Multi0201";
  public const string RAM_ID = "64dd-Multi0101";

  //MFS FAT
  public enum Fat : ushort {
    UNUSED = 0x0000,
    DONT_MANAGE = 0xFFFD,
    PROHIBITED = 0xFFFE,
    LAST_FILE_BLOCK = 0xFFFF
  }

  public const int FAT_MAX = 2874;

  //MFS Entry
  public struct EntryAttr {
    public bool copyLimit;   //Limit Copy
    public bool encode;      //Encode
    public bool hidden;      //Hidden
    public bool disableRead; //Cannot be read by other applications

    public bool
        disableWrite; //Cannot be written, renamed, or deleted by other applications
  }

  public static readonly ushort[] ENTRY_LIMIT =
      [899, 814, 729, 644, 559, 474, 0];

  enum Error {
    GOOD = 0, ARGUMENT, FILENAME, FILE_NOT_EXIST, DISK_FULL, FILE_ALREADY_EXISTS
  }
}