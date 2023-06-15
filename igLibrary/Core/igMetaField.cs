using System.Reflection;

namespace igLibrary.Core
{
	public class igMetaField : igObject
	{
		public enum CopyType
		{
			kCopyByValue,
			kCopyByReference,
			kCopyByNoCopy,
			kCopyByDefault,
			kCopyTypeMax
		}
		public enum ResetType
		{
			kResetByValue,
			kResetByReference,
			kResetByNoReset,
			kResetByDefault,
			kResetTypeMax
		}
		public enum IsAlikeCompareType
		{
			kIsAlikeCompareValue,
			kIsAlikeCompareReference,
			kIsAlikeCompareNoCompare,
			kIsAlikeCompareDefault,
			kIsAlikeCompareTypeMax
		}
		public struct Properties
		{
			public CopyType _copyMethod;
			public ResetType _resetMethod;
			public IsAlikeCompareType _isAlikeCompareMethod;
			public CopyType _itemsCopyMethod;
			public CopyType _keysCopyMethod;
			public bool _persistent;
			public bool _hasInvariance;
			public bool _hasPoolName;
			public bool _mutable;
			public bool _implicitAlignment;

			//I'm so sorry
			internal int getArkStorage() => (int)_copyMethod | (int)_resetMethod << 2 | (int)_isAlikeCompareMethod << 4 | (int)_itemsCopyMethod << 6 | (int)_keysCopyMethod << 8 | bts(_persistent, 10) | bts(_hasInvariance, 11) | bts(_hasPoolName, 12) | bts(_mutable, 13) | bts(_implicitAlignment, 14);
			private int bts(bool value, byte shift) => (value ? 1 : 0) << shift;
			private bool stb(int value, byte shift) => ((value >> shift) & 1) != 0;
			internal void setArkStorage(int value)
			{
				_copyMethod = (CopyType)((value >> 0) & 3);
				_resetMethod = (ResetType)((value >> 2) & 3);
				_isAlikeCompareMethod = (IsAlikeCompareType)((value >> 4) & 3);
				_itemsCopyMethod = (CopyType)((value >> 6) & 3);
				_keysCopyMethod = (CopyType)((value >> 8) & 3);
				_persistent = stb(value, 10);
				_hasInvariance = stb(value, 11);
				_hasPoolName = stb(value, 12);
				_mutable = stb(value, 13);
				_implicitAlignment = stb(value, 14);
			}
		}
		public string? _name;
		public ushort _offset;
		public Dictionary<IG_CORE_PLATFORM, ushort> _offsets = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public igBaseMeta _parentMeta;
		public Properties _properties;
		public object? _default;

		public virtual void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			if(this is igPlaceHolderMetaField placeholder)
			{
				saver.SaveString(sh, placeholder._platformInfo._name);
			}
			else
			{
				saver.SaveString(sh, GetType().Name);
			}
			sh.WriteInt32(_properties.getArkStorage());
			uint templateParameterCount = GetTemplateParameterCount();
			sh.WriteUInt32(templateParameterCount);
			for(uint i = 0; i < templateParameterCount; i++)
			{
				igMetaField? templateParam = GetTemplateParameter(i);
				saver.SaveMetaField(sh, templateParam);
			}
			saver.SaveString(sh, _name);

			FieldInfo? numField = GetType().GetField("_num");
			if(numField != null)
			{
				short num = (short)numField.GetValue(this);
				sh.WriteInt16(num);
			}
			else
			{
				sh.WriteInt16((short)-1);
			}
			sh.WriteUInt16(_offset);

			if(_default == null) sh.WriteInt32(-1);
			else DumpDefault(saver, sh);
		}
		public virtual void DumpDefault(igArkCoreFile saver, StreamHelper sh){}
		public virtual void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			_properties.setArkStorage(sh.ReadInt32());
			uint templateArgCount = sh.ReadUInt32();
			SetTemplateParameterCount(templateArgCount);
			for(uint i = 0; i < templateArgCount; i++)
			{
				SetTemplateParameter(i, loader.ReadMetaField(sh));
			}
			_name = loader.ReadString(sh);
			short num = sh.ReadInt16();
			_offset = sh.ReadUInt16();
			FieldInfo? numField = GetType().GetField("_num");
			if(numField != null)
			{
				numField.SetValue(this, num);
			}
			int size = sh.ReadInt32();
			if(size > 0) UndumpDefault(loader, sh);
		}
		public virtual void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			sh.BaseStream.Position -= 4;
			int size = sh.ReadInt32();
			sh.BaseStream.Position += size;
		}

		public virtual object? ReadIGZField(igIGZLoader loader) => throw new NotImplementedException();
		public virtual void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => throw new NotImplementedException();
		public virtual Type GetOutputType() => typeof(object);
		public virtual uint GetSize(IG_CORE_PLATFORM platform) => throw new NotImplementedException();
		public virtual uint GetAlignment(IG_CORE_PLATFORM platform) => throw new NotImplementedException();

		public virtual void Commission(ref object target){}
		public virtual void SetTemplateParameter(uint index, igMetaField meta){}
		public virtual void SetTemplateParameterCount(uint count){}
		public virtual igMetaField? GetTemplateParameter(uint index) => null;
		public virtual uint GetTemplateParameterCount() => 0;

		public virtual object? GetDefault(igObject target)
		{
			FieldInfo? fi = GetType().GetField("_num");
			if(fi != null)
			{
				short num = (short)fi.GetValue(this);
				Array arrayDefault = Array.CreateInstance(GetOutputType().GetElementType(), num);
				if(_default != null)
				{
					for(int i = 0; i < num; i++)
					{
						arrayDefault.SetValue(_default, i);
					}
				}
				return arrayDefault;
			}
			else return _default;
		}

		public virtual igMetaField CreateFieldCopy() => (igMetaField)this.MemberwiseClone();
	}
	public class igMetaFieldList : igTObjectList<igMetaField>{}
}