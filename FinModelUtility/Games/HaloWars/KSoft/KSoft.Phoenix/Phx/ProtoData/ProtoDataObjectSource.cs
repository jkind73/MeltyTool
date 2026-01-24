#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public sealed class ProtoDataObjectSource
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }
		public Engine.XmlFileInfo FileReference { get; private set; }

		public ProtoDataObjectSource(ProtoDataObjectSourceKind kind, Engine.XmlFileInfo fileReference)
		{
			Contract.Requires(kind != ProtoDataObjectSourceKind.NONE);
			Contract.Requires(!kind.RequiresFileReference() || fileReference != null);

			this.SourceKind = kind;
			this.FileReference = fileReference;
		}

		public override string ToString()
		{
			if (this.FileReference == null)
				return this.SourceKind.ToString();

			return string.Format("{0} - {1}",
			                     this.SourceKind,
			                     this.FileReference);
		}

		public ProtoDataObjectDatabase GetObjectDatabase(Engine.PhxEngine engine)
		{
			switch (this.SourceKind)
			{
				case ProtoDataObjectSourceKind.DATABASE:
					return new ProtoDataObjectDatabase(engine.Database, typeof(DatabaseObjectKind));

				case ProtoDataObjectSourceKind.GAME_DATA:
					return new ProtoDataObjectDatabase(engine.Database.GameData, typeof(GameDataObjectKind));

				case ProtoDataObjectSourceKind.HP_DATA:
					return new ProtoDataObjectDatabase(engine.Database.HpBars, typeof(HpBarDataObjectKind));

				default:
					throw new System.NotImplementedException(string.Format(
						nameof(this.GetObjectDatabase) + " needs support for {0}",
						this));
			}
		}
	};
}