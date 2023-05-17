namespace igLibrary.Math
{
	public struct igVec3d
	{
		public double _x;
		public double _y;
		public double _z;
		public igVec3d(double x, double y, double z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3d vec)
		{
			return new System.Numerics.Vector3((float)vec._x, (float)vec._y, (float)vec._z);
		}
		public static implicit operator igVec3d(System.Numerics.Vector3 vec)
		{
			return new igVec3d(vec.X, vec.Y, vec.Z);
		}
	}
}