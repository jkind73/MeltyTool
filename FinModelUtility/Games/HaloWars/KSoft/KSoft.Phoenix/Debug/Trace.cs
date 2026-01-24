using Diag = System.Diagnostics;

namespace KSoft.Phoenix.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kPhoenixSource_
			, kEngineSource_
			, kResourceSource_
			, kSecuritySource_
			, kTriggerSystemSource_
			, kXmlSource_
			;

		static Trace()
		{
			kPhoenixSource_ = new		Diag.TraceSource("KSoft.Phoenix",			Diag.SourceLevels.All);
			kEngineSource_ = new			Diag.TraceSource("KSoft.Phoenix.Engine",	Diag.SourceLevels.All);
			kResourceSource_ = new		Diag.TraceSource("KSoft.Phoenix.Resource",	Diag.SourceLevels.All);
			kSecuritySource_ = new		Diag.TraceSource("KSoft.Security",			Diag.SourceLevels.All);
			kTriggerSystemSource_ = new	Diag.TraceSource("KSoft.Phoenix.Triggers",	Diag.SourceLevels.All);
			kXmlSource_ = new			Diag.TraceSource("KSoft.Phoenix.XML",		Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="KSoft.Phoenix"/> namespace</summary>
		public static Diag.TraceSource Phoenix		{ get { return kPhoenixSource_; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Engine"/> namespace</summary>
		public static Diag.TraceSource Engine		{ get { return kEngineSource_; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Resource"/> namespace</summary>
		public static Diag.TraceSource Resource		{ get { return kResourceSource_; } }
		/// <summary>Tracer for the <see cref="KSoft.Security"/> namespace</summary>
		public static Diag.TraceSource Security		{ get { return kSecuritySource_; } }
		/// <summary>Tracer for the Trigger System related code</summary>
		public static Diag.TraceSource TriggerSystem{ get { return kTriggerSystemSource_; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.XML"/> namespace</summary>
		public static Diag.TraceSource Xml			{ get { return kXmlSource_; } }
	};
}
