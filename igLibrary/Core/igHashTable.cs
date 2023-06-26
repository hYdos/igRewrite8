using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace igLibrary.Core
{
	public abstract class igTUHashTable<T, U> : igContainer, IEnumerable<KeyValuePair<U, T>>, IigHashTable
	{
		public igMemory<T> _values;
		public igMemory<U> _keys;
		public int _hashItemCount;
		public bool _autoRehash;
		public float _loadFactor;
		//private igHashTraits<U> _hashTraits = new igHashTraits<U>();

		public virtual T this[U key]
		{
			get
			{
				int index = GetKeyIndex(key);
				if(index == -1) throw new KeyNotFoundException();
				return _values[index];
			}
			set
			{
				int index = GetKeyIndex(key);
				if(index == -1) throw new KeyNotFoundException();
				_values[index] = value;
			}
		}

		public void Activate(int capacity)
		{
			_values.Realloc(capacity);
			_values._optimalCPUReadWrite = true;
			_keys.Realloc(capacity);
			_keys._optimalCPUReadWrite = true;
			KeyTraitsInvalidateAll();
		}

		protected virtual int GetKeyIndex(U key)
		{
			EqualityComparer<U> comparer = EqualityComparer<U>.Default;
			for(int i = 0; i < _keys.Length; i++)
			{
				if(comparer.Equals(_keys[i], key))
				{
					return i;
				}
			}
			return -1;
		}

		public IEnumerator<KeyValuePair<U, T>> GetEnumerator()
		{
			KeyValuePair<U, T>[] kvps = new KeyValuePair<U, T>[_hashItemCount];
			for(int i = 0, hashI = 0; i < _keys.Length && hashI < _hashItemCount; i++)
			{
				if(_keys[i] != null)
				{
					kvps[hashI] = new KeyValuePair<U, T>(_keys[i], _values[i]);
					hashI++;
				}
			}
			return (IEnumerator<KeyValuePair<U, T>>)kvps.GetEnumerator();
		}

		public virtual void KeyTraitsInvalidateAll()
		{
			for(int i = 0; i < _keys.Length; i++)
			{
				KeyTraitsInvalidate(i);
			}
		}
		public virtual void KeyTraitsInvalidate(int keyIndex)
		{
			     if(!typeof(U).IsValueType)      _keys[keyIndex] = default(U);
			else if(typeof(U) == typeof(uint))   _keys[keyIndex] = (U)(object)0xFAFAFAFA;
			else if(typeof(U) == typeof(int))    _keys[keyIndex] = (U)(object)-84215046;
			else if(typeof(U) == typeof(ulong))  _keys[keyIndex] = (U)(object)0xFAFAFAFAFAFAFAFA;
			else if(typeof(U) == typeof(long))   _keys[keyIndex] = (U)(object)-361700864190383366;
			else if(typeof(U) == typeof(ushort)) _keys[keyIndex] = (U)(object)0x7FFF;
			else throw new NotImplementedException("Key type " + typeof(U).Name + " is not implemented.");
		}
		public virtual bool KeyTraitsInvalid(int keyIndex)
		{
			return false;
			//     if(!typeof(U).IsValueType)                                    return _keys[keyIndex] == default(U);
			//else if(typeof(U) == typeof(int)   || typeof(U) == typeof(uint))   return _keys[keyIndex] == 0xFAFAFAFA;
			//else if(typeof(U) == typeof(long)  || typeof(U) == typeof(ulong))  return _keys[keyIndex] == 0xFAFAFAFAFAFAFAFA;
			//else if(typeof(U) == typeof(short))                                return _keys[keyIndex] == 0x7FFF;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	public class igHashTable : igTUHashTable<byte, byte>
	{
		public static uint HashString(string str) => igHash.Hash(str);
	}
	public interface IigHashTable
	{
		public void Activate(int capacity);
	}
}