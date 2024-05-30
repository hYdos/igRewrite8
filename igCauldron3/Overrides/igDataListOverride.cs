using igLibrary.Core;

namespace igCauldron3
{
	public class igDataListOverride : InspectorDrawOverride
	{
		public igDataListOverride()
		{
			_t = typeof(IigDataList);
		}
		public override void Draw(ObjectManagerFrame objFrame, igObject obj, igMetaObject meta) => throw new NotImplementedException();
		public override void Draw2(DirectoryManagerFrame dirFrame, igObject obj, igMetaObject meta)
		{
			IigDataList dataList = (IigDataList)obj;
			IigMemory memValue = dataList.GetData();
			object? castedObject = memValue;
			if(FieldRenderer.RenderField("Data", ref castedObject, meta._metaFields[2]))
			{
				dataList.SetData(memValue);
				dataList.SetCount(memValue.GetData().Length);
				dataList.SetCapacity(memValue.GetData().Length);
			}
		}
	}
}