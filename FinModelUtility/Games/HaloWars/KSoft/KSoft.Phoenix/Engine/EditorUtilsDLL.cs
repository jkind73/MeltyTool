using System;
using System.Runtime.InteropServices;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public static class EditorUtilsDll
	{
		public enum LibraryMode : uint
		{
			DEBUG,
			RELEASE,
		};

		const uint K_VERSION_ = 3;

		const string K_DLL_NAME_ = @"Engine\EditorUtils.dll";
		const CallingConvention K_CALL_CONV_ = CallingConvention.Cdecl;
		const CharSet K_CHAR_SET_ = CharSet.Ansi;

		public static bool Initialized { get; private set; }

		internal static bool gEntryPointsNotFound;
		public static bool EntryPointsNotFound
		{
			get
			{
				if (!Initialized && !gEntryPointsNotFound && !gIsInitializing_)
					Initialize();

				return gEntryPointsNotFound;
			}
			private set { gEntryPointsNotFound = value; }
		}

		public static void HandleEntryPointNotFound(EntryPointNotFoundException ex)
		{
			if (EntryPointsNotFound)
				return;

			EntryPointsNotFound = true;
			Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
				"Failed to find a EditorUtils method",
				ex);
		}

		private static bool gIsInitializing_;
		public static void Initialize()
		{
			if (Initialized)
				return;

			try
			{
				gIsInitializing_ = true;

				var libPointerSize = GetPointerSize();
				if (libPointerSize != IntPtr.Size)
				{
					Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
						"EditorUtils was built for a platform that doesn't match what we're currently running in",
						libPointerSize,
						IntPtr.Size);
					return;
				}

				var version = GetLibraryVersion();
				if (version != K_VERSION_)
				{
					Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
						"EditorUtils version doesn't match what we expected",
						version,
						K_VERSION_);
					return;
				}

				var libMode = GetLibraryMode();
				Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Information, TypeExtensions.K_NONE,
					"Finished loading EditorUtils",
					libMode,
					version);

				Initialized = true;
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
			}
			finally
			{
				gIsInitializing_ = false;
			}
		}

		public static void Dispose()
		{
			if (!Initialized)
				return;

			Initialized = false;
			EntryPointsNotFound = false;
		}

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		static extern uint GetPointerSize();
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		static extern LibraryMode GetLibraryMode();
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		static extern uint GetLibraryVersion();

		#region Tile/Untile and copy data
		public enum TileCopyFormat : int
		{
			R16_F = 0,
			R8_G8 = 1,
			DXN = 2,
			L8 = 3,
			DXT5_A = 4,
			R11_G11_B10 = 5,
			DXT1 = 6,
			G16_R16 = 7,
		};

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] byte[] dst,
			[In] byte[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] byte[] dst,
			[In] byte[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] short[] dst,
			[In] short[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] short[] dst,
			[In] short[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] uint[] dst,
			[In] uint[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat = TileCopyFormat.R11_G11_B10);
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] uint[] dst,
			[In] uint[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat = TileCopyFormat.R11_G11_B10);

		public static bool TileCopyData(
			Array dstArray,
			Array srcArray,
			int width,
			int height,
			TileCopyFormat dxtFormat)
		{
			Contract.Requires(width > 0);
			Contract.Requires(height > 0);

			if (EntryPointsNotFound)
				return false;

			if (dstArray == null || srcArray == null)
				return false;

			try
			{
				if (dstArray is byte[] && srcArray is byte[])
					return TileCopyData((byte[])dstArray, (byte[])srcArray, width, height, dxtFormat);
				if (dstArray is short[] && srcArray is short[])
					return TileCopyData((short[])dstArray, (short[])srcArray, width, height, dxtFormat);
				if (dstArray is uint[] && srcArray is uint[])
					return TileCopyData((uint[])dstArray, (uint[])srcArray, width, height, dxtFormat);
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
				return false;
			}

			Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
				"EditorUtils.TileCopyData called with a destination and/or source array type that is not supported",
				dstArray.GetType(),
				srcArray.GetType(),
				dxtFormat);
			return false;
		}

		public static bool UntileCopyData(
			Array dstArray,
			Array srcArray,
			int width,
			int height,
			TileCopyFormat dxtFormat)
		{
			Contract.Requires(width > 0);
			Contract.Requires(height > 0);

			if (EntryPointsNotFound)
				return false;

			if (dstArray == null || srcArray == null)
				return false;

			try
			{
				if (dstArray is byte[] && srcArray is byte[])
					return UntileCopyData((byte[])dstArray, (byte[])srcArray, width, height, dxtFormat);
				if (dstArray is short[] && srcArray is short[])
					return UntileCopyData((short[])dstArray, (short[])srcArray, width, height, dxtFormat);
				if (dstArray is uint[] && srcArray is uint[])
					return UntileCopyData((uint[])dstArray, (uint[])srcArray, width, height, dxtFormat);
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
				return false;
			}

			Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
				"EditorUtils.UntileCopyData called with a destination and/or source array type that is not supported",
				dstArray.GetType(),
				srcArray.GetType(),
				dxtFormat);
			return false;
		}
		#endregion
	};
}

