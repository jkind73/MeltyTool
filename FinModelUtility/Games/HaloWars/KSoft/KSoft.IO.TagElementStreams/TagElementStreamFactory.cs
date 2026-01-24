using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public static class TagElementStreamFactory
	{
		#region Registration APIs
		/// <summary>Get the file extension for a given format, or null if it isn't supported</summary>
		/// <param name="format">Format to query the extension for. Supports type flags in value</param>
		/// <returns>The file extension (with initial dot) for that given format. Or null if it isn't support (eg, requested binary, but only supports text)</returns>
		public delegate string GetExtensionDelegate(TagElementStreamFormat format);
		public delegate dynamic OpenFromStreamDelegate(TagElementStreamFormat format, Stream sourceStream,
			FileAccess permissions, object owner = null);

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class RegisteredFormat
		{
			public string Name { get; private set; }
			public TagElementStreamFormat BaseFormat { get; private set; }

			internal GetExtensionDelegate GetExtension { get; private set; }
			internal OpenFromStreamDelegate Open { get; private set; }

			internal RegisteredFormat(string name, TagElementStreamFormat baseFormat)
			{
				this.Name = name;
				this.BaseFormat = baseFormat;

				this.GetExtension = null;
				this.Open = null;
			}

			public RegisteredFormat RegisterExtension(GetExtensionDelegate handler)
			{
				Contract.Requires(handler != null);

				this.GetExtension = handler;

				#region Register Text
				var extensionFormat = this.BaseFormat;
				string extension = this.GetExtension(extensionFormat);
				if (extension != null)
					gRegisteredFileExtensions_.Add(extension, extensionFormat);
				#endregion
				#region Register Binary
				extensionFormat |= TagElementStreamFormat.BINARY;
				// #TODO: not all binary formats are implemented yet, and will throw an exception
				try { extension = this.GetExtension(extensionFormat); }
				catch (NotImplementedException) { extension = null; }

				if (extension != null)
					gRegisteredFileExtensions_.Add(extension, extensionFormat);
				#endregion

				return this;
			}
			public RegisteredFormat RegisterOpen(OpenFromStreamDelegate handler)
			{
				Contract.Requires(handler != null);

				this.Open = handler;

				return this;
			}
		};

		static Dictionary<TagElementStreamFormat, RegisteredFormat> gRegisteredFormats_;
		static Dictionary<string, TagElementStreamFormat> gRegisteredFileExtensions_;

		public static RegisteredFormat Register(TagElementStreamFormat baseFormat, string name = null)
		{
			Contract.Requires<ArgumentException>(baseFormat != TagElementStreamFormat.UNDEFINED);
			Contract.Requires<ArgumentException>(baseFormat.GetTypeFlags() == 0,
				"Format should exclude any type flags when registering");
			Contract.Requires<ArgumentException>((baseFormat >= TagElementStreamFormat.K_CUSTOM_START || baseFormat <= TagElementStreamFormat.K_CUSTOM_END) || !string.IsNullOrEmpty(name),
				"Custom formats require an explicit name");

			if (string.IsNullOrEmpty(name))
				name = baseFormat.ToString();

			var registration = new RegisteredFormat(name, baseFormat);
			gRegisteredFormats_.Add(baseFormat, registration);

			return registration;
		}

		static RegisteredFormat GetRegistration(TagElementStreamFormat format, string operation)
		{
			Contract.Requires(!string.IsNullOrEmpty(operation));

			var baseFormat = format.GetBaseFormat();

			if (!gRegisteredFormats_.TryGetValue(baseFormat, out RegisteredFormat registration))
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Format {0} ({1}) is not registered, can't {2}",
					baseFormat, format, operation));

			return registration;
		}
		#endregion

		#region Xml
		static string XmlGetExtension(TagElementStreamFormat format)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.XML);

			if (format.IsText())
				return ".xml";
			else if (format.IsBinary()) // haven't decided on a standard to use yet
				throw new NotImplementedException("General binary XML files not yet implemented");

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic XmlOpenFromStream(TagElementStreamFormat format, Stream sourceStream,
			FileAccess permissions, object owner)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.XML);

			if (format.IsText())
			{
				var stream = new XmlElementStream(sourceStream, permissions, owner);
				stream.InitializeAtRootElement();

				return stream;
			}
			else if (format.IsBinary()) // haven't decided on a standard to use yet
				throw new NotImplementedException("General binary XML files not yet implemented");

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		#region Json
		static string JsonGetExtension(TagElementStreamFormat format)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.JSON);

			if (format.IsText())
				return ".json";
			else if (format.IsBinary())
				return ".bson";

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic JsonOpenFromStream(TagElementStreamFormat format, Stream sourceStream,
			FileAccess permissions, object owner)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.JSON);

			if (format.IsText())
				throw new NotImplementedException();
			else if (format.IsBinary())
				throw new NotImplementedException();

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		#region Yaml
		static string YamlGetExtension(TagElementStreamFormat format)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.YAML);

			if (format.IsText())
				return ".yaml";
			else if (format.IsBinary()) // Yaml doesn't support binary formats
				return null;

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic YamlOpenFromStream(TagElementStreamFormat format, Stream sourceStream,
			FileAccess permissions, object owner)
		{
			Contract.Requires(format.GetBaseFormat() == TagElementStreamFormat.YAML);

			if (format.IsText())
				throw new NotImplementedException();
			else if (format.IsBinary())
				throw new NotSupportedException("Yaml doesn't support binary streams");

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static TagElementStreamFactory()
		{
			gRegisteredFormats_ = new Dictionary<TagElementStreamFormat, RegisteredFormat>();
			gRegisteredFileExtensions_ = new Dictionary<string, TagElementStreamFormat>();

			Register(TagElementStreamFormat.XML)
				.RegisterExtension	(XmlGetExtension)
				.RegisterOpen		(XmlOpenFromStream);

			Register(TagElementStreamFormat.JSON)
				.RegisterExtension	(JsonGetExtension)
				.RegisterOpen		(JsonOpenFromStream);

			Register(TagElementStreamFormat.YAML)
				.RegisterExtension	(YamlGetExtension)
				.RegisterOpen		(YamlOpenFromStream);
		}

		public static dynamic Open(Stream sourceStream, TagElementStreamFormat format,
			FileAccess permissions = FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(sourceStream != null);
			Contract.Requires<ArgumentException>(sourceStream.HasPermissions(permissions));

			var registration = GetRegistration(format, "open");

			return registration.Open(format, sourceStream, permissions, owner);
		}

		public static dynamic Open(string filename,
			FileAccess permissions = FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(filename));

			string extension = Path.GetExtension(filename);
			if (string.IsNullOrEmpty(extension))
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"'{0}' doesn't have a valid file extension",
					filename));

			if (!gRegisteredFileExtensions_.TryGetValue(extension, out TagElementStreamFormat format))
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"No TagElementStream is registered to handle '{0}' files",
					extension));

			// NOTE: could just use File.OpenRead instead. File isn't actually ever written to in this context
			using (var fs = File.Open(filename, FileMode.Open, permissions))
			{
				var stream = Open(fs, format, permissions, owner);

				return stream;
			}
		}
	};
}
