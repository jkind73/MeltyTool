using System;
using System.Collections.Generic;

namespace KSoft.T4
{
	public static class PrimitiveDefinitions
	{
		class BooleanCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal BooleanCodeDefinition() : base("bool", TypeCode.Boolean) { }

			public override int SizeOfInBytes => sizeof(bool);
		};

		class CharCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal CharCodeDefinition() : base("char", TypeCode.Char) { }

			public override int SizeOfInBytes => sizeof(char);
		};

		class StringCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal StringCodeDefinition() : base("string", TypeCode.String) { }

			public override int SizeOfInBytes => -1;
		};

		class KGuidCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal KGuidCodeDefinition() : base("Values.KGuid", TypeCode.Object) { }

			public override int SizeOfInBytes => 16;
		};

		#region Individual definitions
		internal static readonly NumberCodeDefinition KByte = new NumberCodeDefinition(TypeCode.Byte);
		internal static readonly NumberCodeDefinition KSByte = new NumberCodeDefinition(TypeCode.SByte);

		internal static readonly NumberCodeDefinition KUInt16 = new NumberCodeDefinition(TypeCode.UInt16);
		internal static readonly NumberCodeDefinition KInt16 = new NumberCodeDefinition(TypeCode.Int16);

		internal static readonly NumberCodeDefinition KUInt32 = new NumberCodeDefinition(TypeCode.UInt32);
		internal static readonly NumberCodeDefinition KInt32 = new NumberCodeDefinition(TypeCode.Int32);

		internal static readonly NumberCodeDefinition KUInt64 = new NumberCodeDefinition(TypeCode.UInt64);
		internal static readonly NumberCodeDefinition KInt64 = new NumberCodeDefinition(TypeCode.Int64);

		internal static readonly NumberCodeDefinition KSingle = new NumberCodeDefinition(TypeCode.Single);
		internal static readonly NumberCodeDefinition KDouble = new NumberCodeDefinition(TypeCode.Double);

		internal static readonly PrimitiveCodeDefinition KBool = new BooleanCodeDefinition();

		internal static readonly PrimitiveCodeDefinition KChar = new CharCodeDefinition();

		internal static readonly PrimitiveCodeDefinition KString = new StringCodeDefinition();

		internal static readonly PrimitiveCodeDefinition KKGuid = new KGuidCodeDefinition();
		#endregion

		/// <summary>All primitive type definitions that are numeric</summary>
		public static IEnumerable<NumberCodeDefinition> Numbers { get {
			yield return KByte;
			yield return KSByte;

			yield return KUInt16;
			yield return KInt16;

			yield return KUInt32;
			yield return KInt32;

			yield return KUInt64;
			yield return KInt64;

			yield return KSingle;
			yield return KDouble;
		} }

		/// <summary>All primitive type definitions (sans String)</summary>
		public static IEnumerable<PrimitiveCodeDefinition> Primitives { get {
			yield return KBool;

			yield return KChar;

			yield return KByte;
			yield return KSByte;

			yield return KUInt16;
			yield return KInt16;

			yield return KUInt32;
			yield return KInt32;

			yield return KUInt64;
			yield return KInt64;

			yield return KSingle;
			yield return KDouble;
		} }
	};
}
