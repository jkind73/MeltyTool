#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	/// <remarks>http://bretm.home.comcast.net/~bretm/hash/7.html</remarks>
	// http://burtleburtle.net/bob/c/lookup2.c
	public abstract class JenkinsHashLookup2 : JenkinsHashLookup
	{
		const uint K_GOLDEN_RATIO_ = 0x9E3779B9;
		const int K_BLOCK_SIZE_ = 12; // 96 bits

		struct HashState
		{
			uint a_, b_, c_;

			public uint Result { get { return this.c_; } }

			public HashState(uint seed)
			{
				this.a_ = this.b_ = K_GOLDEN_RATIO_;
				this.c_ = seed;
			}

			void Mix()
			{
				this.a_ -= this.b_;
				this.a_ -= this.c_;
				this.a_ ^= (this.c_ >> 13);
				this.b_ -= this.c_;
				this.b_ -= this.a_;
				this.b_ ^= (this.a_ <<  8);
				this.c_ -= this.a_;
				this.c_ -= this.b_;
				this.c_ ^= (this.b_ >> 13);
				this.a_ -= this.b_;
				this.a_ -= this.c_;
				this.a_ ^= (this.c_ >> 12);
				this.b_ -= this.c_;
				this.b_ -= this.a_;
				this.b_ ^= (this.a_ << 16);
				this.c_ -= this.a_;
				this.c_ -= this.b_;
				this.c_ ^= (this.b_ >>  5);
				this.a_ -= this.b_;
				this.a_ -= this.c_;
				this.a_ ^= (this.c_ >>  3);
				this.b_ -= this.c_;
				this.b_ -= this.a_;
				this.b_ ^= (this.a_ << 10);
				this.c_ -= this.a_;
				this.c_ -= this.b_;
				this.c_ ^= (this.b_ >> 15);
			}

			void Fill(byte[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a_, ref this.b_, ref this.c_, data, ref i);
			}

			void Fill(char[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a_, ref this.b_, ref this.c_, data, ref i);
			}

			void Fill(string data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a_, ref this.b_, ref this.c_, data, ref i);
			}

			void FinalFill(byte[] data, ref int i, int length)
			{
				this.c_ += (uint)length;

				JenkinsHashLookup.FinalFill(ref this.a_, ref this.b_, ref this.c_, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				this.c_ += (uint)length;

				JenkinsHashLookup.FinalFill(ref this.a_, ref this.b_, ref this.c_, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
				this.c_ += (uint)length;

				JenkinsHashLookup.FinalFill(ref this.a_, ref this.b_, ref this.c_, data, ref i, length);
			}

			public void ProcessBlock(byte[] buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessBlock(char[] buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessBlock(string buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessFinalBlock(byte[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				this.Mix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				this.Mix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				this.Mix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(seed);
			for (; index + K_BLOCK_SIZE_ <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(char[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(seed);
			for (; index + K_BLOCK_SIZE_ <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(string buffer, uint seed = 0)
		{
			Contract.Requires(buffer != null);

			int length = buffer.Length;
			int index = 0;

			HashState state = new HashState(seed);
			for (; index + K_BLOCK_SIZE_ <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}