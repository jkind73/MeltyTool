namespace sm64.src {
  class AssemblyReader {
    public List<JalCall> FindJaLsInFunction(uint ramFunc, uint raMtoRom) {
      List<JalCall> calls = [];
      List<Instruction> inst = this.ReadFunction(ramFunc, raMtoRom);
      uint a0 = 0, a1 = 0, a2 = 0, a3 = 0;
      uint jalAddr = 0;
      bool addNextTime = false;
      for (int i = 0; i < inst.Count; i++) {
        bool addJal = addNextTime;
        switch (inst[i].opCode) {
          case Opcode.LUI:
            if (inst[i].gpDest == GpRegister.A0)
              a0 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gpDest == GpRegister.A1)
              a1 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gpDest == GpRegister.A2)
              a2 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gpDest == GpRegister.A3)
              a3 = (uint) (inst[i].immediate << 16);
            break;
          case Opcode.ADDIU:
            if (inst[i].gpDest == GpRegister.A0 &&
                inst[i].gp1 == GpRegister.A0)
              a0 += (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A1 &&
                     inst[i].gp1 == GpRegister.A1)
              a1 += (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A2 &&
                     inst[i].gp1 == GpRegister.A2)
              a2 += (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A3 &&
                     inst[i].gp1 == GpRegister.A3)
              a3 += (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A0 &&
                     inst[i].gp1 == GpRegister.R0)
              a0 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A1 &&
                     inst[i].gp1 == GpRegister.R0)
              a1 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A2 &&
                     inst[i].gp1 == GpRegister.R0)
              a2 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A3 &&
                     inst[i].gp1 == GpRegister.R0)
              a3 = (uint) inst[i].immediate;
            else {
              if (inst[i].gpDest == GpRegister.A0)
                a0 = (uint) (inst[i].immediate +
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              else if (inst[i].gpDest == GpRegister.A1)
                a1 = (uint) (inst[i].immediate +
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              else if (inst[i].gpDest == GpRegister.A2)
                a2 = (uint) (inst[i].immediate +
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              else if (inst[i].gpDest == GpRegister.A3)
                a3 = (uint) (inst[i].immediate +
                             this.gpRegisterValues_[(int) inst[i].gp1]);
            }

            break;
          case Opcode.ORI:
            if (inst[i].gpDest == GpRegister.A0 &&
                inst[i].gp1 == GpRegister.A0)
              a0 |= (ushort) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A1 &&
                     inst[i].gp1 == GpRegister.A1)
              a1 |= (ushort) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A2 &&
                     inst[i].gp1 == GpRegister.A2)
              a2 |= (ushort) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A3 &&
                     inst[i].gp1 == GpRegister.A3)
              a3 |= (ushort) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A0 &&
                     inst[i].gp1 == GpRegister.R0)
              a0 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A1 &&
                     inst[i].gp1 == GpRegister.R0)
              a1 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A2 &&
                     inst[i].gp1 == GpRegister.R0)
              a2 = (uint) inst[i].immediate;
            else if (inst[i].gpDest == GpRegister.A3 &&
                     inst[i].gp1 == GpRegister.R0)
              a3 = (uint) inst[i].immediate;
            else {
              if (inst[i].gpDest == GpRegister.A0) {
                uint immediateUnsigned = (uint) inst[i].immediate;
                a0 = (uint) (immediateUnsigned |
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              } else if (inst[i].gpDest == GpRegister.A1) {
                uint immediateUnsigned = (uint) inst[i].immediate;
                a1 = (uint) (immediateUnsigned |
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              } else if (inst[i].gpDest == GpRegister.A2) {
                uint immediateUnsigned = (uint) inst[i].immediate;
                a2 = (uint) (immediateUnsigned |
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              } else if (inst[i].gpDest == GpRegister.A3) {
                uint immediateUnsigned = (uint) inst[i].immediate;
                a3 = (uint) (immediateUnsigned |
                             this.gpRegisterValues_[(int) inst[i].gp1]);
              }
            }
            break;
          case Opcode.JAL:
            jalAddr = inst[i].jumpToFunc;
            addNextTime = true;
            break;
        }

        if (addJal) {
          JalCall newCall = new JalCall();
          newCall.a0 = a0;
          newCall.a1 = a1;
          newCall.a2 = a2;
          newCall.a3 = a3;
          newCall.jalAddress = jalAddr;
          calls.Add(newCall);
          //Console.WriteLine(newCall.ToString());
          addNextTime = false;
        }
      }

      return calls;
    }

    public List<Instruction> ReadFunction(uint ramAddr, uint raMtoRom) {
      List<Instruction> instructions = [];
      Rom rom = Rom.Instance;
      uint romOffset = ramAddr - raMtoRom;
      uint offset = romOffset;
      int end = 0x8000;
      while (end > 0) {
        Instruction cur = this.ParseInstruction_(rom.ReadWordUnsigned(offset));
        offset += 4;
        end--;
        if (cur.opCode == Opcode.JR && cur.gp1 == GpRegister.RA)
          end = 1;
        instructions.Add(cur);
      }
      return instructions;
    }

    private Instruction ParseInstruction_(uint data) {
      Instruction inst = new Instruction();
      if (data == 0) {
        inst.opCode = Opcode.NOP;
        return inst;
      }

      uint opCode = data >> 26, func = 0;
      switch (opCode) {
        case 0x00:
          func = data & 0x3F;
          switch (func) {
            case 0x08:
              inst.opCode = Opcode.JR;
              inst.gp1 = (GpRegister) ((data >> 21) & 0x1F);
              break;
            default:
              inst.opCode = Opcode.DO_NOT_CARE;
              break;
          }
          break;
        case 0x03:
          inst.opCode = Opcode.JAL;
          inst.jumpToFunc = 0x80000000 + ((data & 0x3FFFFFF) << 2);
          break;
        case 0x09:
          inst.opCode = Opcode.ADDIU;
          inst.gpDest = (GpRegister) ((data >> 16) & 0x1F);
          inst.gp1 = (GpRegister) ((data >> 21) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);
          this.gpRegisterValues_[(int) inst.gpDest] = this.gpRegisterValues_[(int) inst.gp1] + inst.immediate;
          break;
        case 0x0D:
          inst.opCode = Opcode.ORI;
          inst.gpDest = (GpRegister) ((data >> 16) & 0x1F);
          inst.gp1 = (GpRegister) ((data >> 21) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);

          this.gpRegisterValues_[(int) inst.gpDest] = this.gpRegisterValues_[(int) inst.gp1] | (long) inst.immediate;
          break;
        case 0x0F:
          inst.opCode = Opcode.LUI;
          inst.gpDest = (GpRegister) ((data >> 16) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);
          this.gpRegisterValues_[(int) inst.gpDest] = (long) inst.immediate << 16;
          break;
        default:
          inst.opCode = Opcode.DO_NOT_CARE;
          break;
      }

      return inst;
    }


    long[] gpRegisterValues_ = new long[0x32];

    // General-Purpose Registers
    public enum GpRegister {
      R0, // Constant 0
      AT, // Used for psuedo-instructions
      V0,
      V1, // Function returns
      A0,
      A1,
      A2,
      A3, // Function Arguments
      T0,
      T1,
      T2,
      T3,
      T4,
      T5,
      T6,
      T7, // Temporary
      S0,
      S1,
      S2,
      S3,
      S4,
      S5,
      S6,
      S7, // Saved
      T8,
      T9, // More temporary
      K0,
      K1, // Reserved for Kernal (do not use)
      GP, // Global area pointer
      SP, // Stack pointer
      FP, // Frame pointer
      RA // Return address
    }

    // Floating-Point Registers
    public enum FpRegister {
      F0,
      F1,
      F2,
      F3, // Function returns
      F4,
      F5,
      F6,
      F7,
      F8,
      F9,
      F10,
      F11, // Temporary
      F12,
      F13,
      F14,
      F15, // Function arguments
      F16,
      F17,
      F18,
      F19, // More Temporary
      F20,
      F21,
      F22,
      F23,
      F24,
      F25,
      F26,
      F27,
      F28,
      F29,
      F30,
      F31 // Saved
    }

    public enum Opcode {
      LUI,
      ADDIU,
      ORI,
      JAL,
      JR,
      NOP,
      DO_NOT_CARE
    }

    public sealed class JalCall {
      public uint jalAddress = 0, a0 = 0, a1 = 0, a2 = 0, a3 = 0;

      public override string ToString() {
        return this.jalAddress.ToString("X8") + "("
                                               +
                                               this.a0.ToString("X8") + ", "
                                               +
                                               this.a1.ToString("X8") + ", "
                                               +
                                               this.a2.ToString("X8") + ", "
                                               +
                                               this.a3.ToString("X8") + ")";
      }
    }

    public sealed class Instruction {
      public Opcode opCode;
      public GpRegister gp1, gpDest;
      public short immediate = 0;
      public uint jumpToFunc = 0;

      public override string ToString() {
        switch (this.opCode) {
          case Opcode.JR:
            return "JR " + this.gp1.ToString();
          case Opcode.LUI:
            return "LUI " +
                   this.gpDest.ToString() + " 0x" +
                   this.immediate.ToString("X4");
          case Opcode.ADDIU:
            return "ADDIU " +
                   this.gpDest.ToString() + " " +
                   this.gp1.ToString() +
                   " 0x" +
                   this.immediate.ToString("X4");
          case Opcode.JAL:
            return "JAL 0x" + this.jumpToFunc.ToString("X8");
        }
        return this.opCode.ToString();
      }
    }
  }
}