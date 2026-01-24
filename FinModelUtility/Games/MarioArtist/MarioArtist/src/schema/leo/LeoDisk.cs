using System.Security.Cryptography;
using System.Text;
using fin.util.asserts;
using marioartist.schema.mfs;
using schema.binary;

namespace marioartist.schema.leo;

/// <summary>
/// Shamelessly stolen from:
/// https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/Leo/LeoDisk.cs
/// </summary>
public sealed class LeoDisk {
  public enum DiskFormat {
    SDK,
    D64,
    RAM,
    N64,
    MAME,
    INVALID
  }

  public enum FileSystem {
    MFS,
    ATNFS,
    INVALID
  }

  public DiskFormat format;
  public FileSystem ramFileSystem;
  public int offsetToRamArea;
  public int offsetToSysData;
  public int diskType;
  public byte[] data;

  public LeoDisk(IBinaryReader br) {
    this.Load_(br);
  }

  void Load_(IBinaryReader br) {
    //Assume file is bad first
    this.format = DiskFormat.INVALID;
    this.ramFileSystem = FileSystem.INVALID;
    this.offsetToSysData = -1;
    this.offsetToRamArea = -1;

    var fileLength = br.Length;

    if (fileLength > Leo.RAM_SIZE[0]) {
      //Perform System Area heuristics if the file size is MAME or SDK
      bool correctSysData = false;
      byte[] sysData = new byte[Leo.SECTOR_SIZE[0]];
      if ((fileLength == Leo.DISK_SIZE_MAME) ||
          (fileLength == Leo.DISK_SIZE_SDK)) {
        //Check each System Data Block

        //Check Retail SysData
        foreach (int lba in Leo.LBA_SYS_PROD) {
          this.offsetToSysData = Leo.BLOCK_SIZE[0] * lba;
          br.Position = this.offsetToSysData;
          br.ReadBytes(sysData);

          bool isEqual = true;
          for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
            byte[] sysDataCompare = new byte[sysData.Length];
            br.ReadBytes(sysDataCompare);

            //Compare Bytes
            for (int j = 0; j < sysDataCompare.Length; j++) {
              if (sysDataCompare[j] != sysData[j]) {
                isEqual = false;
                break;
              }
            }

            //If not equal then don't bother doing more
            if (!isEqual) break;
          }

          correctSysData = isEqual;

          //If SysData is found then it's fine don't bother checking the rest
          if (correctSysData) break;
        }

        //Check Dev SysData if not found
        if (!correctSysData) {
          sysData = new byte[Leo.SECTOR_SIZE[3]];
          foreach (int lba in Leo.LBA_SYS_DEV) {
            this.offsetToSysData = Leo.BLOCK_SIZE[0] * lba;
            br.Position = this.offsetToSysData;
            br.ReadBytes(sysData);

            bool isEqual = true;
            for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
              byte[] sysDataCompare = new byte[sysData.Length];
              br.ReadBytes(sysDataCompare);

              //Compare Bytes
              for (int j = 0; j < sysDataCompare.Length; j++) {
                if (sysDataCompare[j] != sysData[j]) {
                  isEqual = false;
                  break;
                }
              }

              //If not equal then don't bother doing more
              if (!isEqual) break;
            }

            correctSysData = isEqual;

            //If SysData is found then it's fine don't bother checking the rest
            if (correctSysData) break;
          }
        }
      }

