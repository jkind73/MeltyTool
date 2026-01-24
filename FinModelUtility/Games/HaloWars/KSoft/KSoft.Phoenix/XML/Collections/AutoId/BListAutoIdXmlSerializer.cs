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
		public static IBListAutoIdXmlSerializer CreateXmlSerializer<T>(Collections.BListAutoId<T> list, BListXmlParams @params)
			where T : class, Collections.IListAutoIdObject, new()
		{
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			var xs = new BListAutoIdXmlSerializer<T>(@params, list);

			return xs;
		}

		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListAutoId<T> list, BListXmlParams @params, bool forceNoRootElementStreaming = false)
			where T : class, Collections.IListAutoIdObject, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(true);
			using (var xs = CreateXmlSerializer(list, @params))
			{
				xs.Serialize(s);
			}
			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(false);
		}

		public static void SerializePreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.StreamPreload(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.Serialize(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
		public static void SerializeUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.StreamUpdate(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
	};

	internal sealed class BListAutoIdXmlSerializer<T>
		: BListXmlSerializerBase<T>
		, IBListAutoIdXmlSerializer
		where T : class, Collections.IListAutoIdObject, new()
	{
		BListXmlParams mParams_;
		Collections.BListAutoId<T> mList_;

		public override BListXmlParams Params { get { return this.mParams_; } }
		public override Collections.BListBase<T> List { get { return this.mList_; } }

		public BListAutoIdXmlSerializer(BListXmlParams @params, Collections.BListAutoId<T> list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			this.mParams_ = @params;
			this.mList_ = list;
		}

		bool mIsPreloaded_;
		bool RequiresDataNamePreloading { get { return this.Params.RequiresDataNamePreloading; } }

		int mCountBeforeUpdate_;
		bool mIsUpdating_;

		#region Database interfaces
		bool SetupItem(out T item, string itemName, int iteration)
		{
			bool streamItem = !this.RequiresDataNamePreloading || (this.RequiresDataNamePreloading && this.mIsPreloaded_);

			if (this.mIsUpdating_)
			{
				// The update system in HW is fucked...just because the "update" attribute is true or left out, doesn't mean the value existed before or is not a new value
				// So just try
				int idx = this.mList_.TryGetMemberId(itemName);
				if (idx.IsNotNone())
				{
					item = this.mList_[idx];
					return streamItem;
				}

				iteration += this.mCountBeforeUpdate_;
			}

			if (this.RequiresDataNamePreloading && this.mIsPreloaded_)
			{
				item = this.mList_[iteration];
				return streamItem;
			}

			this.mList_.DynamicAdd(item = new T(), itemName, iteration);

			return streamItem;
		}
		#endregion

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			string itemName = null;
			this.Params.StreamDataName(s, ref itemName);

			T item;
			if (this.SetupItem(out item, itemName, iteration))
				item.Serialize(s);
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			string itemName = data.Data;
			if (itemName != null)
				this.Params.StreamDataName(s, ref itemName);

			try
			{
				data.Serialize(s);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format("Failed to write {0}", itemName),
					ex);
			}
		}
		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			base.WriteNodes(s, xs);

			ProtoEnumUndefinedMembers.Write(s, this.mParams_, this.mList_.UndefinedInterface);
		}

		void Preload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			this.mIsPreloaded_ = false;

			this.Serialize(s);

			this.mIsPreloaded_ = true;
		}
		public void StreamPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			this.Preload(s);
		}
		public void StreamUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			this.mIsUpdating_ = true;
			this.mCountBeforeUpdate_ = this.mList_.Count;

			if (this.RequiresDataNamePreloading)
				this.Preload(s);
			this.Serialize(s);

			this.mIsUpdating_ = false;
			//mCountBeforeUpdate = 0;
		}
		#endregion
	};
}