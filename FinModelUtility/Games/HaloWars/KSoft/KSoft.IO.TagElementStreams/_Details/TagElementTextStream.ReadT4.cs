
namespace KSoft.IO
{
	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region ReadElement impl
		protected override void ReadElement(TCursor n, ref string value)
		{
			value = this.GetInnerText(n);
		}
		protected override void ReadElement(TCursor n, ref char value)
		{
			TagElementTextStreamUtils.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		protected override void ReadElement(TCursor n, ref bool value)
		{
			TagElementTextStreamUtils.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		protected override void ReadElement(TCursor n, ref float value)
		{
			TagElementTextStreamUtils.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		protected override void ReadElement(TCursor n, ref double value)
		{
			TagElementTextStreamUtils.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}

		protected override void ReadElement(TCursor n, ref byte value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref sbyte value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref ushort value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref short value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref uint value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref int value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref ulong value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		protected override void ReadElement(TCursor n, ref long value, NumeralBase fromBase)
		{
			Numbers.ParseString(this.GetInnerText(n), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		#endregion

		#region ReadAttribute
		public override void ReadAttribute(string name, ref string value)
		{
			value = this.ReadAttribute(name);
		}
		public override void ReadAttribute(string name, ref char value)
		{
			TagElementTextStreamUtils.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		public override void ReadAttribute(string name, ref bool value)
		{
			TagElementTextStreamUtils.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		public override void ReadAttribute(string name, ref float value)
		{
			TagElementTextStreamUtils.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}
		public override void ReadAttribute(string name, ref double value)
		{
			TagElementTextStreamUtils.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_,
			                                      this.mReadErrorState_);
		}

		public override void ReadAttribute(string name, ref byte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref sbyte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref ushort value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref short value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref uint value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref int value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref ulong value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override void ReadAttribute(string name, ref long value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			Numbers.ParseString(this.ReadAttribute(name), ref value, K_THROW_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		#endregion

		#region ReadElementOpt
		public override bool ReadElementOpt(string name, ref string value)
		{
			return (value = this.ReadElementOpt(name)) != null;
		}
		public override bool ReadElementOpt(string name, ref char value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadElementOpt(string name, ref bool value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadElementOpt(string name, ref float value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadElementOpt(string name, ref double value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}

		public override bool ReadElementOpt(string name, ref byte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref sbyte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref ushort value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref short value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref uint value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref int value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref ulong value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadElementOpt(string name, ref long value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadElementOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		#endregion

		#region ReadAttributeOpt
		public override bool ReadAttributeOpt(string name, ref string value)
		{
			return (value = this.ReadAttributeOpt(name)) != null;
		}
		public override bool ReadAttributeOpt(string name, ref char value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadAttributeOpt(string name, ref bool value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadAttributeOpt(string name, ref float value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}
		public override bool ReadAttributeOpt(string name, ref double value)
		{
			return TagElementTextStreamUtils.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_,
			                                             this.mReadErrorState_);
		}

		public override bool ReadAttributeOpt(string name, ref byte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref sbyte value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref ushort value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref short value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref uint value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref int value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref ulong value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref long value, NumeralBase fromBase=K_DEFAULT_RADIX)
		{
			return Numbers.ParseString(this.ReadAttributeOpt(name), ref value, K_NO_EXCEPT_, this.mReadErrorState_, fromBase);
		}
		#endregion
	};
}