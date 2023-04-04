using igLibrary.Core;

namespace igCauldron3
{
	public class igDataListOverride : InspectorDrawOverride
	{
		public igDataListOverride()
		{
			_t = typeof(IigDataList);
		}
		public override void Draw(ObjectManagerFrame objFrame, igObject obj, igMetaObject meta)
		{
			IigDataList dataList = (IigDataList)obj;
			IigMemory memValue = dataList.GetData();
			object? hahaCastGoBrrrr = memValue;
			objFrame.RenderField("Data", ref hahaCastGoBrrrr, meta._metaFields[2]);
			objFrame.RenderObjectFields(obj, meta, 3);
		}
	}
}