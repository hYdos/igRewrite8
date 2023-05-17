namespace igLibrary.Math
{
	public struct igVec4fUnaligned
	{
		public float _x;
		public float _y;
		public float _z;
		public float _w;
		public igVec4fUnaligned(float x, float y, float z, float w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
		public static implicit operator System.Numerics.Vector4(igVec4fUnaligned vec)
		{
			return new System.Numerics.Vector4(vec._x, vec._y, vec._z, vec._w);
		}
		public static implicit operator igVec4fUnaligned(System.Numerics.Vector4 vec)
		{
			return new igVec4fUnaligned(vec.X, vec.Y, vec.Z, vec.W);
		}
	}
}