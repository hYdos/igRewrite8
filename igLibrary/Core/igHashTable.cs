using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace igLibrary.Core
{
    //God bless LG-RZ
	public abstract class igTUHashTable<T, U> : igContainer, IEnumerable<KeyValuePair<U, T>>, IigHashTable//, IDictionary<U, T>
	{
		public igMemory<T> _values;
		public igMemory<U> _keys;
		public int _hashItemCount;
		public bool _autoRehash;
		public float _loadFactor;

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
			/*KeyValuePair<U, T>[] kvps = new KeyValuePair<U, T>[_hashItemCount];
			for(int i = 0, hashI = 0; i < _keys.Length && hashI < _hashItemCount; i++)
			{
				if(_keys[i] != null)
				{
					kvps[hashI] = new KeyValuePair<U, T>(_keys[i], _values[i]);
					hashI++;
				}
			}
			return (IEnumerator<KeyValuePair<U, T>>)kvps.GetEnumerator();*/

			for(int i = 0, hashI = 0; i < _keys.Length && hashI < _hashItemCount; i++)
			{
				if(KeyTraitsEqual(_keys[i], KeyTraitsInvalid()))
				{
					yield return new KeyValuePair<U, T>(_keys[i], _values[i]);
				}
			}			
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
			_keys[keyIndex] = KeyTraitsInvalid();
		}
		public virtual U KeyTraitsInvalid()
		{
			     if(!typeof(U).IsValueType)      return default(U);
			else if(typeof(U) == typeof(uint))   return (U)(object)0xFAFAFAFA;
			else if(typeof(U) == typeof(int))    return (U)(object)-84215046;
			else if(typeof(U) == typeof(ulong))  return (U)(object)0xFAFAFAFAFAFAFAFA;
			else if(typeof(U) == typeof(long))   return (U)(object)-361700864190383366;
			else if(typeof(U) == typeof(ushort)) return (U)(object)0x7FFF;
			else if(typeof(U).IsEnum)
			{
				igMetaObject meta = GetMeta();
				igMemoryRefMetaField keysField = (igMemoryRefMetaField)meta._metaFields[1];
				return (U)keysField._memType.GetDefault(null);
			}
			else throw new NotImplementedException("Key type " + typeof(U).Name + " is not implemented.");
		}
        public static uint HashString(string name)
        {
            return HashString(name, 0x811c9dc5);
        }

        public static uint HashString(string name, uint basis)
        {
            for (int i = 0; i < name.Length; i++)
            {
                basis = (basis ^ name[i]) * 0x1000193;
            }

            return basis;
        }

        public static uint HashStringi(string name)
        {
            return HashStringi(name, 0x811c9dc5);
        }

        public static uint HashStringi(string name, uint basis)
        {
            for (int i = 0; i < name.Length; i++)
            {
                basis = (basis ^ char.ToLower(name[i])) * 0x1000193;
            }

            return basis;
        }

        public static uint HashInt(uint integer) => HashInt(unchecked((int)integer));
        public static uint HashInt(int integer)
        {
            uint hash = (uint)(integer + (integer << 0xf));
            hash = (hash >> 10 ^ hash) * 9;
            hash = hash ^ hash >> 6;
            hash = hash + ~(hash << 0xb);
            return hash >> 0x10 ^ hash;
        }

        public static uint HashLong(long integer)
        {
            ulong hash = (ulong)(integer * 0x40000 + ~integer);
            hash = (hash >> 0x1f ^ hash) * 0x15;
            hash = (hash >> 0xb ^ hash) * 0x41;
            return (uint)(hash >> 0x16) ^ (uint)hash;
        }

        public static uint HashLong(ulong integer)
        {
            ulong hash = integer * 0x40000 + ~integer;
            hash = (hash >> 0x1f ^ hash) * 0x15;
            hash = (hash >> 0xb ^ hash) * 0x41;
            return (uint)(hash >> 0x16) ^ (uint)hash;
        }
		private uint KeyTraitsHash(U key)
		{
			if(key is int ksi)     return HashInt(ksi);
			if(key is uint kui)    return HashInt(kui);
			if(key is short kss)   return HashInt(kss);
			if(key is ushort kus)  return HashInt(kus);
			if(key is long ksl)    return HashLong(ksl);
			if(key is ulong kul)   return HashLong(kul);
			if(key is string ks)   return HashString(ks);
			if(key is igObject ko) return HashInt(ko.GetHashCode());	//I hash the hash
			if(typeof(U).IsEnum)   return HashInt((int)(object)key);
			else throw new NotImplementedException($"Unimplemented key type {typeof(U)}");
		}
		private bool KeyTraitsEqual(U key1, U key2)
		{
			return KeyTraitsHash(key1) == KeyTraitsHash(key2);
		}
        public int FindSlot(int count, uint hash, U key)
        {
            if (count != 0)
            {
                uint remainder = hash % (uint)count;
                uint keyIndex = remainder;
                for (int i = 0; i < count; i++)
                {
                    var key1 = _keys[keyIndex];

                    remainder = keyIndex;

                    if (KeyTraitsEqual(key1, key))
                        return (int)keyIndex;

                    if (KeyTraitsEqual(key1, KeyTraitsInvalid()))
                        return (int)keyIndex;

                    keyIndex = 0;

                    if ((remainder + 1) != count)
                        keyIndex = remainder + 1;
                }

            }
            return -1;
        }
        public bool IsValidKey(U key)
        {
            return !KeyTraitsEqual(key, KeyTraitsInvalid());
        }
		private void SetInternal(int index, U key, T value)
		{
			_keys[index] = key;
			_values[index] = value;
		}
        public void SetCapacity(int count)
        {
            igMemory<U> keys = _keys;
            igMemory<T> values = _values;
            int length = _keys.Length;
            if (length == count)
            {
                keys = _keys.CreateCopy(); // make a backup
                values = _values.CreateCopy(); // make a backup
                KeyTraitsInvalidateAll();
            }
            else
            {
                keys = _keys.CreateCopy();
                values = _values.CreateCopy();
                _keys = new igMemory<U>();
                _values = new igMemory<T>();
            }
            _hashItemCount = 0;
            if (length != 0)
            {
                for (int i = 0; i < length; i++)
                {
                    U key = keys[i];
                    bool isEqual = KeyTraitsEqual(key, KeyTraitsInvalid());
                    if (!isEqual)
                    {
                        uint hash = KeyTraitsHash(key);

                        if (!InsertInternal(key, values[i], hash))
                        {
                            return;
                        }
                    }
                }
            }
            KeyTraitsInvalidateAll();
            //ValueTraitsInvalidate(values);
        }
        public bool InsertInternal(U key, T value, uint hash)
        {
            int slot = FindSlot(_keys.Length, hash, key);
            if (slot == -1)
            {
                if (!_autoRehash)
                    return false;
            }
            else
            {
                if (!IsValidKey(_keys[slot]))
                    _hashItemCount++;

                SetInternal(slot, key, value);

                if (!_autoRehash)
                    return true;
            }

            if (slot == -1 || _keys.Length == 0)
            {
                SetCapacity(_keys.Length * 2);

                if (slot == -1)
                {
                    _autoRehash = false;
                    bool inserted = InsertInternal(key, value, hash);
                    _autoRehash = true;

                    return inserted;
                }
            }

            return true;
        }

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(U key, T value)
		{
			int index = GetKeyIndex(key);
			if(index >= 0) throw new ArgumentException($"Key {key} already exists");
			InsertInternal(key, value, KeyTraitsHash(key));
		}
        public void Remove(U key) 
        {
            throw new NotImplementedException("Removing items from hash tables is not implemented");
        }
		public bool ContainsKey(U key)
		{
			int index = GetKeyIndex(key);
			return index >= 0;
		}

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