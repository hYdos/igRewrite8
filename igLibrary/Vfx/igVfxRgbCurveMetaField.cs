/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igVfxRgbCurveMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRgbCurve data = new igVfxRgbCurve();
			data._data = loader._stream.ReadBytes(0x110);
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			section._sh.WriteBytes(((igVfxRgbCurve)value!)._data);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x110;
		public override Type GetOutputType() => typeof(igVfxRgbCurve);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// I cannot be bothered to implement this
			Logging.Warn("Tried parsing igVfxRgbCurveMetaField value string when unimplemented, returning success...");
			return true;
		}
	}
}