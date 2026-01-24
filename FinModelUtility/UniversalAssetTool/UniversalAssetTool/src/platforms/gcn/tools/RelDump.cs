using System.Globalization;
using System.Text.RegularExpressions;

using fin.io;
using fin.util.asserts;

namespace uni.platforms.gcn.tools;

/// <summary>
///   Shamelessly stolen from https://github.com/Cuyler36/RELDumper
/// </summary>
public sealed class RelDump {
  private const int REL_HEADER_SIZE_ = 0xA0;
  private const int DOL_HEADER_SIZE_ = -0xC0;
  private int currentHeaderSize_;

  private Dictionary<string, int> dataSectionMap_;

  public bool Run(
      IFileHierarchyFile relFile,
      IFileHierarchyFile mapFile,
      bool cleanup) {
    Asserts.True(relFile.Exists);
    Asserts.True(mapFile.Exists);

    string relLocation = relFile.FullPath;
    string mapLocation = mapFile.FullPath;

    string dataDir = Path.GetDirectoryName(relLocation) +
                      "\\" +
                      Path.GetFileNameWithoutExtension(relLocation);

    if (!new FinDirectory(dataDir).IsEmpty) {
      return false;
    }

    this.currentHeaderSize_ =
        Path.GetExtension(relLocation).Contains("dol")
            ? DOL_HEADER_SIZE_
            : REL_HEADER_SIZE_;
    byte[] relData = File.ReadAllBytes(relLocation);
    string[] mapData = File.ReadAllLines(mapLocation);
    string memoryMap =
        mapData.FirstOrDefault(o => o.Contains("Memory map:"));
    if (!string.IsNullOrEmpty(memoryMap)) {
      int memoryMapIdx = Array.IndexOf(mapData, memoryMap);
      if (memoryMapIdx > -1) {
        memoryMapIdx += 3; // Data starts three lines after

        Directory.CreateDirectory(dataDir);

        // Create Section Folders
        this.dataSectionMap_ = new Dictionary<string, int>();
        for (int i = memoryMapIdx; i < mapData.Length; i++) {
          if (string.IsNullOrEmpty(mapData[i]))
            break;
          string sectionInfo = mapData[i].TrimStart();
          string sectionName =
              Regex.Match(sectionInfo, @"^[^ ]*").Value;
          string sectionOffsets =
              sectionInfo[(sectionName.Length + 11)..];
          string sectionSize =
              Regex.Match(sectionOffsets, @"^[^ ]*").Value;
          string sectionOffset =
              sectionOffsets.Substring(sectionSize.Length + 1, 8);
          if (int.TryParse(sectionOffset,
                           NumberStyles.AllowHexSpecifier,
                           null,
                           out int offset) &&
              int.TryParse(sectionSize,
                           NumberStyles.AllowHexSpecifier,
                           null,
                           out int size)) {
            string sectionDir = dataDir + "\\" + sectionName;
            if (!Directory.Exists(sectionDir)) {
              Directory.CreateDirectory(sectionDir);
            }
            this.dataSectionMap_.Add(sectionName, offset);
          }
        }

        // Section off data
        string currentSection = "";
        for (int i = 0; i < memoryMapIdx - 3; i++) {
          string line = mapData[i];
          if (!string.IsNullOrEmpty(line)) {
            if (line.Contains(" section layout")) {
              i += 3; // Skip column text
              currentSection =
                  Regex.Match(line.TrimStart(), @"^[^ ]*").Value;
              Console.WriteLine("Switched to section: " +
                                currentSection);
              //Console.ReadKey();
            } else if (!string.IsNullOrEmpty(currentSection)) {
              if (line.Contains(@"...")) {
                //Console.WriteLine("Contained ... : " + Line);
                continue;
              }

              line = line.Trim(); // Clear Leading/Trailing Whitespace
              line = line.Replace("\t",
                                  " "); // Confirm all tabs get turned into a space
              line = Regex.Replace(line,
                                   @"\s+",
                                   " "); // Turn multiple spaces/tabs to one space
              string[] lineData = line.Split(' ');
              /*
               * Line_Data contents
               * =================
               * Starting Address (relative to section start) 0
               * Size 1
               * Virtual Address (same as Starting Address??) 2
               * Type (1 = Object, 4 = Method) 3
               * Name 4
               * Object 5
               */
              int offset = this.GetRELOffset_(
                  this.dataSectionMap_[currentSection],
                  int.Parse(
                      lineData[0],
                      NumberStyles
                          .AllowHexSpecifier));
              int size =
                  int.Parse(lineData[1], NumberStyles.AllowHexSpecifier);
              bool isObject = lineData[3].Equals("1");
              string methodName = lineData[4];
              string objectName = lineData[5];
              if (lineData.Length >= 7)
                objectName += ("_" + lineData[6]);

              string dir = dataDir +
                           "\\" +
                           currentSection +
                           "\\" +
                           objectName;
              if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
              }

              if (!isObject) {
                try {
                  using (FileStream dataFile =
                         File.Create(dir + "\\" + methodName + ".bin")) {
                    dataFile.Write(relData, offset, size);
                    dataFile.Flush();
                  }
                } catch {
                  //Console.WriteLine(string.Format("Unable to create file for: {0}/{1}! Offset was past the end of the file!",
                  //Current_Section, Object_Name));
                  //Console.ReadKey();
                }
              } else {
                Console.WriteLine(
                    string.Format("Parsing data for {0}/{1}",
                                  currentSection,
                                  objectName));
              }
            }
          }
        }
      }
    }

    if (cleanup) {
      relFile.Impl.Delete();
      mapFile.Impl.Delete();
    }

    return true;
  }

  private int GetRELOffset_(int sectionOffset, int dataOffset) {
    return this.currentHeaderSize_ +
           sectionOffset +
           dataOffset; // Header is 0xA0 bytes
  }
}