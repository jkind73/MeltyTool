using System.Numerics;

using f3dzex2.image;

using sm64.LevelInfo;
using sm64.memory;

namespace sm64.Scripts {
  public sealed class LevelScripts {
    private static uint BytesToInt_(byte[] b, int offset, int length) {
      switch (length) {
        case 1: return b[0 + offset];
        case 2: return (uint) (b[0 + offset] << 8 | b[1 + offset]);
        case 3:
          return (uint) (b[0 + offset] << 16 | b[1 + offset] << 8 |
                         b[2 + offset]);
        default:
          return (uint) (b[0 + offset] << 24 | b[1 + offset] << 16 |
                         b[2 + offset] << 8 | b[3 + offset]);
      }
    }

    public static int Parse(IN64Hardware<ISm64Memory> sm64Hardware,
                            ref Level lvl,
                            byte seg,
                            uint off) {
      var sm64Memory = sm64Hardware.Memory;

      if (seg == 0) return -1;
      Rom rom = Rom.Instance;
      byte[] data = rom.GetSegment(seg, null);
      bool end = false;
      int endCmd = 0;
      while (!end) {
        //Stopwatch stopWatch = new Stopwatch();
        //stopWatch.Start();
        byte cmdLen = data[off + 1];
        byte[] cmd = rom.getSubArray_safe(data, off, cmdLen);
        //Console.WriteLine(rom.decodeSegmentAddress_safe(seg, off, null).ToString("X8"));
        //rom.printArray(cmd, cmdLen);
        string desc = "Unknown command";
        bool alreadyAdded = false;
        switch (cmd[0]) {
          case 0x00:
          case 0x01:
            CMD_00(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            break;
          case 0x02:
            endCmd = 2;
            desc = "End level script";
            end = true;
            break;
          case 0x03:
          case 0x04:
            desc = "Delay frames";
            break;
          case 0x05:
            endCmd = CMD_05(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            end = true;
            break;
          case 0x06:
            if (CMD_06(sm64Hardware, ref lvl, ref desc, cmd, seg, off) == 0x02) {
              end = true;
              endCmd = 2;
            }
            alreadyAdded = true;
            break;
          case 0x07:
            end = true;
            desc = "Pop script stack and return back";
            endCmd = 0x07;
            break;
          case 0x08:
            desc = "Push script stack and a 16-bit parameter onto stack";
            break;
          case 0x09:
            desc = "Pops script stack and parameter";
            break;
          case 0x0A:
            desc =
                "Push next level command on script stack and param 0x00000000 onto stack";
            break;
          case 0x0B:
            CMD_0B(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0C:
            CMD_0C(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            break;
          case 0x0D:
            CMD_0D(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0E:
            CMD_0E(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0F:
            desc = "Skip following 0x10 (Do nothing) commands";
            break;
          case 0x10:
            desc = "Do nothing";
            break;
          case 0x11:
            desc = "Call ASM function and set level accumulator (script_accum)";
            break;
          case 0x12:
            desc =
                "Call ASM function loop and set level accumulator (script_accum)";
            break;
          case 0x13:
            desc = "Set level accumulator (script_accum) as 0x" +
                   BytesToInt_(cmd, 2, 2).ToString("X4");
            break;
          case 0x14:
            desc = "Call PushPoolState() function";
            break;
          case 0x15:
            desc = "Call PopPoolState() function";
            break;
          case 0x16:
            desc = "Copy bytes from ROM (0x" +
                   BytesToInt_(cmd, 8, 4).ToString("X8") + " to 0x" +
                   BytesToInt_(cmd, 12, 4).ToString("X8") +
                   ") to RAM address 0x" + BytesToInt_(cmd, 4, 4).ToString("X8");
            break;
          case 0x17:
            CMD_17(ref lvl, ref desc, cmd);
            break;
          case 0x18:
          case 0x1A:
            CMD_18(ref lvl, ref desc, cmd);
            break;
          case 0x19:
            desc = "Create Mario head demo (";
            switch (cmd[3]) {
              case 1:
                desc += "No face";
                break;
              case 2:
                desc += "Regular face";
                break;
              case 3:
                desc += "Game over face";
                break;
            }
            desc += ")";
            break;
          case 0x1B:
            desc = "Start loading sequence";
            break;
          case 0x1C:
            desc = "Level & Memory cleanup";
            break;
          case 0x1D:
            desc = "End loading sequence";
            break;
          case 0x1E:
            desc = "Allocate space for level data from pool";
            break;
          case 0x1F: {
            //Globals.DEBUG_PLG = true;                       
            CMD_1F(sm64Hardware, ref lvl, ref desc, cmd, data);
            break;
          }
          case 0x20: {
            desc = "End of area " + lvl.CurrentAreaId;
            sm64Memory.AreaId = null;
            break;
          }
          case 0x21:
            CMD_21(sm64Hardware, ref lvl, ref desc, cmd);
            break;
          case 0x22:
            //Globals.DEBUG_PLG = false;
            CMD_22(sm64Hardware, ref lvl, ref desc, cmd);
            break;
          case 0x24:
            CMD_24(sm64Memory, ref lvl, ref desc, cmd, seg, off);
            break;
          case 0x25:
            desc = "Setup Mario object";
            break;
          case 0x26:
            CMD_26(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x27:
            CMD_27(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x28:
            CMD_28(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x2B: {
            desc = "Mario's default pos = (" +
                   (short) BytesToInt_(cmd, 6, 2) + "," +
                   (short) BytesToInt_(cmd, 8, 2) + "," +
                   (short) BytesToInt_(cmd, 10, 2) + "), Y-Rot = " +
                   (short) BytesToInt_(cmd, 4, 2) + ", start in area " + cmd[2];
          }
            break;
          case 0x2E:
            CMD_2E(ref lvl, ref desc, cmd, sm64Memory.AreaId);
            break;
          case 0x2F:
            CMD_2F(ref lvl, ref desc, cmd, sm64Memory.AreaId);
            break;
          case 0x30:
            desc = "Show dialog message when level starts; dialog ID = 0x" +
                   cmd[3].ToString("X2");
            break;
          case 0x31: {
            var defaultTerrainType = cmd[3];

            // TODO: Does this need to be remembered for future areas?
            var area =
                lvl.areas.Single(area => area.AreaId == sm64Memory.AreaId);
            area.DefaultTerrainType = defaultTerrainType;

            switch (defaultTerrainType) {
              case 0:
                desc = "Set default terrain to \"Normal\"";
                break;
              case 1:
                desc = "Set default terrain to \"Normal B\"";
                break;
              case 2:
                desc = "Set default terrain to \"Snow\"";
                break;
              case 3:
                desc = "Set default terrain to \"Sand\"";
                break;
              case 4:
                desc = "Set default terrain to \"Haunted House\"";
                break;
              case 5:
                desc = "Set default terrain to \"Water levels\"";
                break;
              case 6:
                desc = "Set default terrain to \"Slippery Slide\"";
                break;
            }
            break;
          }
          case 0x32:
            desc = "Do nothing";
            break;
          case 0x33:
            if (cmd[2] == 0x01)
              desc = "Fade screen with color (R = " + cmd[4] + ", G = " +
                     cmd[5] + ", B = " + cmd[6] + ", duration = " + cmd[3] +
                     " frames)";
            else
              desc = "Disable screen fade";
            break;
          case 0x34:
            if (cmd[2] == 0x00)
              desc = "Cancel blackout";
            else
              desc = "Blackout screen";
            break;
          case 0x36:
            desc = "Set music (Seq = 0x" + cmd[5].ToString("X2") + ")";
            break;
          case 0x37:
            desc = "Set music (Seq = 0x" + cmd[3].ToString("X2") + ")";
            break;
          case 0x39:
            CMD_39(sm64Memory, ref lvl, ref desc, cmd);
            break;
          case 0x3B:
            desc = "Add jet stream; Position = (" +
                   (short) BytesToInt_(cmd, 4, 2) + "," +
                   (short) BytesToInt_(cmd, 6, 2) + "," +
                   (short) BytesToInt_(cmd, 8, 2) +
                   "), Intensity = " + (short) BytesToInt_(cmd, 10, 2);
            break;
        }
        if (!alreadyAdded)
          AddLsCommandToDump_(ref lvl, cmd, seg, off, desc, sm64Memory.AreaId);
        //stopWatch.Stop();
        // if(stopWatch.Elapsed.Milliseconds > 1)
        //    Console.WriteLine("RunTime (CMD "+cmd[0].ToString("X2")+"): " + stopWatch.Elapsed.Milliseconds + "ms");
        off += cmdLen;
      }
      return endCmd;
    }

    private static void AddLsCommandToDump_(ref Level lvl,
                                           byte[] cmd,
                                           byte seg,
                                           uint offset,
                                           string description,
                                           byte? areaId) {
      ScriptDumpCommandInfo info = new ScriptDumpCommandInfo();
      info.data = cmd;
      info.description = description;
      info.segAddress = (uint) (seg << 24) | offset;
      info.romAddress =
          Rom.Instance.decodeSegmentAddress_safe(seg, offset, areaId);
      lvl.levelScriptCommandsForDump.Add(info);
    }

    private static void CMD_00(
        IN64Hardware<ISm64Memory> sm64Hardware,
        ref Level lvl,
        ref string desc,
        byte[] cmd,
        byte orgSeg,
        uint orgOff) {
      Rom rom = Rom.Instance;
      byte seg = cmd[3];
      uint start = BytesToInt_(cmd, 4, 4);
      uint end = BytesToInt_(cmd, 8, 4);
      uint off = BytesToInt_(cmd, 13, 3);
      desc = "Load segment 0x" + seg.ToString("X2") + " and jump to 0x" +
             seg.ToString("X2") + off.ToString("X6");
      rom.SetSegment(seg, start, end, false, null);
      if (seg == 0x14) {
        desc +=
            " (This gets skipped in Quad64, since it's just the star select screen data)";
        AddLsCommandToDump_(ref lvl,
                           cmd,
                           orgSeg,
                           orgOff,
                           desc,
                           sm64Hardware.Memory.AreaId);
        return;
      }

      AddLsCommandToDump_(ref lvl,
                         cmd,
                         orgSeg,
                         orgOff,
                         desc,
                         sm64Hardware.Memory.AreaId);
      Parse(sm64Hardware, ref lvl, seg, off);
    }

    private static int CMD_05(IN64Hardware<ISm64Memory> sm64Hardware,
                              ref Level lvl,
                              ref string desc,
                              byte[] cmd,
                              byte currentSeg,
                              uint currentOff) {
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);
      desc = "Jump to segment address 0x" + seg.ToString("X2") +
             off.ToString("X6");
      AddLsCommandToDump_(ref lvl,
                         cmd,
                         currentSeg,
                         currentOff,
                         desc,
                         sm64Hardware.Memory.AreaId);
      if (seg == currentSeg) {
        if ((long) off - (long) currentOff == -4) {
          Console.WriteLine("Infinite loop detected!");
          return 0x02;
        }
      }
      return Parse(sm64Hardware, ref lvl, seg, off);
    }

    private static int CMD_06(IN64Hardware<ISm64Memory> sm64Hardware,
                              ref Level lvl,
                              ref string desc,
                              byte[] cmd,
                              byte orgSeg,
                              uint orgOff) {
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);
      desc = "Push script stack and jump to address 0x" + seg.ToString("X2") +
             off.ToString("X6");
      AddLsCommandToDump_(ref lvl,
                         cmd,
                         orgSeg,
                         orgOff,
                         desc,
                         sm64Hardware.Memory.AreaId);
      return Parse(sm64Hardware, ref lvl, seg, off);
    }

    private static string GetCondition_(byte operation,
                                       uint argument,
                                       bool inverse) {
      string appendInverse = (inverse ? "!" : "");
      switch (operation) {
        case 0:
          return "if (" + appendInverse + " script_accum & 0x" +
                 argument.ToString("X2") + ")";
        case 1:
          return "if (" + appendInverse + "!(script_accum & 0x" +
                 argument.ToString("X2") + "))";
        case 2:
          return "if (" + appendInverse + "script_accum == 0x" +
                 argument.ToString("X2") + ")";
        case 3:
          return "if (" + appendInverse + "script_accum != 0x" +
                 argument.ToString("X2") + ")";
        case 4:
          return "if (" + appendInverse + "script_accum < 0x" +
                 argument.ToString("X2") + ")";
        case 5:
          return "if (" + appendInverse + "script_accum <= 0x" +
                 argument.ToString("X2") + ")";
        case 6:
          return "if (" + appendInverse + "script_accum > 0x" +
                 argument.ToString("X2") + ")";
        case 7:
          return "if (" + appendInverse + "script_accum >= 0x" +
                 argument.ToString("X2") + ")";
        default:
          return "";
      }
    }


    private static void CMD_0B(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte orgSeg,
                               uint orgOff,
                               byte? areaId) {
      var lvlcheck = BytesToInt_(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Pop stack " + " " + GetCondition_(operation, lvlcheck, false);
      AddLsCommandToDump_(ref lvl, cmd, orgSeg, orgOff, desc, areaId);
    }

    private static void CMD_0C(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte orgSeg,
                               uint orgOff) {
      var lvlcheck = BytesToInt_(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Jump to address 0x" + BytesToInt_(cmd, 8, 4).ToString("X8") + " " +
             GetCondition_(operation, lvlcheck, false);
      AddLsCommandToDump_(ref lvl,
                         cmd,
                         orgSeg,
                         orgOff,
                         desc,
                         sm64Hardware.Memory.AreaId);
      if (orgSeg == 0x15) {
        if (lvlcheck == lvl.LevelId) {
          byte seg = cmd[8];
          uint off = BytesToInt_(cmd, 9, 3);
          Parse(sm64Hardware, ref lvl, seg, off);
        }
      }
    }


    private static void CMD_0D(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte orgSeg,
                               uint orgOff,
                               byte? areaId) {
      var lvlcheck = BytesToInt_(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Push next command to stack and jump " + " " +
             GetCondition_(operation, lvlcheck, false);
      AddLsCommandToDump_(ref lvl, cmd, orgSeg, orgOff, desc, areaId);
    }

    private static void CMD_0E(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte orgSeg,
                               uint orgOff,
                               byte? areaId) {
      var lvlcheck = BytesToInt_(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Skip following 0x10 and 0x0F levelscript commands if " + " " +
             GetCondition_(operation, lvlcheck, true);
      AddLsCommandToDump_(ref lvl, cmd, orgSeg, orgOff, desc, areaId);
    }

    private static void CMD_17(ref Level lvl, ref string desc, byte[] cmd) {
      Rom rom = Rom.Instance;
      byte seg = cmd[3];
      uint start = BytesToInt_(cmd, 4, 4);
      uint end = BytesToInt_(cmd, 8, 4);
      desc = "Load Segment 0x" + seg.ToString("X2") + " from ROM 0x" +
             start.ToString("X8") + " to 0x" + end.ToString("X8");
      rom.SetSegment(seg, start, end, false, null);
    }

    private static void CMD_18(ref Level lvl, ref string desc, byte[] cmd) {
      Rom rom = Rom.Instance;
      byte seg = cmd[3];
      uint start = BytesToInt_(cmd, 4, 4);
      uint end = BytesToInt_(cmd, 8, 4);
      desc = "Load Segment 0x" + seg.ToString("X2") +
             " from compressed MIO0 at ROM addr 0x" + start.ToString("X8") +
             " to 0x" + end.ToString("X8");
      byte[] mio0Header = rom.getSubArray_safe(rom.Bytes, start, 0x10);
      if (BytesToInt_(mio0Header, 0, 4) == 0x4D494F30) // Check MIO0 signature
      {
        int compressedOffset = (int) BytesToInt_(mio0Header, 0x8, 4);
        int uncompressedOffset = (int) BytesToInt_(mio0Header, 0xC, 4);
        bool isFakeMio0 =
            rom.TestIfMio0IsFake(start, compressedOffset, uncompressedOffset);
        rom.SetSegment(seg, start, end, true, isFakeMio0,
                       (uint) uncompressedOffset, null);
      }
    }

    private static void CMD_1F(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte[] data) {
      byte areaId = cmd[2];
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);

      sm64Hardware.Memory.AreaId = areaId;

      SetAreaSegmented0XE_(areaId, data);

      desc = "Start area " + areaId + "; Load area geo layout from 0x" +
             seg.ToString("X2") + off.ToString("X6");

      Area newArea = new Area(sm64Hardware, areaId, BytesToInt_(cmd, 4, 4), lvl);

      // Globals.DEBUG_PARSING_LEVEL_AREA = true;
      // Stopwatch stopWatch = new Stopwatch();
      // stopWatch.Start();
      new GeoScriptsWrapper().Parse(sm64Hardware.Memory,
                                    newArea.areaModel,
                                    ref lvl,
                                    seg,
                                    off);
      lvl.SetAreaBackgroundInfo(ref newArea);
      lvl.areas.Add(newArea);
      lvl.CurrentAreaId = areaId;
      // stopWatch.Stop();
      // Console.WriteLine("RunTime (GeoScripts.parse): " + stopWatch.Elapsed.Milliseconds + "ms");

      //stopWatch = new Stopwatch();
      // stopWatch.Start();
      //if(areaID == 1) newArea.AreaModel.dumpModelToOBJ(1.0f/500.0f);
      //stopWatch.Stop();
      //Console.WriteLine("RunTime (newArea.AreaModel.buildBuffers): " + stopWatch.Elapsed.Milliseconds + "ms");
      //Globals.DEBUG_PARSING_LEVEL_AREA = false;
      // newArea.AreaModel.outputTextureAtlasToPng("Area_"+areaID+"_TexAtlus.png");
    }

    private static void CMD_21(
        IN64Hardware<ISm64Memory> sm64Hardware,
        ref Level lvl,
        ref string desc,
        byte[] cmd) {
      Rom rom = Rom.Instance;
      byte modelId = cmd[3];
      var address = BytesToInt_(cmd, 4, 4);
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);

      desc = "Define Model ID 0x" + modelId.ToString("X2") +
             "; Load Fast3D from 0x" + seg.ToString("X2") + off.ToString("X6");

      Model3DLods newModel = new Model3DLods(sm64Hardware);

      if (rom.GetSegment(seg, sm64Hardware.Memory.AreaId) != null) {
        newModel.AddDl(address);
      }

      if (lvl.modelIDs.ContainsKey(modelId))
        lvl.modelIDs.Remove(modelId);
      lvl.modelIDs.Add(modelId, newModel);
    }

    private static void CMD_22(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd) {
      Rom rom = Rom.Instance;
      byte modelId = cmd[3];
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);

      if (modelId == 0x7A)
        Globals.debugParsingDl = true;

      desc = "Define Model ID 0x" + modelId.ToString("X2") +
             "; Load Geometry layout from 0x" + seg.ToString("X2") +
             off.ToString("X6");

      //Console.WriteLine("Size of seg 0x"+seg.ToString("X2")+" = " + rom.getSegment(seg).Length);
      Model3DLods newModel = new Model3DLods(sm64Hardware);
      if (rom.GetSegment(seg, sm64Hardware.Memory.AreaId) != null) {
        try {
          new GeoScriptsWrapper().Parse(sm64Hardware.Memory,
                                        newModel,
                                        ref lvl,
                                        seg,
                                        off);
        } catch (Exception e) {
          Console.WriteLine(e.Message);
          Console.WriteLine(e.StackTrace);
        }
      }

      var geoDataSegAddress = BytesToInt_(cmd, 4, 4);
      lvl.AddObjectCombos(modelId, geoDataSegAddress);

      if (lvl.modelIDs.ContainsKey(modelId))
        lvl.modelIDs.Remove(modelId);
      lvl.modelIDs.Add(modelId, newModel);

      if (modelId == 0x7A)
        Globals.debugParsingDl = false;
    }

    private static void CMD_24(
        IReadOnlySm64Memory n64Memory,
        ref Level lvl,
        ref string desc,
        byte[] cmd,
        byte seg,
        uint off) {
      Rom rom = Rom.Instance;
      Object3D newObj = new Object3D();
      if (rom.IsSegmentMio0(seg, n64Memory.AreaId)) {
        newObj.MakeReadOnly();
        newObj.Address = "N/A";
      } else {
        newObj.Address =
            "0x" + rom.DecodeSegmentAddress(seg, off, n64Memory.AreaId).ToString("X");
      }

      byte actMask = cmd[2];
      newObj.AllActs = (actMask == 0x1F);
      newObj.Act1 = ((actMask & 0x1) == 0x1);
      newObj.Act2 = ((actMask & 0x2) == 0x2);
      newObj.Act3 = ((actMask & 0x4) == 0x4);
      newObj.Act4 = ((actMask & 0x8) == 0x8);
      newObj.Act5 = ((actMask & 0x10) == 0x10);
      newObj.Act6 = ((actMask & 0x20) == 0x20);
      newObj.ShowHideActs(newObj.AllActs);
      newObj.ModelId = cmd[3];
      newObj.XPos = (short) BytesToInt_(cmd, 0x4, 2);
      newObj.YPos = (short) BytesToInt_(cmd, 0x6, 2);
      newObj.ZPos = (short) BytesToInt_(cmd, 0x8, 2);
      newObj.XRot = (short) BytesToInt_(cmd, 0xA, 2);
      newObj.YRot = (short) BytesToInt_(cmd, 0xC, 2);
      newObj.ZRot = (short) BytesToInt_(cmd, 0xE, 2);
      newObj.MakeBehaviorReadOnly(false);
      newObj.MakeModelIDReadOnly(false);
      newObj.SetBehaviorFromAddress(BytesToInt_(cmd, 0x14, 4));
      newObj.BehaviorParameter1 = cmd[0x10];
      newObj.BehaviorParameter2 = cmd[0x11];
      newObj.BehaviorParameter3 = cmd[0x12];
      newObj.BehaviorParameter4 = cmd[0x13];
      newObj.CreatedFromLevelScriptCommand = Object3D.FromLsCmd.CMD_24;
      newObj.Level = lvl;
      lvl.GetCurrentArea().objects.Add(newObj);

      desc = "Place Object at pos (" + newObj.XPos + "," + newObj.YPos + "," +
             newObj.ZPos + ")";
    }


    private static void CMD_26(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaId) {
      Rom rom = Rom.Instance;
      Warp warp = new Warp(false);
      if (rom.IsSegmentMio0(seg, areaId)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.DecodeSegmentAddress(seg, off, areaId).ToString("X");
      }
      warp.WarpFromId = cmd[2];
      warp.WarpToLevelId = cmd[3];
      warp.WarpToAreaId = cmd[4];
      warp.WarpToWarpId = cmd[5];
      lvl.GetCurrentArea().warps.Add(warp);
      desc = "Define warp (0x" + warp.WarpFromId.ToString("X2") + " -> ";
      if (warp.WarpToLevelId == lvl.LevelId) {
        if (warp.WarpToAreaId == lvl.GetCurrentArea().AreaId)
          desc += "0x" + warp.WarpToWarpId.ToString("X2") + ")";
        else
          desc += "0x" + warp.WarpToWarpId.ToString("X2") + " in area " +
                  warp.WarpToAreaId + ")";
      } else {
        desc += "0x" + warp.WarpToWarpId.ToString("X2") + " at level 0x" +
                warp.WarpToLevelId.ToString("X2") + " in area " +
                warp.WarpToAreaId + ")";
      }
    }

    private static void CMD_27(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaId) {
      Rom rom = Rom.Instance;
      Warp warp = new Warp(true);
      if (rom.IsSegmentMio0(seg, areaId)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.DecodeSegmentAddress(seg, off, areaId).ToString("X");
      }
      warp.WarpFromId = cmd[2];
      warp.WarpToLevelId = cmd[3];
      warp.WarpToAreaId = cmd[4];
      warp.WarpToWarpId = cmd[5];
      lvl.GetCurrentArea().paintingWarps.Add(warp);
      desc = "Define painting warp (0x" + warp.WarpFromId.ToString("X2") +
             " -> ";
      if (warp.WarpToLevelId == lvl.LevelId) {
        if (warp.WarpToAreaId == lvl.GetCurrentArea().AreaId)
          desc += "0x" + warp.WarpToWarpId.ToString("X2") + ")";
        else
          desc += "0x" + warp.WarpToWarpId.ToString("X2") + " in area " +
                  warp.WarpToAreaId + ")";
      } else {
        desc += "0x" + warp.WarpToWarpId.ToString("X2") + " at level 0x" +
                warp.WarpToLevelId.ToString("X2") + " in area " +
                warp.WarpToAreaId + ")";
      }
    }

    private static void CMD_28(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaId) {
      Rom rom = Rom.Instance;
      WarpInstant warp = new WarpInstant();
      if (rom.IsSegmentMio0(seg, areaId)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.DecodeSegmentAddress(seg, off, areaId).ToString("X");
      }
      warp.TriggerId = cmd[2];
      warp.AreaId = cmd[3];
      warp.TeleX = (short) BytesToInt_(cmd, 4, 2);
      warp.TeleY = (short) BytesToInt_(cmd, 6, 2);
      warp.TeleZ = (short) BytesToInt_(cmd, 8, 2);
      lvl.GetCurrentArea().instantWarps.Add(warp);
      desc = "Define Instant warp (";
      if (lvl.GetCurrentArea().AreaId != warp.AreaId)
        desc += "To area " + warp.AreaId + ",";

      desc += "Teleport (" + warp.TeleX + "," + warp.TeleY + "," + warp.TeleZ +
              "), Trigger = collision id 0x" +
              (warp.TriggerId + 0x1B).ToString("X2") + ")";
    }

    /* Process collision map, Special Objects, and waterboxes. */
    private static void CMD_2E(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte? areaId) {
      Rom rom = Rom.Instance;
      if (cmd.Length < 8)
        return;
      desc = "Load collision, and place special objects from address 0x" +
             BytesToInt_(cmd, 4, 4).ToString("X8");
      byte segment = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);
      byte[] data = rom.GetSegment(segment, areaId)!;
      var subCmd = (CollisionMapReader.CollisionSubCommand) BytesToInt_(data, (int) off, 2);

      // Check if the data is actually collision data.
      if (data[off] != 0x00 || data[off + 1] != 0x40)
        return;

      CollisionMap cmap = lvl.GetCurrentArea().collision;
      uint numVerts = (ushort) BytesToInt_(data, (int) off + 2, 2);

      off += 4;
      for (int i = 0; i < numVerts; i++) {
        short x = (short) BytesToInt_(data, (int) off + 0, 2);
        short y = (short) BytesToInt_(data, (int) off + 2, 2);
        short z = (short) BytesToInt_(data, (int) off + 4, 2);
        cmap.AddVertex(new Vector3(x, y, z));
        off += 6;
      }

      while (subCmd != CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_CONTINUE) {
        subCmd = (CollisionMapReader.CollisionSubCommand) BytesToInt_(data, (int) off, 2);
        //Console.WriteLine(sub_cmd.ToString("X8"));
        if (subCmd == CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_CONTINUE) break;
        //rom.printArraySection(data, (int)off, 4 + (int)collisionLength(sub_cmd));
        cmap.NewTriangleList((int) BytesToInt_(data, (int) off, 2));
        uint numTri = (ushort) BytesToInt_(data, (int) off + 2, 2);
        uint colLen = CollisionMapReader.GetLengthOfSubCommand(subCmd);
        off += 4;
        for (int i = 0; i < numTri; i++) {
          uint a = BytesToInt_(data, (int) off + 0, 2);
          uint b = BytesToInt_(data, (int) off + 2, 2);
          uint c = BytesToInt_(data, (int) off + 4, 2);
          cmap.AddTriangle(a, b, c);
          off += colLen;
        }
      }
      cmap.BuildCollisionMap();
      off += 2;
      bool end = false;
      while (!end) {
        subCmd = (CollisionMapReader.CollisionSubCommand) BytesToInt_(data, (int) off, 2);
        switch (subCmd) {
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_END:
            end = true;
            break;
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_OBJECTS:
            uint numObj = (ushort) BytesToInt_(data, (int) off + 2, 2);
            off += 4;
            for (int i = 0; i < numObj; i++) {
              ushort objId = (ushort) BytesToInt_(data, (int) off, 2);
              byte[] entry = GetSpecialObjectEntry_((byte) objId);
              uint objLen = GetSpecialObjectLength_(objId);
              Object3D newObj = new Object3D();
              if (rom.IsSegmentMio0(segment, areaId)) {
                newObj.MakeReadOnly();
                newObj.Address = "N/A";
              } else {
                newObj.Address = "0x" + rom
                                        .DecodeSegmentAddress(
                                            segment, off, areaId)
                                        .ToString("X");
              }
              newObj.SetPresetId(objId);
              newObj.Level = lvl;
              newObj.HideProperty(Object3D.Flags.ROTATION_X);
              newObj.HideProperty(Object3D.Flags.ROTATION_Z);
              newObj.HideProperty(Object3D.Flags.BPARAM_3);
              newObj.HideProperty(Object3D.Flags.BPARAM_4);
              newObj.XPos = (short) BytesToInt_(data, (int) off + 2, 2);
              newObj.YPos = (short) BytesToInt_(data, (int) off + 4, 2);
              newObj.ZPos = (short) BytesToInt_(data, (int) off + 6, 2);
              newObj.BehaviorParameter1 = entry[1];
              newObj.BehaviorParameter2 = entry[2];
              newObj.MakeBehaviorReadOnly(true);
              newObj.MakeModelIDReadOnly(true);
              if (objLen > 8) {
                newObj.YRot =
                    (short) (BytesToInt_(data, (int) off + 8, 2) * 1.40625);
                if (objLen > 10) {
                  newObj.BehaviorParameter1 = data[off + 10];
                  newObj.BehaviorParameter2 = data[off + 11];
                  newObj.CreatedFromLevelScriptCommand =
                      Object3D.FromLsCmd.CMD_2_E_12;
                  lvl.AddSpecialObjectPreset_12(objId, entry[3],
                                                BytesToInt_(entry, 4, 4),
                                                data[off + 10], data[off + 11]);
                } else {
                  lvl.AddSpecialObjectPreset_10(objId, entry[3],
                                                BytesToInt_(entry, 4, 4));
                  newObj.HideProperty(Object3D.Flags.BPARAM_1);
                  newObj.HideProperty(Object3D.Flags.BPARAM_2);
                  newObj.CreatedFromLevelScriptCommand =
                      Object3D.FromLsCmd.CMD_2_E_10;
                }
              } else {
                lvl.AddSpecialObjectPreset_8(objId, entry[3],
                                             BytesToInt_(entry, 4, 4));
                newObj.HideProperty(Object3D.Flags.BPARAM_1);
                newObj.HideProperty(Object3D.Flags.BPARAM_2);
                newObj.HideProperty(Object3D.Flags.ROTATION_Y);
                newObj.CreatedFromLevelScriptCommand =
                    Object3D.FromLsCmd.CMD_2_E_8;
              }
              newObj.ModelId = entry[3];
              uint behavior = BytesToInt_(entry, 4, 4);
              newObj.SetBehaviorFromAddress(behavior);
              newObj.DontShowActs();
              if (behavior != 0)
                lvl.GetCurrentArea().specialObjects.Add(newObj);
              off += objLen;
            }
            break;
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_ENVIRONMENT:
            // Also skipping water boxes. Will come back to it later.
            uint numBoxes = (ushort) BytesToInt_(data, (int) off + 2, 2);
            off += 4 + (numBoxes * 0xC);
            break;
        }
      }
    }

    private static void CMD_2F(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte? areaId) {
      Rom rom = Rom.Instance;
      Console.WriteLine(BytesToInt_(cmd, 4, 4).ToString("X8"));
      byte seg = cmd[4];
      uint off = BytesToInt_(cmd, 5, 3);
      uint len = lvl.GetCurrentArea().collision.GetTriangleCount();
      //byte[] data = rom.getSubArray_safe(rom.getSegment(seg, areaID), off, len);
      //Console.WriteLine("Num triangles = 0x" + len.ToString("X8") + ": ");
      //rom.printArray(data);
    }

    private static byte[] GetSpecialObjectEntry_(byte presetId) {
      Rom rom = Rom.Instance;
      byte[] data = new byte[8];
      uint offset = Globals.MemoryConstants.SpecialPresetTable;
      byte got = rom.Bytes[offset];
      while (got != 0xFF) {
        if (got == presetId) {
          Array.Copy(rom.Bytes, offset, data, 0, 8);
          break;
        }
        offset += 8;
        got = rom.Bytes[offset];
      }

      return data;
    }

    private static uint GetSpecialObjectLength_(int obj) {
      if (obj > 0x64 && obj < 0x79) return 10;
      else if (obj > 0x78 && obj < 0x7E) return 8;
      else if (obj > 0x7D && obj < 0x83) return 10;
      else if (obj > 0x88 && obj < 0x8E) return 10;
      else if (obj > 0x82 && obj < 0x8A) return 12;
      else if (obj == 0x40) return 10;
      else if (obj == 0x64) return 12;
      else if (obj == 0xC0) return 8;
      else if (obj == 0xE0) return 12;
      else if (obj == 0xCD) return 12;
      else if (obj == 0x00) return 10;
      return 8;
    }

    private static void CMD_39(
        IReadOnlySm64Memory sm64Memory,
        ref Level lvl,
        ref string desc,
        byte[] cmd) {
      if (cmd.Length < 8)
        return;
      uint pos = BytesToInt_(cmd, 4, 4);

      desc = "Place macro objects loaded from address 0x" + pos.ToString("X8");

      Rom rom = Rom.Instance;
      using var br = sm64Memory.OpenAtSegmentedAddress(pos);

      lvl.GetCurrentArea().macroObjects.Clear();
      bool endList = false;
      while (!endList) {
        //rom.printArray(data, 10);
        var firstAndSecond = br.ReadUInt16();
        uint id = (uint) (firstAndSecond & 0x1FF);
        if (id == 0 || id == 0x1E) break;
        Object3D newObj = new Object3D();
        if (rom.IsSegmentMio0(cmd[4], sm64Memory.AreaId)) {
          newObj.MakeReadOnly();
          newObj.Address = "N/A";
        } else {
          newObj.Address =
              "0x" + rom.DecodeSegmentAddress(pos, sm64Memory.AreaId)
                        .ToString("X");
        }

        uint tableOff = (id - 0x1F) * 8;
        byte[] entryData =
            rom.getSubArray_safe(rom.Bytes,
                                 Globals.MemoryConstants.MacroPresetTable + tableOff, 8);
        newObj.Level = lvl;
        newObj.CreatedFromLevelScriptCommand = Object3D.FromLsCmd.CMD_39;
        newObj.SetBehaviorFromAddress(BytesToInt_(entryData, 0, 4));
        newObj.HideProperty(Object3D.Flags.ROTATION_X);
        newObj.HideProperty(Object3D.Flags.ROTATION_Z);
        newObj.HideProperty(Object3D.Flags.BPARAM_3);
        newObj.HideProperty(Object3D.Flags.BPARAM_4);
        newObj.ModelId = entryData[5];
        newObj.SetPresetId((ushort) id);
        newObj.YRot = (short) ((firstAndSecond >> 9) * 2.8125);
        newObj.XPos = br.ReadInt16();
        newObj.YPos = br.ReadInt16();
        newObj.ZPos = br.ReadInt16();
        newObj.DontShowActs();
        newObj.MakeBehaviorReadOnly(true);
        newObj.MakeModelIDReadOnly(true);

        var bp1 = br.ReadByte();
        if (bp1 != 0)
          newObj.BehaviorParameter1 = bp1;
        else
          newObj.BehaviorParameter1 = entryData[6];

        var bp2 = br.ReadByte();
        if (bp2 != 0)
          newObj.BehaviorParameter2 = bp2;
        else
          newObj.BehaviorParameter2 = entryData[7];

        lvl.GetCurrentArea().macroObjects.Add(newObj);
      }

      //uint end = bytesToInt(cmd, 8, 4);
      //rom.setSegment(seg, start, end, false);
    }

    private static bool IsPerAreaBank0E_(byte[] segData) {
      if (segData.Length < 0x6000) return false;
      uint offset = 0x5FFC;
      return ((segData[0 + offset] << 24 | segData[1 + offset] << 16 |
               segData[2 + offset] << 8 | segData[3 + offset]) == 0x4BC9189A);
    }

    private static void SetAreaSegmented0XE_(byte areaId, byte[] segData) {
      if (!IsPerAreaBank0E_(segData))
        return;

      uint start, end;

      uint offset = 0x5F00 + (uint) areaId * 0x10;
      start = (uint) ((segData[offset] << 24) | (segData[offset + 1] << 16) |
                      (segData[offset + 2] << 8) | segData[offset + 3]);

      offset += 4;
      end = (uint) ((segData[offset] << 24) | (segData[offset + 1] << 16) |
                    (segData[offset + 2] << 8) | segData[offset + 3]);

      Rom.Instance.SetSegment(0xE, start, end, false, areaId);
    }
  }
}