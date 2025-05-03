/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Globalization;
using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedQuadraticMetaField : igMetaField
	{
		private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");
		private igRangedQuadratic data;

		public override object? ReadIGZField(igIGZLoader loader)
		{
			data = new igRangedQuadratic
			{
				_a1 = loader._stream.ReadSingle(),
				_b1 = loader._stream.ReadSingle(),
				_c1 = loader._stream.ReadSingle(),
				
				_a2 = loader._stream.ReadSingle(),
				_b2 = loader._stream.ReadSingle(),
				_c2 = loader._stream.ReadSingle(),
				
				_minX = loader._stream.ReadSingle(),
				_maxX = loader._stream.ReadSingle()
			};
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			section._sh.WriteSingle(((igRangedQuadratic)value!)._a1);
			section._sh.WriteSingle(((igRangedQuadratic)value)._b1);
			section._sh.WriteSingle(((igRangedQuadratic)value)._c1);
			
			section._sh.WriteSingle(((igRangedQuadratic)value)._a2);
			section._sh.WriteSingle(((igRangedQuadratic)value)._b2);
			section._sh.WriteSingle(((igRangedQuadratic)value)._c2);
			
			section._sh.WriteSingle(((igRangedQuadratic)value)._minX);
			section._sh.WriteSingle(((igRangedQuadratic)value)._maxX);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igRangedQuadratic);
		
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			if (input == "(null)")
			{
				target = null;
				return true;
			}
			
			var split = input.Split(',');
			data._a1 = float.Parse(split[0], Culture);
			data._b1 = float.Parse(split[1], Culture);
			data._c1 = float.Parse(split[2], Culture);
			
			data._a2 = float.Parse(split[3], Culture);
			data._b2 = float.Parse(split[4], Culture);
			data._c2 = float.Parse(split[5], Culture);
			
			data._maxX = float.Parse(split[6], Culture);
			data._minX = float.Parse(split[7], Culture);
			return true;
		}
	}
}