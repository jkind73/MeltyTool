using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.XML
{
	public static partial class XmlUtil
	{
		/// <summary>Pass this to a Stream call when reading something from the Text data (which doesn't have a name), just to be clear of the code's intention</summary>
		public const string K_NO_XML_NAME = null;
		public const IO.TagElementNodeType K_SOURCE_ATTR = IO.TagElementNodeType.ATTRIBUTE;
		public const IO.TagElementNodeType K_SOURCE_ELEMENT = IO.TagElementNodeType.ELEMENT;
		public const IO.TagElementNodeType K_SOURCE_CURSOR = IO.TagElementNodeType.TEXT;

		public static void ReadDetermineListSize<TDoc, TCursor, T>(IO.TagElementStream<TDoc, TCursor, string> s, List<T> list)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(s.IsReading);

			int childElementCount = s.TryGetCursorElementCount();
			if (list.Capacity < childElementCount)
				list.Capacity = childElementCount;
		}

		public static IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, IO.TagElementNodeType xmlSource)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != K_NO_XML_NAME));
			Contract.Requires(xmlSource != IO.TagElementNodeType.ATTRIBUTE);

			return xmlSource == IO.TagElementNodeType.TEXT
				? s.ElementsByName(xmlName)
				: s.Elements;
		}
	};
}

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public static bool StreamBVector<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref BVector vector
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.K_NO_XML_NAME));

			string stringValue = null;
			bool wasStreamed = true;
			const bool toLower = false;

			if (s.IsReading)
			{
				if (isOptional)
					wasStreamed = s.StreamStringOpt(xmlName, ref stringValue, toLower, xmlSource);
				else
					s.StreamString(xmlName, ref stringValue, toLower, xmlSource);

				if (wasStreamed)
				{
					var parseResult = ParseBVectorString(stringValue);
					if (!parseResult.HasValue)
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
							"Failed to parse value (hint: {0}) as vector: {1}",
							xmlSource.RequiresName() ? xmlName : "ElementText",
							stringValue)));

					vector = parseResult.Value;
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(vector))
				{
					wasStreamed = false;
					return wasStreamed;
				}

				stringValue = vector.ToBVectorString();

				if (isOptional)
					s.StreamStringOpt(xmlName, ref stringValue, toLower, xmlSource);
				else
					s.StreamString(xmlName, ref stringValue, toLower, xmlSource);
			}

			return wasStreamed;
		}

		public static bool StreamIntegerColor<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref System.Drawing.Color color
			, byte defaultAlpha = 0xFF
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.K_NO_XML_NAME));

			string stringValue = null;
			bool wasStreamed = true;
			const bool toLower = false;

			if (s.IsReading)
			{
				if (isOptional)
					wasStreamed = s.StreamStringOpt(xmlName, ref stringValue, toLower, xmlSource);
				else
					s.StreamString(xmlName, ref stringValue, toLower, xmlSource);

				if (wasStreamed)
				{
					if (!TokenizeIntegerColor(stringValue, defaultAlpha, ref color))
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
							"Failed to parse value (hint: {0}) as color: {1}",
							xmlSource.RequiresName() ? xmlName : "ElementText",
							stringValue)));
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(color))
				{
					wasStreamed = false;
					return wasStreamed;
				}

				stringValue = color.ToIntegerColorString(defaultAlpha);

				if (isOptional)
					s.StreamStringOpt(xmlName, ref stringValue, toLower, xmlSource);
				else
					s.StreamString(xmlName, ref stringValue, toLower, xmlSource);
			}

			return wasStreamed;
		}

		public static bool StreamProtoEnum<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref int dbid
			, Collections.IProtoEnum protoEnum
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.K_SOURCE_ELEMENT
			, int isOptionalDefaultValue = TypeExtensions.K_NONE)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.K_NO_XML_NAME));
			Contract.Requires(protoEnum != null);

			string idName = null;
			bool wasStreamed = true;
			bool toLower = false;

			if (s.IsReading)
			{
				if (isOptional)
					wasStreamed = s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);

				if (wasStreamed)
				{
					dbid = protoEnum.TryGetMemberId(idName);
					Contract.Assert(dbid.IsNotNone(), idName);
				}
				//else
				//	dbid = isOptionalDefaultValue;
			}
			else if (s.IsWriting)
			{
				if (isOptional && isOptionalDefaultValue.IsNotNone() && isOptionalDefaultValue == dbid)
				{
					wasStreamed = false;
					return wasStreamed;
				}

				idName = protoEnum.TryGetMemberName(dbid);
				if (idName.IsNullOrEmpty())
					Contract.Assert(!idName.IsNullOrEmpty(), dbid.ToString());

				if (isOptional)
					s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}
	};
}