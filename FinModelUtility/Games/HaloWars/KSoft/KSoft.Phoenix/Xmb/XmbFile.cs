using System;
using System.Collections.Generic;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	/*public */sealed class XmbFileContext
	{
		public Shell.ProcessorSize pointerSize;
	};

	/*public*/ sealed partial class XmbFile
		: IO.IEndianStreamable
		, IDisposable
	{
		public const string K_FILE_EXT = ".xmb";
		const uint K_SIGNATURE_ = 0x71439800;

		List<Element> mElements_;
		XmbVariantMemoryPool mPool_;
		bool mHasUnicodeStrings_;

		public bool HasUnicodeStrings { get { return this.mHasUnicodeStrings_; } }

	#region IDisposable Members
	public void Dispose()
		{
			if (this.mElements_ != null)
			{
				this.mElements_.Clear();
				this.mElements_ = null;
			}

			Util.DisposeAndNull(ref this.mPool_);
		}
		#endregion

		#region IEndianStreamable Members
		public void Read(IO.EndianReader s)
		{
			var context = s.UserData as XmbFileContext;

			using (s.ReadSignatureWithByteSwapSupport(K_SIGNATURE_))
			{
				if (context.pointerSize == Shell.ProcessorSize.X64)
				{
					// #HACK to deal with xmb files which weren't updated with new tools
					if (s.ByteOrder == Shell.EndianFormat.BIG)
					{
						context.pointerSize = Shell.ProcessorSize.X32;
					}
				}

				s.VirtualAddressTranslationInitialize(context.pointerSize);

				Values.PtrHandle elementsOffsetPos;

				if (context.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad32();
				}
				#region Initialize elements
				{
					int count = s.ReadInt32();
					if (context.pointerSize == Shell.ProcessorSize.X64)
					{
						s.Pad32();
					}
					s.ReadVirtualAddress(out elementsOffsetPos);

					this.mElements_ = new List<Element>(count);
				}
				#endregion
				#region Initialize and read pool
				{
					int size = s.ReadInt32();
					if (context.pointerSize == Shell.ProcessorSize.X64)
					{
						s.Pad32();
					}
					Values.PtrHandle poolOffsetPos = s.ReadVirtualAddress();

					s.Seek((long)poolOffsetPos);
					byte[] buffer = s.ReadBytes(size);

					this.mPool_ = new XmbVariantMemoryPool(buffer, s.ByteOrder);
				}
				#endregion

				if (context.pointerSize == Shell.ProcessorSize.X64)
				{
					s.Pad64();
				}

				s.Seek((long)elementsOffsetPos);
				for (int x = 0; x < this.mElements_.Capacity; x++)
				{
					var e = new Element();
					this.mElements_.Add(e);

					e.index = x;
					e.Read(this, context, s);
				}

				foreach (var e in this.mElements_)
				{
					e.ReadAttributes(this, s);
					e.ReadChildren(s);
				}
			}
		}

		public void Write(IO.EndianWriter s)
		{
			var context = s.UserData as XmbFileContext;

			s.Write(K_SIGNATURE_);
			if (context.pointerSize == Shell.ProcessorSize.X64)
			{
				s.Pad32();
			}

			#region Elements header
			s.Write(this.mElements_.Count);
			if (context.pointerSize == Shell.ProcessorSize.X64)
			{
				s.Pad32();
			}
			var elementsOffsetPos = s.MarkVirtualAddress(context.pointerSize);
			#endregion

			#region Pool header
			s.Write(this.mPool_.Size);
			if (context.pointerSize == Shell.ProcessorSize.X64)
			{
				s.Pad32();
			}
			var poolOffsetPos = s.MarkVirtualAddress(context.pointerSize);
			#endregion

			if (context.pointerSize == Shell.ProcessorSize.X64)
			{
				s.Pad64();
			}

			var elementsOffset = s.PositionPtr;
			foreach (var e in this.mElements_)
			{
				e.Write(s);
			}
			foreach (var e in this.mElements_)
			{
				e.WriteAttributes(s);
				e.WriteChildren(s);
			}

			var poolOffset = s.PositionPtr;
			this.mPool_.Write(s);

			s.Seek((long)elementsOffsetPos);
			s.WriteVirtualAddress(elementsOffset);
			s.Seek((long)poolOffsetPos);
			s.WriteVirtualAddress(poolOffset);
		}
		#endregion

		string ToString(XmbVariant v) { return v.ToString(this.mPool_); }

		public XmlDocument ToXmlDocument()
		{
			Contract.Ensures(Contract.Result<XmlDocument>() != null);

			var doc = new XmlDocument();

			return this.ToXmlDocument(doc);
		}

		public XmlDocument ToXmlDocument(XmlDocument doc)
		{
			Contract.Ensures(doc == null || Contract.Result<XmlDocument>() != null);

			if (doc != null && this.mElements_ != null && this.mElements_.Count > 1)
			{
				var root = this.mElements_[0];
				var rootE = root.ToXml(this, doc, null);

				doc.AppendChild(rootE);
			}

			return doc;
		}

		#region FromXml
		public void FromXml(XmlElement root)
		{
			var e = new Element();
		}
		#endregion
		#region ToXml
		public void ToXml(string file)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			using (var fs = System.IO.File.Create(file))
			{
				this.ToXml(fs);
			}
		}
		public void ToXml(System.IO.Stream stream)
		{
			Contract.Requires(stream != null);

			var doc = this.ToXmlDocument();

			var encoding = this.mHasUnicodeStrings_
				? System.Text.Encoding.UTF8
				: System.Text.Encoding.ASCII;
			var xmlWriterSettings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				CloseOutput = false,
				Encoding = encoding,
			};
			using (var xml = XmlWriter.Create(stream, xmlWriterSettings))
			{
				doc.Save(xml);
			}
		}
		#endregion
	};
}