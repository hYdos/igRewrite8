namespace igLibrary.Math
{
	public struct igVec4i
	{
		public int _x;
		public int _y;
		public int _z;
		public int _w;
		public igVec4i(int x, int y, int z, int w)
		{
			_x = x;
			_y = y;
			_z = z;
			_w = w;
		}
		public static implicit operator System.Numerics.Vector4(igVec4i vec)
		{
			return new System.Numerics.Vector4(vec._x, vec._y, vec._z, vec._w);
		}
		public static implicit operator igVec4i(System.Numerics.Vector4 vec)
		{
			return new igVec4i((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);
		}
	}
}