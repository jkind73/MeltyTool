using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListArray<T> list, BListXmlParams @params)
			where T : IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using (var xs = new BListArrayXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BListArrayXmlSerializer<T>
		: BListXmlSerializerBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
		BListXmlParams mParams_;
		Collections.BListArray<T> mList_;

		public override BListXmlParams Params { get { return this.mParams_; } }
		public override Collections.BListBase<T> List { get { return this.mList_; } }

		public BListArrayXmlSerializer(BListXmlParams @params, Collections.BListArray<T> list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			this.mParams_ = @params;
			this.mList_ = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			T item = new T();
			item.Serialize(s);

			this.List.AddItem(item);
		}

		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}