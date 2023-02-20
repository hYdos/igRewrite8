using System.Reflection.Emit;

namespace igLibrary.Core
{
	public class igMetaEnum : igBaseMeta
	{
		public List<string> _names = new List<string>();
		public List<int> _values = new List<int>();

		public override void PostUndump()
		{
			Type? t = igArkCore.GetEnumDotNetType(_name);
			if(t != null)
			{
				_internalType = t;
			}
		}
		public object GetEnumFromValue(int value)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			int index = _values.IndexOf(value);
			if(index < 0) throw new KeyNotFoundException($"Value {value} not found in enum {_name}");

			return Enum.Parse(_internalType, _names[index]);
		}
	}
}