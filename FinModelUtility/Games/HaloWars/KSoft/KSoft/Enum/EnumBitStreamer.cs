using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Expr = System.Linq.Expressions.Expression;

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <typeparam name="TOptions">TBD</typeparam>
	public class EnumBitStreamer<TEnum, TStreamType, TOptions> : EnumBitStreamerBase, IEnumBitStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct
		where TOptions : EnumBitStreamerOptions, new()
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
			public readonly bool streamTypeIsSigned;

			public TOptions options;

			void AssertStreamTypeIsValid(out bool isSigned)
			{
				var tc = Type.GetTypeCode(this.streamType);
				isSigned = tc.IsSigned();

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
				this.AssertStreamTypeIsValid(out this.streamTypeIsSigned);

				this.underlyingTypeNeedsConversion = this.underlyingType != this.streamType;

				this.options = new TOptions();

				if (this.options.UseNoneSentinelEncoding)
				{
					if (this.streamType == typeof(sbyte) || this.streamType == typeof(byte))
						throw new ArgumentException(
							"{0}: UseNoneSentinelEncoding can't operate on (s)byte types (StreamType)",
							this.enumType.FullName);
				}
				#region Options.BitSwap
				if (this.options.BitSwap)
				{
					if (this.streamTypeIsSigned)
						throw new ArgumentException(
							"{0}: Bit-swapping only makes sense on flags/unsigned types, but StreamType is signed",
							this.enumType.FullName);
				}
				else
				{
					if (this.options.BitSwapGuardAgainstOneBit)
						Debug.Trace.Io.TraceInformation("{0}'s {1} says we should guard against one bit cases, but not bitswap",
						                                this.enumType.FullName, typeof(TOptions).FullName);
				}
				#endregion
			}
		};

		/// <summary>Auto-generated method for reading enum values</summary>
		static readonly ReadDelegate KRead;
		/// <summary>Auto-generated method for writing enum values</summary>
		static readonly Action<BitStream, TEnum, int> KWrite;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumBitStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBitStreamer{TEnum}"/> class by generating the IO methods.</summary>
		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBitStreamer()
		{
			var generationArgs = new MethodGenerationArgs();
			MethodInfo readMethodInfo, writeMethodInfo, swapMethod;
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
				swapMethod = streamTypeClass.GetField("kBitSwap").GetValue(null) as MethodInfo;
			}
			else
			{
				// If we don't use the type-parameter hack and instead are explicitly given the
				// integer type, we can safely instantiate StreamType<> without reflection
				readMethodInfo = StreamType<TStreamType>.KRead;
				writeMethodInfo = StreamType<TStreamType>.KWrite;
				swapMethod = StreamType<TStreamType>.KBitSwap;
			}
			#endregion

			KRead = GenerateReadMethod(generationArgs, readMethodInfo, swapMethod);
			KWrite = GenerateWriteMethod(generationArgs, writeMethodInfo, swapMethod);

			Instance = new EnumBitStreamer<TEnum, TStreamType, TOptions>();
		}

		#region Method generators
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public delegate void ReadDelegate(BitStream s, out TEnum v, int bitCount);

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Read(IO.BitStream s, out TEnum v, int bitCount)
		/// {
		///     v = (UnderlyingType)s.Read[TStreamType](bitCount);
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="readMethodInfo"></param>
		/// <param name="bitSwapMethod"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static ReadDelegate GenerateReadMethod(MethodGenerationArgs args, MethodInfo readMethodInfo, MethodInfo bitSwapMethod)
		{
			// Get a "ref type" of the enum we're dealing with so we can define the enum value as an 'out' parameter
			var enumRef = args.enumType.MakeByRefType();

			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var paramS =		Expr.Parameter(KBitStreamType, "s");					// BitStream s
			var paramV =		Expr.Parameter(enumRef, "v");							// ref TEnum v
			var paramBc =		Expr.Parameter(typeof(int), "bitCount");				// int bitCount

			//////////////////////////////////////////////////////////////////////////
			// Define the Read call
			Expr callRead;
			if (args.streamTypeIsSigned)
				callRead =		Expr.Call(paramS, readMethodInfo, paramBc, Expr.Constant(args.options.SignExtend));
			else
				callRead =		Expr.Call(paramS, readMethodInfo, paramBc);			// i.e., 's.Read<Type>(bitCount)'

			if (args.options.UseNoneSentinelEncoding)
				callRead = Expr.Decrement(callRead);

			#region options.BitSwap
			if (args.options.BitSwap)
			{
				// i.e., Bits.BitSwap( Read(), bitCount-1 );
				var startBitIndex = Expr.Decrement(paramBc);
				Expr swapCall = Expr.Call(null, bitSwapMethod, callRead, startBitIndex);

				// i.e., bitCount-1 ? Bits.BitSwap( Read(), bitCount-1 ) : Read() ;
				if (args.options.BitSwapGuardAgainstOneBit)
				{
					var startBitIndexIsNotZero = Expr.NotEqual(startBitIndex, Expr.Constant(0, typeof(int)));
					swapCall = Expr.Condition(startBitIndexIsNotZero,
						swapCall, callRead);
				}

				callRead = swapCall;
			}
			#endregion

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
			var lambda =		Expr.Lambda<ReadDelegate>(assign, paramS, paramV, paramBc);
			return lambda.Compile();
		}

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Write(IO.BitStream s, TEnum v, int bitCount)
		/// {
		///     s.Write((TStreamType)v, bitCount);
		/// }
		/// </code>
		/// </summary>
		/// <returns>The generated method.</returns>
		/// <param name="args"></param>
		/// <param name="writeMethodInfo"></param>
		/// <param name="bitSwapMethod"></param>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static Action<BitStream, TEnum, int> GenerateWriteMethod(MethodGenerationArgs args, MethodInfo writeMethodInfo, MethodInfo bitSwapMethod)
		{
			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var paramS =		Expr.Parameter(KBitStreamType, "s");					// BitStream s
			var paramV =		Expr.Parameter(args.enumType, "v");							// TEnum v
			var paramBc =		Expr.Parameter(typeof(int), "bitCount");				// int bitCount

			//////////////////////////////////////////////////////////////////////////
			// Define the member access
			var paramVMember =Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);	// i.e., 'v.value__'
			var writeParam =	args.underlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're writing,
									Expr.Convert(paramVMember, args.streamType) :		// we need to cast the Write param from UnderlyingType to TStreamType
									(Expr)paramVMember;

			if (args.options.UseNoneSentinelEncoding)
				writeParam = Expr.Increment(writeParam);

			#region options.BitSwap
			if (args.options.BitSwap)
			{
				// i.e., Bits.BitSwap( value, bitCount-1 );
				var startBitIndex = Expr.Decrement(paramBc);
				Expr swapCall = Expr.Call(null, bitSwapMethod, writeParam, startBitIndex);

				// i.e., bitCount-1 ? Bits.BitSwap( value, bitCount-1 ) : value ;
				if (args.options.BitSwapGuardAgainstOneBit)
				{
					var startBitIndexIsNotZero = Expr.NotEqual(startBitIndex, Expr.Constant(0, typeof(int)));
					swapCall = Expr.Condition(startBitIndexIsNotZero,
						swapCall, writeParam);
				}

				writeParam = swapCall;
			}
			#endregion

			//////////////////////////////////////////////////////////////////////////
			// Define the Write call
			// i.e., 's.Write(v.value__, bitCount)' or 's.Write((TStreamType)v.value__, bitCount)'
			var callWrite =	Expr.Call(paramS, writeMethodInfo, writeParam, paramBc);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda = Expr.Lambda<Action<BitStream, TEnum, int>>(callWrite, paramS, paramV, paramBc);
			return lambda.Compile();
		}
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="bitCount"></param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(BitStream s, int bitCount)
		{
			KRead(s, out TEnum value, bitCount);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public static void Read(BitStream s, out TEnum value, int bitCount)	{ KRead(s, out value, bitCount); }
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void Write(BitStream s, TEnum value, int bitCount)		{ KWrite(s, value, bitCount); }

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.BitStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="bitCount"></param>
		public static void Stream(BitStream s, ref TEnum value, int bitCount)
		{
				 if (s.IsReading) Read(s, out value, bitCount);
			else if (s.IsWriting) Write(s, value, bitCount);
		}
		#endregion

		#region IEnumBitStreamer<TEnum> Members
		TEnum IEnumBitStreamer<TEnum>.Read(BitStream s, int bitCount)					{ return Read(s, bitCount); }
		void IEnumBitStreamer<TEnum>.Read(BitStream s, out TEnum value, int bitCount)	{ Read(s, out value, bitCount); }
		void IEnumBitStreamer<TEnum>.Write(BitStream s, TEnum value, int bitCount)		{ Write(s, value, bitCount); }
		void IEnumBitStreamer<TEnum>.Stream(BitStream s, ref TEnum value, int bitCount)	{ Stream(s, ref value, bitCount); }
		#endregion
	};

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <remarks>Uses the default options in <see cref="EnumBitStreamerOptions"/></remarks>
	public class EnumBitStreamer<TEnum, TStreamType> : EnumBitStreamer<TEnum, TStreamType, EnumBitStreamerOptions>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBitStreamer<TEnum> : EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, IComparable, IFormattable, IConvertible;

	public sealed class EnumBitStreamerWithOptions<TEnum, TOptions> : EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType, TOptions>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TOptions : EnumBitStreamerOptions, new();
}
