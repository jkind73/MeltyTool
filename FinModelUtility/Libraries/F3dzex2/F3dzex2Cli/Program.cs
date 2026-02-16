using System.Globalization;
using System.Text.RegularExpressions;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;

public static class Program {
  public static void Main() {
    while (true) {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("Enter DL commands as hex, or an empty string to exit");

      var line = Console.ReadLine();
      if (line == null || line.Trim().Length == 0) {
        return;
      }

      line = line.Replace("0x", "", StringComparison.OrdinalIgnoreCase);
      line = Regex.Replace(line, "\\s", "");

      if (line.Length % 8 != 0) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(
            "Invalid command, expected length to be multiple of 8");
        continue;
      }

      var lineSpan = line.AsSpan();
      var bytes = new byte[line.Length / 2];
      for (var i = 0; i < bytes.Length; ++i) {
        bytes[i] = byte.Parse(lineSpan.Slice(i, 2), NumberStyles.HexNumber);
      }

      var n64Hardware = new N64Hardware<N64Memory>();
      n64Hardware.Rdp = new Rdp {
          Tmem = new NoclipTmem(n64Hardware),
      };
      n64Hardware.Rsp = new Rsp {
          GeometryMode = GeometryMode.G_LIGHTING,
      };
      var n64Memory = n64Hardware.Memory = new N64Memory(bytes);
      n64Memory.SetSegment(0, 0, (uint) bytes.Length);

      var displayListReader = new DisplayListReader();
      var f3dzex2OpcodeParser = new F3dzex2OpcodeParser();
      var displayList = displayListReader.ReadDisplayList(
              n64Hardware.Memory,
              f3dzex2OpcodeParser,
              0);

      Console.ForegroundColor = ConsoleColor.Cyan;
      foreach (var opcode in displayList.OpcodeCommands) {
        Console.WriteLine(opcode);
      }
    }
  }
}