/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igWideCharMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUInt16();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteUInt16((char)value!);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 2;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 2;
		public override Type GetOutputType() => typeof(char);
	}
}