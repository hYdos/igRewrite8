namespace igLibrary.Math
{
	public struct igVec3uc
	{
		public byte _x;
		public byte _y;
		public byte _z;
		public igVec3uc(byte x, byte y, byte z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3uc vec)
		{
			return new System.Numerics.Vector3(vec._x, vec._y, vec._z);
		}
		public static implicit operator igVec3uc(System.Numerics.Vector3 vec)
		{
			return new igVec3uc((byte)vec.X, (byte)vec.Y, (byte)vec.Z);
		}
	}
}