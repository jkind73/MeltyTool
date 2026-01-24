using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.XML
{
	internal abstract class BListXmlSerializerBase<T>
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public abstract BListXmlParams Params { get; }
		public abstract Collections.BListBase<T> List { get; }

		#region IXmlElementStreamable Members
		protected abstract void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
			where TDoc : class
			where TCursor : class;
		protected abstract void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
			where TDoc : class
			where TCursor : class;

		protected virtual void ReadDetermineListSize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			int childElementCount = s.TryGetCursorElementCount();
			if (this.List.Capacity < childElementCount)
				this.List.Capacity = childElementCount;
		}
		protected virtual IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			return this.Params.UseElementName
				? s.ElementsByName(this.Params.elementName)
				: s.Elements;
		}
		protected virtual void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			this.ReadDetermineListSize(s, xs);

			int x = 0;
			foreach (var n in this.ReadGetNodes(s))
			{
				using (s.EnterCursorBookmark(n))
					this.Read(s, xs, x++);
			}

			this.List.OptimizeStorage();
		}
		protected virtual string WriteGetElementName(T data)
		{
			return this.Params.elementName;
		}
		protected virtual void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			foreach (T data in this.List)
				using (s.EnterCursorBookmark(this.WriteGetElementName(data)))
					this.Write(s, xs, data);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool shouldStream = true;
			string rootName = this.Params.GetOptionalRootName();
			var xs = s.GetSerializerInterface();

			if (s.IsReading) // If the stream doesn't have the expected element, don't try to stream
				shouldStream = rootName == null || s.ElementsExists(rootName);
			else if (s.IsWriting)
				shouldStream = this.List != null && this.List.IsEmpty == false;

			if (shouldStream) using (s.EnterCursorBookmark(rootName))
			{
					 if (s.IsReading)
						 this.ReadNodes(s, xs);
				else if (s.IsWriting)
					this.WriteNodes(s, xs);
			}
		}
		#endregion

		#region IDisposable Members
		public virtual void Dispose()
		{
		}
		#endregion
	};
}