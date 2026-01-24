using System;
using System.Diagnostics.CodeAnalysis;
using Pure = System.Diagnostics.Contracts.PureAttribute;
using DebuggerStepThrough = System.Diagnostics.DebuggerStepThroughAttribute;

namespace KSoft
{
	public static class Predicates
	{
		#region IsFalse/True
		private static Predicate<bool> gIsFalse_;
		public static Predicate<bool> IsFalse { get {
			if (gIsFalse_ == null)
				gIsFalse_ = b => !b;

			return gIsFalse_;
		} }

		private static Predicate<bool> gIsTrue_;
		public static Predicate<bool> IsTrue { get {
			if (gIsTrue_ == null)
				gIsTrue_ = b => b;

			return gIsTrue_;
		} }
		#endregion

		#region IsNotNullOrEmpty
		private static Predicate<string> gIsNotNullOrEmpty_;
		public static Predicate<string> IsNotNullOrEmpty { get {
			if (gIsNotNullOrEmpty_ == null)
				gIsNotNullOrEmpty_ = s => !string.IsNullOrEmpty(s);

			return gIsNotNullOrEmpty_;
		} }
		#endregion

		#region HasItems/Bits
		private static Predicate<System.Collections.ICollection> gHasItems_;
		public static Predicate<System.Collections.ICollection> HasItems { get {
			if (gHasItems_ == null)
				gHasItems_ = coll => coll != null && coll.Count > 0;

			return gHasItems_;
		} }

		private static Predicate<Collections.IReadOnlyBitSet> gHasBits_;
		public static Predicate<Collections.IReadOnlyBitSet> HasBits { get {
			if (gHasBits_ == null)
				gHasBits_ = set => set != null && set.Cardinality > 0;

			return gHasBits_;
		} }
		#endregion

		//////////////////////////////////////////////////////////////////////////
		// The following are defined as functions to use type inference at the expense of implicit Predicate<> allocations

		[Pure, DebuggerStepThrough] public static bool True()				{ return true; }
		[Pure, DebuggerStepThrough] public static bool False()				{ return false; }
		[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
		[Pure, DebuggerStepThrough] public static bool True<T>(T dummy)		{ return true; }
		[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
		[Pure, DebuggerStepThrough] public static bool False<T>(T dummy)	{ return false; }

		[Pure, DebuggerStepThrough]
		public static bool IsNotNull<T>(T theObj)
			where T : class
		{ return theObj != null; }

		[Pure, DebuggerStepThrough] public static bool IsNotEmpty(Guid uuid)		{ return uuid != Guid.Empty; }
		[Pure, DebuggerStepThrough] public static bool IsNotEmpty(Values.KGuid uuid)	{ return uuid != Values.KGuid.Empty; }

		[Pure, DebuggerStepThrough] public static bool IsZero(int x)	{ return x == 0; }
		[Pure, DebuggerStepThrough] public static bool IsZero(uint x)	{ return x == 0; }

		[Pure, DebuggerStepThrough] public static bool IsNotZero(sbyte x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(byte x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(short x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(ushort x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(int x)		{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(uint x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(long x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(ulong x)	{ return x != 0; }
		[Pure, DebuggerStepThrough] public static bool IsNotZero(float x)	{ return x != 0.0f; }

		/// <returns>x != -1</returns>
		[Pure, DebuggerStepThrough] public static bool IsNotNone(sbyte x)	{ return x != TypeExtensions.K_NONE_INT8; }
		/// <returns>x != -1</returns>
		[Pure, DebuggerStepThrough] public static bool IsNotNone(short x)	{ return x != TypeExtensions.K_NONE_INT16; }
		/// <returns>x != -1</returns>
		[Pure, DebuggerStepThrough] public static bool IsNotNone(int x)		{ return x != TypeExtensions.K_NONE_INT32; }
		/// <returns>x != -1</returns>
		[Pure, DebuggerStepThrough] public static bool IsNotNone(long x)	{ return x != TypeExtensions.K_NONE_INT64; }
	};
}
