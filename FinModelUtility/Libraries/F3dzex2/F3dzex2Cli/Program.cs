using System.Globalization;
using System.Text.RegularExpressions;

using CommunityToolkit.Diagnostics;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;

using fin.util.strings;

var hexRegex = new Regex("[a-fA-F0-9]+");

while (true) {
  Console.ForegroundColor = ConsoleColor.White;
  Console.WriteLine("Enter DL commands as hex, or an empty string to exit");

  var line = Console.ReadLine();
  if (line == null || line.Trim().Length == 0) {
    continue;
  }

  line = line.Replace("0x", "", StringComparison.OrdinalIgnoreCase);
  line = Regex.Replace(line, "\\s", "");

  if (!hexRegex.IsMatch(line)) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(
        "Invalid command, contained unexpected character");
    continue;
  }

  if (line.Length % 8 != 0) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(
        "Invalid command, expected length to be multiple of 8");
    continue;
  }

  var lineSpan = line.AsSpan();
  var bytes = new byte[line.Length / 2];
  for (var i = 0; i < bytes.Length; ++i) {
    bytes[i] = byte.Parse(lineSpan.Slice(2 * i, 2), NumberStyles.HexNumber);
  }

  Console.ForegroundColor = ConsoleColor.Cyan;
  Console.WriteLine(
      $"Parsing bytes: {bytes.Select(b => b.ToHexString()).Join(", ")}");

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

  using var br = n64Memory.OpenAtSegmentedAddress(0);

  Console.ForegroundColor = ConsoleColor.Green;
  while (!br.Eof) {
    var opcode = f3dzex2OpcodeParser.Parse(n64Memory, displayListReader, br);
    Console.WriteLine(opcode);
  }
}