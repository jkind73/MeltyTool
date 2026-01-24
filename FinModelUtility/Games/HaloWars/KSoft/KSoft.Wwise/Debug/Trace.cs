using Diag = System.Diagnostics;

namespace KSoft.Wwise.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kWwiseSource_
			, kFilePackageSource_
			;

		static Trace()
		{
			kWwiseSource_ = new			Diag.TraceSource("KSoft.Wwise",				Diag.SourceLevels.All);
			kFilePackageSource_ = new	Diag.TraceSource("KSoft.Wwise.FilePackage",	Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="KSoft.Wwise"/> namespace</summary>
		public static Diag.TraceSource Wwise		{ get { return kWwiseSource_; } }
		/// <summary>Tracer for the <see cref="KSoft.Wwise.FilePackage"/> namespace</summary>
		public static Diag.TraceSource FilePackage	{ get { return kFilePackageSource_; } }
	};
}
