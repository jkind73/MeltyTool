using System;

namespace KSoft.IO
{
	partial class EndianReader
	{
		#region VirtualAddressTranslation
		Memory.VirtualAddressTranslationStack mVat_;

		/// <summary>Verify the state of the VAT (is it initialized?)</summary>
		void VerifyVat()
		{
			if (this.mVat_ == null)
				throw new InvalidOperationException("VAT uninitialized");
		}
		/// <summary>Initialize the VAT with a specific handle size and initial table capacity</summary>
		/// <param name="vaSize">Handle size</param>
		/// <param name="translationCapacity">The initial table capacity</param>
		public void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0)
		{
			if (this.mVat_ == null)
			{
				this.mVat_ = new Memory.VirtualAddressTranslationStack(vaSize, translationCapacity);
				this.mVat_.PushNull(); // implicitly use null as our initial VA translator
			}
		}
		/// <summary>Push a PA into to the VAT table, setting the current PA in the process</summary>
		/// <param name="physicalAddress">PA to push and to use as the VAT's current address</param>
		public void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress)
		{
			this.VerifyVat();

			this.mVat_.PushPhysicalAddress(physicalAddress);
		}
		/// <summary>Push the stream's position (as a physical address) into the VAT table</summary>
		public void VirtualAddressTranslationPushPosition()
		{
			this.VirtualAddressTranslationPush(this.PositionPtr);
		}
		/// <summary>Increase the current address (PA) by a relative offset</summary>
		/// <param name="relativeOffset">Offset, relative to the current address</param>
		public void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset)
		{
			this.VerifyVat();

			this.mVat_.PushPhysicalAddressOffset(relativeOffset);
		}
		/// <summary>Pop and return the current address (PA) in the VAT table</summary>
		/// <returns>The VAT's current address value before this call</returns>
		public Values.PtrHandle VirtualAddressTranslationPop()
		{
			this.VerifyVat();

			if (this.mVat_.Count == 1)
				throw new InvalidOperationException("Pop underflow");

			return this.mVat_.PopPhysicalAddress();
		}
		#endregion
	};

	partial class EndianWriter
	{
		#region VirtualAddressTranslation
		Memory.VirtualAddressTranslationStack mVat_;

		/// <summary>Verify the state of the VAT (is it initialized?)</summary>
		void VerifyVat()
		{
			if (this.mVat_ == null)
				throw new InvalidOperationException("VAT uninitialized");
		}
		/// <summary>Initialize the VAT with a specific handle size and initial table capacity</summary>
		/// <param name="vaSize">Handle size</param>
		/// <param name="translationCapacity">The initial table capacity</param>
		public void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0)
		{
			if (this.mVat_ == null)
			{
				this.mVat_ = new Memory.VirtualAddressTranslationStack(vaSize, translationCapacity);
				this.mVat_.PushNull(); // implicitly use null as our initial VA translator
			}
		}
		/// <summary>Push a PA into to the VAT table, setting the current PA in the process</summary>
		/// <param name="physicalAddress">PA to push and to use as the VAT's current address</param>
		public void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress)
		{
			this.VerifyVat();

			this.mVat_.PushPhysicalAddress(physicalAddress);
		}
		/// <summary>Push the stream's position (as a physical address) into the VAT table</summary>
		public void VirtualAddressTranslationPushPosition()
		{
			this.VirtualAddressTranslationPush(this.PositionPtr);
		}
		/// <summary>Increase the current address (PA) by a relative offset</summary>
		/// <param name="relativeOffset">Offset, relative to the current address</param>
		public void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset)
		{
			this.VerifyVat();

			this.mVat_.PushPhysicalAddressOffset(relativeOffset);
		}
		/// <summary>Pop and return the current address (PA) in the VAT table</summary>
		/// <returns>The VAT's current address value before this call</returns>
		public Values.PtrHandle VirtualAddressTranslationPop()
		{
			this.VerifyVat();

			if (this.mVat_.Count == 1)
				throw new InvalidOperationException("Pop underflow");

			return this.mVat_.PopPhysicalAddress();
		}
		#endregion
	};

}