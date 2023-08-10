namespace igLibrary.Vfx
{
	public unsafe struct igVfxRangedCurve
	{
		public readonly igVfxCurveKeyframe[] _keyframes;
		public float _field_0x3C;
		public float _field_0x40;
		public float _field_0x44;
		public float _field_0x48;
		public ushort _field_0x4C;
		public bool _field_0x4E;
		public byte _flags;
		public ushort _field_0x50;
		public ushort _field_0x52;
		public igVfxRangedCurve()
		{
			_keyframes = new igVfxCurveKeyframe[5];
			_field_0x3C = default;
			_field_0x40 = default;
			_field_0x44 = default;
			_field_0x48 = default;
			_field_0x4C = default;
			_field_0x4E = default;
			_flags = default;
			_field_0x50 = default;
			_field_0x52 = default;
		}
	}
}