using System;
using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.zPatching
{
	// Patches? We don't need no stinkin- owait.
	// I hacked this together. I hope I never have to touch this again.

	public sealed class WinExePatcherProcessHeaderData
	{
		public short[] bytePattern;
		public int bytePatternNextJmpOffset;
		public int bytePatternModJmpOffset;

		public string sourceExeFileName;
		public byte[] sourceExeBytes;
		public List<int> patternFileOffsets = [];

		public int modJmpFileOffset;
		public int modJmpVa;

		public WinExePatcherProcessHeaderData()
		{
			this.bytePattern = [
				/*
					call    sub
					nop
					mov     edi, ebx
					cmp     ebx, [r14+20h]
					jnb     i32
				*/
				0xE8, /*0xD7*/-1, /*0xE5*/-1, 0xFF, 0xFF,
				0x90,
				/*0x8B*/-1, 0xFB,
				0x41, 0x3B, 0x5E, 0x20,
				0x0F, 0x83,   -1, 0x00, 0x00, 0x00

				// alignment asm bytes, can't rely on these :(
				//0x66, 0x66,
				//0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
			];
			this.bytePatternNextJmpOffset = this.bytePattern.Length - sizeof(uint);
			this.bytePatternModJmpOffset = this.bytePattern.Length - sizeof(uint) - sizeof(ushort);
		}

		public bool ReadSourceExeBytes(string sourceExeFileName)
		{
			try
			{
				this.sourceExeFileName = sourceExeFileName;
				this.sourceExeBytes = File.ReadAllBytes(this.sourceExeFileName);
			} catch (Exception ex)
			{
				Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.K_NONE
					, "Failed to read " + sourceExeFileName
					, ex);
				return false;
			}

			return true;
		}

		public bool FindPatterns()
		{
			this.patternFileOffsets.Clear();

			int offset = 0;
			while (PhxUtil.FindBytePattern(this.patternFileOffsets, this.sourceExeBytes, ref offset, this.bytePattern))
			{
			}

			return this.patternFileOffsets.Count == 1;
		}

		public bool CalculateModJmp()
		{
			this.modJmpFileOffset = this.modJmpVa = TypeExtensions.K_NONE;

			int fileOffset = this.patternFileOffsets[0];
			int nextJmpIndex = fileOffset + this.bytePatternNextJmpOffset;
			int nextJmpOffsetVa = BitConverter.ToInt32(this.sourceExeBytes, nextJmpIndex);
			int goodJmpBase = nextJmpOffsetVa;
			goodJmpBase += sizeof(uint);
			goodJmpBase += nextJmpIndex;

			// jnz short i8
			if (0x75 != this.sourceExeBytes[goodJmpBase])
				return false;
			int goodAsmOffsetVa = this.sourceExeBytes[goodJmpBase + 1];
			int goodAsmIndex = (goodJmpBase + 2);
			goodAsmIndex += goodAsmOffsetVa;

			// 0xE9 .. .. .. ..
			int modJmpBase = fileOffset + this.bytePatternModJmpOffset;
			modJmpBase += sizeof(byte) + sizeof(uint);
			int modJmpOffsetVa = goodAsmIndex;
			modJmpOffsetVa -= modJmpBase;

			this.modJmpFileOffset = fileOffset + this.bytePatternModJmpOffset;
			this.modJmpVa = modJmpOffsetVa;

			return true;
		}

		public void ApplyModJmp()
		{
			int index = this.modJmpFileOffset;
			this.sourceExeBytes[index++] = 0xE9;
			Bitwise.ByteSwap.ReplaceBytes(this.sourceExeBytes, index, this.modJmpVa);
		}
	};
}