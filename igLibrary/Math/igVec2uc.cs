namespace igLibrary.Math
{
	public struct igVec2uc
	{
		public byte _x;
		public byte _y;
		public igVec2uc(byte x, byte y)
		{
			_x = x;
			_y = y;
		}
		public static implicit operator System.Numerics.Vector2(igVec2uc vec)
		{
			return new System.Numerics.Vector2(vec._x, vec._y);
		}
		public static implicit operator igVec2uc(System.Numerics.Vector2 vec)
		{
			return new igVec2uc((byte)vec.X, (byte)vec.Y);
		}
	}
}