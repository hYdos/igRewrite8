using System;
using System.Reflection;
using System.Linq;
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

			if(t == null)
			{
				EnumBuilder eb = igArkCore.GetNewEnumBuilder(_name);

				for(int i = 0; i < _names.Count; i++)
				{
					eb.DefineLiteral(_names[i], _values[i]);
				}

				_internalType = eb.CreateType();
				return;
			}
			else
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
		public int GetValueFromEnum(object enumValue)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			int index = _names.IndexOf(enumValue.ToString());
			if(index < 0) throw new KeyNotFoundException($"Value {enumValue.ToString()} not found in enum {_name}");

			return _values[index];
		}
		public object GetEnumFromName(string name)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			return Enum.Parse(_internalType, name);
		}
	}
}