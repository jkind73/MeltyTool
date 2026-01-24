using System;
using System.IO;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Xml
{
	// based on: http://g-m-a-c.blogspot.com/2013/11/determine-exact-position-of-xmlreader.html

	static class XmlReaderStreamOffsetCalculator
	{
		#region StreamReader util
		const string K_STREAM_READER_BUFFER_LENGTH_PROP_NAME_ = "ByteLen_Prop";
		const string K_STREAM_READER_BUFFER_POSITION_PROP_NAME_ = "CharPos_Prop";
		const string K_STREAM_READER_DEFAULT_BUFFER_SIZE_FIELD_NAME_ = "DefaultBufferSize";

		static readonly Func<StreamReader, int> KStreamReaderBufferLengthGet =
			Reflection.Util.GenerateMemberGetter<StreamReader, int>(K_STREAM_READER_BUFFER_LENGTH_PROP_NAME_);
		static readonly Func<StreamReader, int> KStreamReaderBufferPositionGet =
			Reflection.Util.GenerateMemberGetter<StreamReader, int>(K_STREAM_READER_BUFFER_POSITION_PROP_NAME_);

		static readonly int KStreamReaderDefaultBufferSize =
			Reflection.Util.GenerateStaticFieldGetter<StreamReader, int>(K_STREAM_READER_DEFAULT_BUFFER_SIZE_FIELD_NAME_)();

		static int GetBufferLength(StreamReader s)
		{
			return KStreamReaderBufferLengthGet(s);
		}
		static int GetBufferPosition(StreamReader s)
		{
			return KStreamReaderBufferPositionGet(s);
		}
		static int GetPreambleLength(StreamReader s)
		{
			return s.CurrentEncoding.GetPreamble().Length;
		}
		#endregion

		#region XmlTextReaderImpl util
		const string K_TEXT_READER_IMPL_BUFFER_LENGTH_PROP_NAME_ = "DtdParserProxy_ParsingBufferLength";
		const string K_TEXT_READER_IMPL_BUFFER_POSITION_PROP_NAME_ = "DtdParserProxy_CurrentPosition";

		static readonly Func<XmlReader, int> KTextReaderImplBufferLengthGet =
			Reflection.Util.GenerateMemberGetter<XmlReader, int>(K_TEXT_READER_IMPL_BUFFER_LENGTH_PROP_NAME_);
		static readonly Func<XmlReader, int> KTextReaderImplBufferPositionGet =
			Reflection.Util.GenerateMemberGetter<XmlReader, int>(K_TEXT_READER_IMPL_BUFFER_POSITION_PROP_NAME_);

		static int GetBufferLength(XmlReader s)
		{
			return KTextReaderImplBufferLengthGet(s);
		}
		static int GetBufferPosition(XmlReader s)
		{
			return KTextReaderImplBufferPositionGet(s);
		}
		#endregion

		public static long GetPosition(this XmlReader xmlReader, StreamReader underlyingStreamReader)
		{
			Contract.Requires<ArgumentNullException>(xmlReader != null);
			Contract.Requires<ArgumentNullException>(underlyingStreamReader != null);
			Contract.Requires<InvalidOperationException>(xmlReader.GetType().Name == "XmlTextReaderImpl");

			// get the 'base' position from the root stream
			long streamPosition = underlyingStreamReader.BaseStream.Position;

			// get the underlying stream's buffer state and text encoding preamble
			var streamBufferLength = GetBufferLength(underlyingStreamReader);
			var streamBufferPos = GetBufferPosition(underlyingStreamReader);
			var streamPreambleLength = GetPreambleLength(underlyingStreamReader);

			// get the xml reader's buffer state
			var xmlBufferLength = GetBufferLength(xmlReader);
			var xmlBufferPos = GetBufferPosition(xmlReader);

			// subtract the lengths of the buffers which the stream/xml readers cached
			// then add the 'cursor' positions the readers have in those buffers
			// plus the text encoding preamble length
			long pos = streamPosition
				- (streamBufferLength == KStreamReaderDefaultBufferSize ? KStreamReaderDefaultBufferSize : 0)
				- xmlBufferLength
				+ xmlBufferPos + streamBufferPos + streamPreambleLength;

			return pos;
		}
	};
}