using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KSoft.Phoenix.Xmb
{
	public enum XmbFileBuilderOptions
	{
		[Display(	Name="Force string encoding",
					Description="XML element and attribute values are treated as string data, even if it could be detected as a integer, bool, etc")]
		FORCE_STRING_VARIANTS,
		[Browsable(false)]
		[Display(	Name="Unicode strings are permitted",
					Description="")]
		ALLOW_UNICODE,
		[Display(	Name="Force Unicode strings",
					Description="XML element and attribute values are treated as Unicode strings")]
		FORCE_UNICODE,
		[Browsable(false)]
		LITTLE_ENDIAN,

		K_NUMBER_OF
	};

	public sealed class XmbFileBuilder
	{
		public const int K_CREATOR_TOOL_VERSION = 1;

		#region BuilderOptions
		public Collections.BitVector32 builderOptions;
		public string DebugBuilderOptions { get {
			return this.builderOptions.ToString(XmbFileBuilderOptions.K_NUMBER_OF);
		} }

		public bool ForceStringVariants { get {
			return this.builderOptions.Test(XmbFileBuilderOptions.FORCE_STRING_VARIANTS);
		} }
		public bool AllowUnicode { get {
			return this.builderOptions.Test(XmbFileBuilderOptions.ALLOW_UNICODE);
		} }
		public bool ForceUnicode { get {
			return this.builderOptions.Test(XmbFileBuilderOptions.FORCE_UNICODE);
		} }
		#endregion

		internal XmbFile xmb;

		#region Stats
		public int NumberOfElements { get; set; }
		public int NumberOfAttributes { get; set; }

		public int NumberOfInputVariants { get; set; }
		public int NumberOfRedundantVariants { get; set; }

		public int NumberOfDirectVariants { get; set; }
		public int NumberOfIndirectVariants { get; set; }

		public int NumberOfNulls { get; set; }
		public int NumberOfSingle24 { get; set; }
		public int NumberOfSingles { get; set; }
		public int NumberOfInt24 { get; set; }
		public int NumberOfInts { get; set; }
		public int NumberOfFixedPoint { get; set; }
		public int NumberOfDouble { get; set; }
		public int NumberOfBooleans { get; set; }
		public int NumberOfDirectStringAnsi { get; set; }
		public int NumberOfIndirectStringAnsi { get; set; }
		// There's no support for indirect Unicode
		public int NumberOfIndirectStringUnicode { get; set; }
		public int NumberOfVectors { get; set; }

		public int NumberOfIndirectStringBytes { get; set; }
		public int NumberOfVariantBytes { get; set; }
		#endregion

		public XmbFileBuilder()
		{
			this.builderOptions.Set(XmbFileBuilderOptions.ALLOW_UNICODE);
		}

		public string GetCreatorToolCommandLine(string xmlFileName)
		{
			var sb = new System.Text.StringBuilder();

			sb.AppendFormat("XMLCOMP -file {0}",
				xmlFileName);
			sb.Append(this.builderOptions.Test(XmbFileBuilderOptions.LITTLE_ENDIAN)==false ? "" :
				" -littleEndian");
			sb.Append(this.ForceStringVariants==false ? "" :
				" -disableNumerics");
			sb.Append(this.ForceUnicode==false ? "" :
				" -forceUnicode");

			return sb.ToString();
		}
	};
}