      if (fileLength == Leo.DISK_SIZE_MAME) {
        /* --- Check if it's MAME Format --- */

        //if SysData found
        if (correctSysData) {
          this.diskType = sysData[0x5] & 0xF;
          this.format = DiskFormat.MAME;
          this.offsetToRamArea = 0;
          //Data is good.
        }
      } else if (fileLength == Leo.DISK_SIZE_SDK) {
        /* --- Check if it's SDK Format --- */

        //if SysData found
        if (correctSysData) {
          this.diskType = sysData[0x5] & 0xF;
          this.format = DiskFormat.SDK;
          this.offsetToRamArea
            = Leo.LbaToByte(this.diskType, 0, Leo.RAM_START_LBA[this.diskType]);
          //Data is good.
        }
      } else {
        /* --- Check if it's N64 CART Format --- */

        //SHA256 check if N64 Cartridge Port bootloader
        byte[] headerTest = new byte[0xFC0];
        br.Position = 0x40;
        br.ReadBytes(headerTest);

        SHA256 hashHeader = SHA256.Create();
        hashHeader.ComputeHash(headerTest);

        string hashHeaderStr = "";
        foreach (byte b in hashHeader.Hash)
          hashHeaderStr += b.ToString("x2");

        Console.WriteLine(hashHeaderStr);

        int offsetStart = 0;

        //SHA256 = 53c0088fb777870d0af32f0251e964030e2e8b72e830c26042fd191169508c05
        if (hashHeaderStr ==
            "53c0088fb777870d0af32f0251e964030e2e8b72e830c26042fd191169508c05") {
          offsetStart
            = 0x738C0 - 0x10E8; //Start of User LBA 0 (24 w/ System Area)

          br.Position = 0x1000;
          br.ReadBytes(sysData);

          this.diskType = sysData[0x5] & 0xF;
          this.format = DiskFormat.N64;
          this.offsetToRamArea
            = Leo.LbaToByte(this.diskType,
                            0,
                            Leo.RAM_START_LBA[this.diskType]) -
              offsetStart;
          this.offsetToSysData = 0x1000;
          //Data is good.
        }
      }
    } else {
      /* --- Check if it's RAM Format --- */
      if (Array.Exists(Leo.RAM_SIZE, x => x == fileLength)) {
        this.diskType = Array.FindIndex(Leo.RAM_SIZE, x => x == fileLength);
        this.format = DiskFormat.RAM;
        this.offsetToRamArea = 0;
        //Data is good.
      }
    }

    if (this.format != DiskFormat.INVALID) {
      //Copy full file
      this.data = new byte[fileLength];
      br.Position = 0;
      br.ReadBytes(this.data);
      //Disk is considered loaded here.
    }

    /* Check RAM FileSystem */
    if (this.format != DiskFormat.INVALID) {
      //Only check if RAM Area exists (Disk Type 6 has no RAM area)
      if (this.diskType < 6) {
        //MultiFileSystem
        byte[] test = new byte[Mfs.RAM_ID.Length];
        var firstRam = this.ReadLba(Leo.RAM_START_LBA[this.diskType]);
        firstRam.Slice(0, test.Length).CopyTo(test);

        //See if equal to RAM_ID, and if so, it is found.
        if (Encoding.ASCII.GetString(test).Equals(Mfs.RAM_ID)) {
          this.ramFileSystem = FileSystem.MFS;
        }
      }
    }
  }

  public ReadOnlySpan<byte> ReadLba(int lba) {
    //Do not read anywhere before RAM Area
    Asserts.True(lba >= Leo.RAM_START_LBA[this.diskType]);
    Asserts.True(lba <= Leo.MAX_LBA);

    //Read Block
    var outputLength = Leo.LbaToByte(this.diskType, lba, 1);
    if (this.format == DiskFormat.MAME) {
      int sourceOffset = Leo.LbaToMameOffset(lba, this.GetSystemData());
      return this.data.AsSpan(sourceOffset, outputLength);
    } else {
      Asserts.True(this.offsetToRamArea >= 0);
      int sourceOffset
        = Leo.LbaToByte(this.diskType,
                        Leo.RAM_START_LBA[this.diskType],
                        lba - Leo.RAM_START_LBA[this.diskType]) +
          this.offsetToRamArea;
      return this.data.AsSpan(sourceOffset, outputLength);
    }
  }

  public ReadOnlySpan<byte> GetSystemData() {
    Asserts.True(this.offsetToSysData >= 0);
    return this.data.AsSpan(this.offsetToSysData, Leo.SECTOR_SIZE[0]);
  }

  public byte[]? GetRamAreaArray() {
    if (this.offsetToRamArea < 0) return null;

    var totalLength = 0;
    for (int lba = Leo.RAM_START_LBA[this.diskType]; lba <= Leo.MAX_LBA; lba++) {
      totalLength += Leo.LbaToByte(this.diskType, lba, 1);
    }

    var i = 0;
    var array = new byte[totalLength];
    for (int lba = Leo.RAM_START_LBA[this.diskType]; lba <= Leo.MAX_LBA; lba++) {
      var readSlice = this.ReadLba(lba);

      var dst = array.AsSpan(i, readSlice.Length);
      readSlice.CopyTo(dst);

      i += readSlice.Length;
    }

    return array;
  }
}