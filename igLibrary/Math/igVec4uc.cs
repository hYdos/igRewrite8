namespace igLibrary.Math
{
	public struct igVec4uc
	{
		public byte _r;
		public byte _g;
		public byte _b;
		public byte _a;
		public igVec4uc(byte r, byte g, byte b, byte a)
		{
			_r = r;
			_g = g;
			_b = b;
			_a = a;
		}
		public static implicit operator System.Drawing.Color(igVec4uc vec)
		{
			return System.Drawing.Color.FromArgb(vec._a, vec._r, vec._g, vec._b);
		}
		public static implicit operator igVec4uc(System.Drawing.Color vec)
		{
			return new igVec4uc(vec.R, vec.G, vec.B, vec.A);
		}
	}
}