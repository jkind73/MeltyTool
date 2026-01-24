using System;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileUtilOptions
	{
		DUMP_DEBUG_INFO,
		SKIP_VERIFICATION,
		/// <summary>Built for 64-bit builds</summary>
		X64,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public abstract class EcfFileUtil
		: IDisposable
	{
		public EcfFileDefinition EcfDefinition { get; private set; }
		internal EcfFile mEcfFile;
		protected string mSourceFile; // filename of the source file which the util stems from
		public TextWriter ProgressOutput { get; set; }
		public TextWriter VerboseOutput { get; set; }
		public TextWriter DebugOutput { get; set; }

		/// <see cref="EcfFileUtilOptions"/>
		public Collections.BitVector32 options = new Collections.BitVector32();

		protected EcfFileUtil()
		{
			this.EcfDefinition = new EcfFileDefinition();

			if (System.Diagnostics.Debugger.IsAttached)
				this.ProgressOutput = Console.Out;
			if (System.Diagnostics.Debugger.IsAttached)
				this.VerboseOutput = Console.Out;
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			this.ProgressOutput = null;
			this.VerboseOutput = null;
			this.DebugOutput = null;
			Util.DisposeAndNull(ref this.mEcfFile);
		}
		#endregion
	};
}