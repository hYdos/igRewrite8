using System.Reflection;

namespace igLibrary.Core
{
	public class igMetaField : igObject
	{
		public string? _name;
		public ushort _offset;
		public Dictionary<IG_CORE_PLATFORM, ushort> _offsets = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public igBaseMeta _parentMeta;

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
		}
		public virtual void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
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
		}

		public virtual object? ReadIGZField(igIGZLoader loader) => throw new NotImplementedException();
		public virtual Type GetOutputType() => typeof(object);
		public virtual uint GetSize(IG_CORE_PLATFORM platform) => throw new NotImplementedException();
		public virtual uint GetAlignment(IG_CORE_PLATFORM platform) => throw new NotImplementedException();

		public virtual void SetTemplateParameter(uint index, igMetaField meta){}
		public virtual void SetTemplateParameterCount(uint count){}
		public virtual igMetaField? GetTemplateParameter(uint index) => null;
		public virtual uint GetTemplateParameterCount() => 0;

		public virtual igMetaField CreateFieldCopy() => (igMetaField)this.MemberwiseClone();
	}
}