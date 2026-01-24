using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory
{
	/// <summary>
	/// Specialized stack for dealing with "Physical Addresses" (PAs) and translating them into virtual addresses
	/// (VAs) when serialized to a stream, and from VAs to PAs when serialized from a stream
	/// </summary>
	/// <remarks>Should only be instanced and used directly be the EndianStream classes</remarks>
	class VirtualAddressTranslationStack
		: Stack<Values.PtrHandle>
	{
		const int K_DEFAULT_CAPACITY_ = 8;

		readonly Values.PtrHandle mNull_;
		Values.PtrHandle mCurrentPa_;
		/// <summary>The top PA on the stack</summary>
		public Values.PtrHandle CurrentAddress { get { return this.mCurrentPa_; } }

		#region Ctor
		public VirtualAddressTranslationStack(Shell.ProcessorSize ptrSize)
			: this(ptrSize, K_DEFAULT_CAPACITY_)
		{
		}

		public VirtualAddressTranslationStack(Shell.ProcessorSize ptrSize, int capacity)
			: base(capacity != 0 ? capacity : K_DEFAULT_CAPACITY_)
		{
			Contract.Requires<ArgumentOutOfRangeException>(
				ptrSize == Shell.ProcessorSize.X32 || ptrSize == Shell.ProcessorSize.X64);

			switch (ptrSize)
			{
			case Shell.ProcessorSize.X32:
				this.mNull_ = Values.PtrHandle.Null32; break;
			case Shell.ProcessorSize.X64:
				this.mNull_ = Values.PtrHandle.Null64; break;
			}

			this.mCurrentPa_ = this.mNull_;
		}
		#endregion

		#region Stack
		/// <summary>Push a new PA, setting <see cref="CurrentAddress"/> in the process</summary>
		/// <param name="pa">PA to push</param>
		public void PushPhysicalAddress(Values.PtrHandle pa)
		{
			this.mCurrentPa_ = pa;

			this.Push(pa);
		}
		/// <summary>Push a new PA that is a relative offset of <see cref="CurrentAddress"/></summary>
		/// <remarks>So, CurrentAddress += offset</remarks>
		/// <param name="relativeOffset">offset relative to <see cref="CurrentAddress"/></param>
		public void PushPhysicalAddressOffset(Values.PtrHandle relativeOffset)
		{
			var newPa = this.CurrentAddress + relativeOffset;

			this.PushPhysicalAddress(newPa);
		}
		/// <summary>Push the null identifier on to the PA stack</summary>
		/// <remarks><see cref="CurrentAddress"/> gets set to the null identifier as well</remarks>
		public void PushNull()
		{
			this.PushPhysicalAddress(this.mNull_);
		}

		/// <summary>
		/// Pop and return the top PA on the stack. If no other PAs are left, <see cref="CurrentAddress"/> is set to null
		/// </summary>
		/// <returns></returns>
		public Values.PtrHandle PopPhysicalAddress()
		{
			var pop = this.Pop();

			if (this.Count != 0)
				this.mCurrentPa_ = this.Peek();
			else
				this.mCurrentPa_ = this.mNull_;

			return pop;
		}
		#endregion

		#region IO
		/// <summary>Read a VA from a stream, and translate it into a PA</summary>
		/// <param name="s">Stream to read from</param>
		/// <returns>VA + <see cref="CurrentAddress"/></returns>
		/// <remarks>If the VA read is a <see cref="PtrHandle.IsInvalidHandle">InvalidHandle</see>, it is returned without fix-up</remarks>
		public Values.PtrHandle ReadVirtualAsPhysicalAddress(IO.EndianReader s)
		{
			Values.PtrHandle va = this.mNull_;
			s.ReadRawPointer(ref va);

			if (va.IsInvalidHandle)
				return va;

			return this.CurrentAddress + va;
		}
		/// <summary>Translate a PA to a VA and write it to a stream</summary>
		/// <param name="s">Stream to write to</param>
		/// <param name="pa">PA to translate to a VA (ie, PA - <see cref="CurrentAddress"/>)</param>
		/// <remarks>If <paramref name="pa"/> is a <see cref="PtrHandle.IsInvalidHandle">InvalidHandle</see>, it streamed without fix-up</remarks>
		public void WritePhysicalAsVirtualAddress(IO.EndianWriter s, Values.PtrHandle pa)
		{
			var va = pa.IsInvalidHandle
				? pa
				: pa - this.CurrentAddress;

			s.WriteRawPointer(va);
		}
		#endregion
	};
}