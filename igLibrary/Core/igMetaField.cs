using System.Reflection;

namespace igLibrary.Core
{
	public class igMetaField : igObject
	{
		public struct Properties
		{
			//Others will be added as I figure out their structures
			public uint _storage;
			public bool _persistent
			{
				get => ((_storage >> 13) & 1) != 0;
				set => _storage = (uint)(_storage & ~0x2000) | (uint)((value ? 1 : 0) << 13);
			}
			public bool _writeOut
			{
				get => ((_storage >> 18) & 1) != 0;
				set => _storage = (uint)(_storage & ~0x40000) | (uint)((value ? 1 : 0) << 18);
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
			sh.WriteUInt32(_properties._storage);
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
			_properties._storage = sh.ReadUInt32();
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

		public virtual object? GetDefault(igObject target) => _default;

		public virtual igMetaField CreateFieldCopy() => (igMetaField)this.MemberwiseClone();
	}
	public class igMetaFieldList : igTObjectList<igMetaField>{}
}