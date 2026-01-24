using System;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.Phx
{
	public enum ProtoDataObjectSourceKind
	{
		NONE = PhxUtil.K_OBJECT_KIND_NONE,

		DATABASE,
		GAME_DATA,
		HP_DATA,

		SCENARIO,
		TACTIC_DATA,
		TRIGGER_SCRIPT,
		VISUAL,
	};

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ProtoDataTypeObjectSourceKindAttribute
		: Attribute
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }

		public ProtoDataTypeObjectSourceKindAttribute(ProtoDataObjectSourceKind kind)
		{
			this.SourceKind = kind;
		}
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		[Contracts.Pure]
		public static bool RequiresFileReference(this Phx.ProtoDataObjectSourceKind kind)
		{
			switch (kind)
			{
				case Phx.ProtoDataObjectSourceKind.TACTIC_DATA:
				case Phx.ProtoDataObjectSourceKind.VISUAL:
				case Phx.ProtoDataObjectSourceKind.TRIGGER_SCRIPT:
				case Phx.ProtoDataObjectSourceKind.SCENARIO:
					return true;

				default:
					return false;
			}
		}
	};
}