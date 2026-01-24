using System.Numerics;

namespace sm64.Scripts {
  public static class CollisionMapReader {
    public enum CollisionSubCommand : ushort {
      TERRAIN_LOAD_VERTICES =
          0x0040, // Begins vertices list for collision triangles

      TERRAIN_LOAD_CONTINUE =
          0x0041, // Stop loading vertices but continues to load other collision commands
      TERRAIN_LOAD_END = 0x0042, // End the collision list
      TERRAIN_LOAD_OBJECTS = 0x0043, // Loads in certain objects for level start
      TERRAIN_LOAD_ENVIRONMENT = 0x0044, // Loads water/HMC gas
    }

    public static CollisionMap Load(uint address) {
      var cmap = new CollisionMap();
      LoadInto(cmap, address);
      return cmap;
    }

    public static void LoadInto(CollisionMap cmap, uint address) {
      var rom = Rom.Instance;

      var segment = (ushort) (address >> 24);
      uint off = address & 0xFFFFFF;
      byte[] data = rom.GetSegment(segment, null)!;
      var subCmd =
          (CollisionSubCommand) BitLogic.BytesToInt(data, (int) off, 2);

      // Check if the data is actually collision data.
      if (data[off] != 0x00 || data[off + 1] != 0x40)
        return;

      uint numVerts = (ushort) BitLogic.BytesToInt(data, (int) off + 2, 2);

      off += 4;
      for (int i = 0; i < numVerts; i++) {
        short x = (short) BitLogic.BytesToInt(data, (int) off + 0, 2);
        short y = (short) BitLogic.BytesToInt(data, (int) off + 2, 2);
        short z = (short) BitLogic.BytesToInt(data, (int) off + 4, 2);
        cmap.AddVertex(new Vector3(x, y, z));
        off += 6;
      }

      while (subCmd != CollisionSubCommand.TERRAIN_LOAD_CONTINUE) {
        subCmd = (CollisionSubCommand) BitLogic.BytesToInt(data, (int) off, 2);
        //Console.WriteLine(sub_cmd.ToString("X8"));
        if (subCmd == CollisionSubCommand.TERRAIN_LOAD_CONTINUE) break;
        //rom.printArraySection(data, (int)off, 4 + (int)collisionLength(sub_cmd));
        cmap.NewTriangleList((int) BitLogic.BytesToInt(data, (int) off, 2));
        uint numTri = (ushort) BitLogic.BytesToInt(data, (int) off + 2, 2);
        uint colLen = GetLengthOfSubCommand(subCmd);
        off += 4;
        for (int i = 0; i < numTri; i++) {
          uint a = BitLogic.BytesToInt(data, (int) off + 0, 2);
          uint b = BitLogic.BytesToInt(data, (int) off + 2, 2);
          uint c = BitLogic.BytesToInt(data, (int) off + 4, 2);
          cmap.AddTriangle(a, b, c);
          off += colLen;
        }
      }
      cmap.BuildCollisionMap();
      off += 2;
      bool end = false;
      while (!end) {
        subCmd = (CollisionSubCommand) BitLogic.BytesToInt(data, (int) off, 2);
        switch (subCmd) {
          case CollisionSubCommand.TERRAIN_LOAD_END:
            end = true;
            break;
          case CollisionSubCommand.TERRAIN_LOAD_OBJECTS:
            throw new NotImplementedException();
          case CollisionSubCommand.TERRAIN_LOAD_ENVIRONMENT:
            // TODO: Handle water and gas boxes
            // Also skipping water boxes. Will come back to it later.
            uint numBoxes =
                (ushort) BitLogic.BytesToInt(data, (int) off + 2, 2);
            off += 4 + (numBoxes * 0xC);
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }

    public static uint GetLengthOfSubCommand(CollisionSubCommand type) {
      switch ((int) type) {
        case 0x0E:
        case 0x24:
        case 0x25:
        case 0x27:
        case 0x2C:
        case 0x2D:
        case 0x40:
          return 8;
        default:
          return 6;
      }
    }
  }
}