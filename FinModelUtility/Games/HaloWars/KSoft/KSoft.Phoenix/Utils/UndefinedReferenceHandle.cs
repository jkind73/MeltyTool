#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

// The integer type used to represent a handle
using HandleWord = System.Int32;
// The unsigned equivalent of HandleWord's underlying type
using HandleWordUnsigned = System.UInt32;

namespace KSoft.Phoenix
{
	partial class PhxUtil
	{
		const HandleWordUnsigned K_UNDEFINED_REFERENCE_HANDLE_BITMASK_ =
			unchecked((HandleWordUnsigned)HandleWord.MinValue); // 0x80...

		public static bool IsUndefinedReferenceHandle(HandleWord handle)
		{
			var uhandle = (HandleWordUnsigned)handle;

			return (uhandle & K_UNDEFINED_REFERENCE_HANDLE_BITMASK_) != 0;
		}
		public static HandleWord GetUndefinedReferenceDataIndex(HandleWord undefinedRefHandle)
		{
			var uhandle = (HandleWordUnsigned)undefinedRefHandle;

			return (HandleWord)(uhandle & ~K_UNDEFINED_REFERENCE_HANDLE_BITMASK_);
		}
		public static HandleWord GetUndefinedReferenceHandle(HandleWord undefinedRefDataIndex)
		{
			Contract.Requires(undefinedRefDataIndex < HandleWord.MaxValue,
				"Index value would generate a handle that matches the general invalid-handle sentinel");

			var index = (HandleWordUnsigned)undefinedRefDataIndex;

			return (HandleWord)(index | K_UNDEFINED_REFERENCE_HANDLE_BITMASK_);
		}

		public static bool IsUndefinedReferenceHandleOrNone(HandleWord handle)
		{
			if (IsUndefinedReferenceHandle(handle))
				return true;

			return handle.IsNone();
		}
	};

	public struct UndefinedObjectResult
	{
		public int MemberId { get; private set; }
		public string MemberName { get; private set; }

		public UndefinedObjectResult(int id, string name)
		{
			this.MemberId = id;
			this.MemberName = name;
		}
	};
};