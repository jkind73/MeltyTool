using System.Collections.Generic;

using f3dzex2.displaylist.opcodes;

namespace f3dzex2.displaylist;

public enum DisplayListType {
  FAST_3D,
  F_3DZEX2,
}

public interface IDisplayList {
  IReadOnlyList<IOpcodeCommand> OpcodeCommands { get; }
  DisplayListType Type { get; }
}