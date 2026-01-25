using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Expr = System.Linq.Expressions.Expression;

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Don't use me unless you're <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>. I am a util class</summary>
	public abstract class EnumBinaryStreamerBase
	{
		protected static readonly Type KBinaryReaderType;
		protected static readonly Type KBinaryWriterType;

		#region Stream Methods
		// I could have made this readonly as well, but then I would have to move the init code from InitializeMethodDictionaries to the cctor
		static Dictionary<TypeCode, MethodInfo> kReadMethods_, kWriteMethods_;

		/// <summary>Initialize <see cref="kReadMethods_"/> with the read methods for the supported underlying enum types <see cref="EnumUtils.KSupportedTypeCodes"/></summary>
		static void InitializeReadMethods()
		{
			var methods = KBinaryReaderType.GetMethods();
			foreach (TypeCode c in EnumUtils.KSupportedTypeCodes)
			{
				var mi = KBinaryReaderType.GetMethod("Read" + c.ToString());
				kReadMethods_.Add(c, mi);
			}
		}
		/// <summary>Initialize <see cref="kWriteMethods_"/> with the read methods for the supported underlying enum types <see cref="EnumUtils.KSupportedTypeCodes"/></summary>
		static void InitializeWriteMethods()
		{
			var methods = KBinaryWriterType.GetMethods();
			// Avoid having to allocate a new array every iteration
			Type[] types = [null];
			foreach (Type t in EnumUtils.KSupportedTypes)
			{
				types[0] = t;

				// GetMethod doesn't have a params overload :(
				var mi = KBinaryWriterType.GetMethod("Write", types);
				kWriteMethods_.Add(Type.GetTypeCode(t), mi);
			}
		}

		static void InitializeMethodDictionaries()
		{
			int capacity = EnumUtils.KSupportedTypeCodes.Length;

			// #NOTE: The EnumComparer<TypeCode> may not be needed for .NET 4 environments:
			// http://www.codeproject.com/Messages/3968802/Re-What-about-Net-4-0.aspx
			kReadMethods_ = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);
			kWriteMethods_ = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);

			InitializeReadMethods();
			InitializeWriteMethods();
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBinaryStreamerBase()
		{
			KBinaryReaderType = typeof(BinaryReader);
			KBinaryWriterType = typeof(BinaryWriter);

			InitializeMethodDictionaries();
		}

		/// <summary>Utility for instant look-up of a type's read/write methods</summary>
		/// <typeparam name="TStreamType">Integer-type</typeparam>
		/// <remarks>
		/// Why did I make a static generic class just for this? It feels clean and
		/// http://stackoverflow.com/questions/686630/static-generic-class-as-dictionary/686689#686689
		/// </remarks>
		internal protected static class StreamType<TStreamType>
			where TStreamType : struct
		{
			/// <summary><typeparamref name="TStreamType"/>'s Read method in <see cref="BinaryReader"/></summary>
			public static readonly MethodInfo KRead;
			/// <summary><typeparamref name="TStreamType"/>'s Write method in <see cref="BinaryWriter"/></summary>
			public static readonly MethodInfo KWrite;

			[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static StreamType()
			{
				TypeCode c = Type.GetTypeCode(typeof(TStreamType));

				KRead = kReadMethods_[c];
				KWrite = kWriteMethods_[c];
			}
		};
	};

	#region IEnumBinaryStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	[Contracts.ContractClass(typeof(EnumBinaryStreamerContract<>))]
	public interface IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		TEnum Read(BinaryReader s);
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		void Read(BinaryReader s, out TEnum value);

		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		void Write(BinaryWriter s, TEnum value);
	};
	[Contracts.ContractClassFor(typeof(IEnumBinaryStreamer<>))]
	abstract class EnumBinaryStreamerContract<TEnum> : IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		public TEnum Read(BinaryReader s)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
		public void Read(BinaryReader s, out TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
		public void Write(BinaryWriter s, TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
	};
	#endregion

	#region IEnumEndianStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	[Contracts.ContractClass(typeof(EnumEndianStreamerContract<>))]
	public interface IEnumEndianStreamer<TEnum> : IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		void Stream(EndianStream s, ref TEnum value);
	};
	[Contracts.ContractClassFor(typeof(IEnumEndianStreamer<>))]
	abstract class EnumEndianStreamerContract<TEnum> : IEnumEndianStreamer<TEnum>
		where TEnum : struct
	{
		public abstract TEnum Read(BinaryReader s);
		public abstract void Read(BinaryReader s, out TEnum value);
		public abstract void Write(BinaryWriter s, TEnum value);

		public void Stream(EndianStream s, ref TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
	};
	#endregion

	public static class EnumBinaryStreamer
	{
		#region IEnumBinaryStreamer
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum, TStreamType>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion

		#region IEnumEndianStreamer
		public static IEnumEndianStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumEndianStreamer<TEnum> For<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion
	};

	/// <summary>Utility for auto-generating methods for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	public class EnumBinaryStreamer<TEnum, TStreamType> : EnumBinaryStreamerBase, IEnumEndianStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct
	{
		class MethodGenerationArgs
		{
			/// <summary>Integer-type to stream the enum value as</summary>
			public readonly Type streamType;
			/// <summary>Enum type to stream</summary>
			public readonly Type enumType;
			/// <summary><see cref="enumType"/>'s integer type used to represent its raw value</summary>
			public readonly Type underlyingType;
			/// <summary>True when <see cref="underlyingType"/> != <see cref="streamType"/></summary>
			public readonly bool underlyingTypeNeedsConversion;
			public readonly bool useUnderlyingType;

			void AssertStreamTypeIsValid()
			{
				var tc = Type.GetTypeCode(this.streamType);

				if (!EnumUtils.TypeIsSupported(tc))
				{
					var message = string.Format(Util.InvariantCultureInfo, "{0} is an invalid stream type", this.streamType);

					throw new NotSupportedException(message);
				}
			}

			public MethodGenerationArgs()
			{
				this.enumType = typeof(TEnum);
				this.streamType = typeof(TStreamType);
				this.underlyingType = Enum.GetUnderlyingType(this.enumType);

				// Check if the user wants us to always use the underlying type
				this.useUnderlyingType = this.streamType == typeof(EnumBinaryStreamerUseUnderlyingType);
				if (this.useUnderlyingType)
					this.streamType = this.underlyingType;

				EnumUtils.AssertTypeIsEnum(this.enumType);
				EnumUtils.AssertUnderlyingTypeIsSupported(this.enumType, this.underlyingType);
				this.AssertStreamTypeIsValid();

				this.underlyingTypeNeedsConversion = this.underlyingType != this.streamType;
			}
		};

		/// <summary>Auto-generated method for reading enum values</summary>
		static readonly ReadDelegate KRead;
		/// <summary>Auto-generated method for writing enum values</summary>
		static readonly Action<BinaryWriter, TEnum> KWrite;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumEndianStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBinaryStreamer{TEnum}"/> class by generating the IO methods.</summary>
		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBinaryStreamer()
		{
			var generationArgs = new MethodGenerationArgs();
			MethodInfo readMethodInfo, writeMethodInfo;
			#region Get read/write method info
			if (generationArgs.useUnderlyingType)
			{
				// Since we use a type-parameter hack to imply we want to use the underlying type
				// for the TStreamType, we have to use reflection to instantiate StreamType<>
				// using kUnderlyingType, which kStreamType is set to up above
				var streamTypeGenClass = typeof(StreamType<>);
				var streamTypeClass = streamTypeGenClass.MakeGenericType(generationArgs.streamType);
				readMethodInfo = streamTypeClass.GetField("kRead").GetValue(null) as MethodInfo;
				writeMethodInfo = streamTypeClass.GetField("kWrite").GetValue(null) as MethodInfo;
			}
			else
			{
				// If we don't use the type-parameter hack and instead are explicitly given the
				// integer type, we can safely instantiate StreamType<> without reflection
				readMethodInfo = StreamType<TStreamType>.KRead;
				writeMethodInfo = StreamType<TStreamType>.KWrite;
			}
			#endregion

			KRead = GenerateReadMethod(generationArgs, readMethodInfo);
			KWrite = GenerateWriteMethod(generationArgs, writeMethodInfo);

			Instance = new EnumBinaryStreamer<TEnum, TStreamType>();
		}

		#region Method generators
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		public delegate void ReadDelegate(BinaryReader s, out TEnum v);

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Read(BinaryReader s, out TEnum v)
		/// {
		///     v = (UnderlyingType)s.Read[TStreamType]();
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="readMethodInfo"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static ReadDelegate GenerateReadMethod(MethodGenerationArgs args, MethodInfo readMethodInfo)
		{
			// Get a "ref type" of the enum we're dealing with so we can define the enum value as an 'out' parameter
			var enumRef = args.enumType.MakeByRefType();

			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var paramS =		Expr.Parameter(KBinaryReaderType, "s");					// BinaryReader s
			var paramV =		Expr.Parameter(enumRef, "v");							// ref TEnum v

			//////////////////////////////////////////////////////////////////////////
			// Define the Read call
			var callRead =		Expr.Call(paramS, readMethodInfo);						// i.e., 's.Read<Type>()'
			var readResult =	args.underlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're reading,
									Expr.Convert(callRead, args.underlyingType) :		// we need to cast the Read result from TStreamType to UnderlyingType
									(Expr)callRead;

			//////////////////////////////////////////////////////////////////////////
			// Define the member assignment
			var paramVMember =Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);	// i.e., 'v.value__'
			// i.e., 'v.value__ = s.Read<Type>()' or 'v.value__ = (UnderlyingType)s.Read<Type>()'
			var assign =		Expr.Assign(paramVMember, readResult);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda =		Expr.Lambda<ReadDelegate>(assign, paramS, paramV);
			return lambda.Compile();
		}

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Write(BinaryWriter s, TEnum v)
		/// {
		///     s.Write((TStreamType)v);
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="writeMethodInfo"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static Action<BinaryWriter, TEnum> GenerateWriteMethod(MethodGenerationArgs args, MethodInfo writeMethodInfo)
		{
			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var paramS =		Expr.Parameter(KBinaryWriterType, "s");					// BinaryWriter s
			var paramV =		Expr.Parameter(args.enumType, "v");						// TEnum v

			//////////////////////////////////////////////////////////////////////////
			// Define the member access
			var paramVMember =Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);	// i.e., 'v.value__'
			var writeParam =	args.underlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're writing,
									Expr.Convert(paramVMember, args.streamType) :		// we need to cast the Write param from UnderlyingType to TStreamType
									(Expr)paramVMember;

			//////////////////////////////////////////////////////////////////////////
			// Define the Write call
			// i.e., 's.Write(v.value__)' or 's.Write((TStreamType)v.value__)'
			var callWrite =	Expr.Call(paramS, writeMethodInfo, writeParam);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda = Expr.Lambda<Action<BinaryWriter, TEnum>>(callWrite, paramS, paramV);
			return lambda.Compile();
		}
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(BinaryReader s)
		{
			KRead(s, out TEnum value);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		public static void Read(BinaryReader s, out TEnum value)	{ KRead(s, out value); }
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		public static void Write(BinaryWriter s, TEnum value)		{ KWrite(s, value); }

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.EndianStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		public static void Stream(EndianStream s, ref TEnum value)
		{
				 if (s.IsReading) Read(s.Reader, out value);
			else if (s.IsWriting) Write(s.Writer, value);
		}
		#endregion

		#region IEnumEndianStreamer<TEnum> Members
		TEnum IEnumBinaryStreamer<TEnum>.Read(BinaryReader s)						{ return Read(s); }
		void IEnumBinaryStreamer<TEnum>.Read(BinaryReader s, out TEnum value)		{ Read(s, out value); }
		void IEnumBinaryStreamer<TEnum>.Write(BinaryWriter s, TEnum value)			{ Write(s, value); }
		void IEnumEndianStreamer<TEnum>.Stream(EndianStream s, ref TEnum value)	{ Stream(s, ref value); }
		#endregion
	};

	public struct EnumBinaryStreamerUseUnderlyingType;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBinaryStreamer<TEnum> : EnumBinaryStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, IComparable, IFormattable, IConvertible;
}
