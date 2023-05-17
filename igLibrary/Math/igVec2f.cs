namespace igLibrary.Math
{
	public struct igVec2f
	{
		public float _x;
		public float _y;
		public igVec2f(float x, float y)
		{
			_x = x;
			_y = y;
		}
		public static implicit operator System.Numerics.Vector2(igVec2f vec)
		{
			return new System.Numerics.Vector2(vec._x, vec._y);
		}
		public static implicit operator igVec2f(System.Numerics.Vector2 vec)
		{
			return new igVec2f(vec.X, vec.Y);
		}
	}
}