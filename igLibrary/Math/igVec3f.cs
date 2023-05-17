namespace igLibrary.Math
{
	public struct igVec3f
	{
		public float _x;
		public float _y;
		public float _z;
		public igVec3f(float x, float y, float z)
		{
			_x = x;
			_y = y;
			_z = z;
		}
		public static implicit operator System.Numerics.Vector3(igVec3f vec)
		{
			return new System.Numerics.Vector3(vec._x, vec._y, vec._z);
		}
		public static implicit operator igVec3f(System.Numerics.Vector3 vec)
		{
			return new igVec3f(vec.X, vec.Y, vec.Z);
		}
	}
}