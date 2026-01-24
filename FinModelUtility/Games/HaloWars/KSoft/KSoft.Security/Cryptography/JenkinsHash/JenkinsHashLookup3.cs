#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	// http://burtleburtle.net/bob/c/lookup3.c
	// #REVIEW: hashlittle2 support (two results)?
	public abstract class JenkinsHashLookup3 : JenkinsHashLookup
	{
		const uint K_GOLDEN_RATIO_ = 0xDEADBEEF;
		const int K_BLOCK_SIZE_ = 12;

		static uint Rot(uint x, int k)
		{
			return (x << k) | (x >> (Bits.K_INT32_BIT_COUNT-k));
		}

		struct HashState
		{
			uint a_, b_, c_;

			public uint Result { get { return this.c_; } }

			public HashState(int length, uint seed)
			{
				this.a_ = this.b_ = this.c_ = K_GOLDEN_RATIO_ + (uint)length + seed;
			}

			void Mix()
			{
				this.a_ -= this.c_;
				this.a_ ^= Rot(this.c_, 4);
				this.c_ += this.b_;
				this.b_ -= this.a_;
				this.b_ ^= Rot(this.a_, 6);
				this.a_ += this.c_;
				this.c_ -= this.b_;
				this.c_ ^= Rot(this.b_, 8);
				this.b_ += this.a_;
				this.a_ -= this.c_;
				this.a_ ^= Rot(this.c_,16);
				this.c_ += this.b_;
				this.b_ -= this.a_;
				this.b_ ^= Rot(this.a_,19);
				this.a_ += this.c_;
				this.c_ -= this.b_;
				this.c_ ^= Rot(this.b_, 4);
				this.b_ += this.a_;
			}

			void FinalMix()
			{
				this.c_ ^= this.b_;
				this.c_ -= Rot(this.b_, 14);
				this.a_ ^= this.c_;
				this.a_ -= Rot(this.c_, 11);
				this.b_ ^= this.a_;
				this.b_ -= Rot(this.a_, 25);
				this.c_ ^= this.b_;
				this.c_ -= Rot(this.b_, 16);
				this.a_ ^= this.c_;
				this.a_ -= Rot(this.c_, 4);
				this.b_ ^= this.a_;
				this.b_ -= Rot(this.a_, 14);
				this.c_ ^= this.b_;
				this.c_ -= Rot(this.b_, 24);
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
				JenkinsHashLookup.FinalFill(ref this.a_, ref this.b_, ref this.c_, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref this.a_, ref this.b_, ref this.c_, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
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
				if(length > 0)
					this.FinalMix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				if (length > 0)
					this.FinalMix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				if (length > 0)
					this.FinalMix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(length, seed);
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

			HashState state = new HashState(length, seed);
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

			HashState state = new HashState(length, seed);
			for (; index + K_BLOCK_SIZE_ <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}