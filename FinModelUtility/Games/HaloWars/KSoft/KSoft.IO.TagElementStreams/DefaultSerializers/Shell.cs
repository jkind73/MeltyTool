
namespace KSoft.IO
{
	partial class TagElementStreamDefaultSerializer
	{
		public static void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Shell.Processor value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var instSet = reading
				? 0
				: value.InstructionSet;
			var procSize = reading
				? 0
				: value.ProcessorSize;
			var byteOrder = reading
				? 0
				: value.ByteOrder;

			s.StreamAttributeEnum("instructionSet", ref instSet);
			s.StreamAttributeEnum("processorSize", ref procSize);
			s.StreamAttributeEnum("byteOrder", ref byteOrder);

			if (reading)
			{
				value = new Shell.Processor(procSize, byteOrder, instSet);
			}
		}

		public static void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Shell.Platform value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var platformType = reading
				? 0
				: value.Type;
			var processor = reading
				? new Shell.Processor()
				: value.ProcessorType;

			s.StreamAttributeEnum("platformType", ref platformType);
			Serialize(s, ref processor);

			if (reading)
			{
				value = new Shell.Platform(platformType, processor);
			}
		}
	};
}
