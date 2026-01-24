using System.Collections.Generic;
using System.Xml;

namespace KSoft.Phoenix.Xmb
{
	partial class XmbFile
	{
		sealed class Element
		{
			internal int index;
			Values.PtrHandle mAttributesOffsetPos_, mAttributesOffset_;
			Values.PtrHandle mChildrenOffsetPos_, mChildrenOffset_;

			public int rootElementIndex = TypeExtensions.K_NONE;
			public XmbVariant nameVariant;
			public XmbVariant innerTextVariant;
			List<KeyValuePair<XmbVariant, XmbVariant>> attributes_;
			List<int> childrenIndices_;

			#region IEndianStreamable Members
			public void ReadAttributes(XmbFile xmb, IO.EndianReader s)
			{
				if (this.mAttributesOffset_.IsInvalidHandle)
					return;

				s.Seek((long) this.mAttributesOffset_);
				for (int x = 0; x < this.attributes_.Capacity; x++)
				{
					XmbVariant k; XmbVariantSerialization.Read(s, out k);
					XmbVariant v; XmbVariantSerialization.Read(s, out v);

					var kv = new KeyValuePair<XmbVariant, XmbVariant>(k, v);
					this.attributes_.Add(kv);

					if (k.HasUnicodeData || v.HasUnicodeData)
						xmb.mHasUnicodeStrings_ = true;
				}
			}
			public void ReadChildren(IO.EndianReader s)
			{
				if (this.mChildrenOffset_.IsInvalidHandle)
					return;

				s.Seek((long) this.mChildrenOffset_);
				for (int x = 0; x < this.childrenIndices_.Capacity; x++)
					this.childrenIndices_.Add(s.ReadInt32());
			}
			public void Read(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				s.Read(out this.rootElementIndex);
				XmbVariantSerialization.Read(s, out this.nameVariant);
				XmbVariantSerialization.Read(s, out this.innerTextVariant);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}

				#region Attributes header
				int count;
				s.Read(out count);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out this.mAttributesOffset_);
				this.attributes_ = new List<KeyValuePair<XmbVariant, XmbVariant>>(count);
				#endregion

				#region Children header
				s.Read(out count);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out this.mChildrenOffset_);
				this.childrenIndices_ = new List<int>(count);
				#endregion

				if (this.nameVariant.HasUnicodeData || this.innerTextVariant.HasUnicodeData)
					xmb.mHasUnicodeStrings_ = true;
			}

			public void WriteAttributes(IO.EndianWriter s)
			{
				if (this.attributes_.Count == 0)
					return;

				this.mAttributesOffset_ = s.PositionPtr;
				foreach (var kv in this.attributes_)
				{
					XmbVariantSerialization.Write(s, kv.Key);
					XmbVariantSerialization.Write(s, kv.Value);
				}

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long) this.mAttributesOffsetPos_);
				s.WriteVirtualAddress(this.mAttributesOffset_);
				s.Seek(pos);
			}
			public void WriteChildren(IO.EndianWriter s)
			{
				if (this.childrenIndices_.Count == 0)
					return;

				this.mChildrenOffset_ = s.PositionPtr;
				foreach (int ci in this.childrenIndices_)
					s.Write(ci);

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long) this.mChildrenOffsetPos_);
				s.WriteVirtualAddress(this.mChildrenOffset_);
				s.Seek(pos);
			}
			public void Write(IO.EndianWriter s)
			{
				var xmbContext = s.UserData as XmbFileContext;

				s.Write(this.rootElementIndex);
				XmbVariantSerialization.Write(s, this.nameVariant);
				XmbVariantSerialization.Write(s, this.innerTextVariant);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}

				#region Attributes header
				s.Write(this.attributes_.Count);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}

				this.mAttributesOffsetPos_ = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion

				#region Children header
				s.Write(this.childrenIndices_.Count);
				if (xmbContext.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}

				this.mChildrenOffsetPos_ = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion
			}
			#endregion

			#region FromXml
			public void FromXmlProcessChildren(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlProcessAttributes(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlInitialize(XmbFileBuilder builder, int rootIndex, int index, XmlElement e)
			{
				this.index = index;
				this.rootElementIndex = rootIndex;

				if (e.HasAttributes)
					this.attributes_ = new List<KeyValuePair<XmbVariant, XmbVariant>>(e.Attributes.Count);
				if (e.HasChildNodes)
					this.childrenIndices_ = new List<int>(e.ChildNodes.Count);

				string name = e.Name;
				string text = e.Value;

				if (e.HasAttributes)
					this.FromXmlProcessAttributes(builder, e);
				if (e.HasChildNodes)
					this.FromXmlProcessChildren(builder, e);
			}
			#endregion
			#region ToXml
			void InnerTextToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (!this.innerTextVariant.IsEmpty)
				{
					var text = doc.CreateTextNode(xmb.ToString(this.innerTextVariant));
					e.AppendChild(text);
				}
			}
			void AttributesToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (this.attributes_.Count > 0) foreach (var kv in this.attributes_)
				{
					string k = xmb.ToString(kv.Key);
					string v = xmb.ToString(kv.Value);

					var attr = doc.CreateAttribute(k);
					attr.Value = v;

					// #HACK avoids exceptions like:
					// "The prefix '' cannot be redefined from '' to 'http://www.w3.org/2000/09/xmldsig#' within the same start element tag."
					// for XML files that weren't meant for the game but were transformed to XMB anyway
					if (string.CompareOrdinal(k, "xmlns")==0)
					{
						var comment = doc.CreateComment(attr.OuterXml);
						e.AppendChild(comment);
						continue;
					}

					e.Attributes.Append(attr);
				}
			}
			void ChildrenToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (this.childrenIndices_.Count > 0) foreach (int x in this.childrenIndices_)
				{
					var element = xmb.mElements_[x];

					element.ToXml(xmb, doc, e);
				}
			}
			public XmlElement ToXml(XmbFile xmb, XmlDocument doc, XmlElement root)
			{
				var e = doc.CreateElement(xmb.ToString(this.nameVariant));

				if (root != null)
					root.AppendChild(e);

				this.AttributesToXml(xmb, doc, e);
				this.ChildrenToXml(xmb, doc, e);
				this.InnerTextToXml(xmb, doc, e);

				return e;
			}
			#endregion
		};
	};
}