using System.Reflection;

namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public uint refCount;
		public igMemoryPool internalMemoryPool;

		public virtual igMetaObject GetMeta()
		{
			//The following have their own _meta field which is used for GetMeta:
			// - igDynamicObject
			// - igGuiButtonBehavior
			// - igGuiDotNetAction
			// - igGuiDotNetBehavior
			// - CBehaviorLogic
			// - COnlyOneBehaviorLogic
			// - igGuiDotNetListItem
			// - List_1
			// - igGuiVisualBehavior
			// - CDotNetEntityMessageList
			// - COnlyOneBehaviorLogic
			// - Dictionary_2
			// - CDotNetWaypoint
			// - igGuiDotNetListItemPopulator
			// - igGuiDotNetEvent
			// - igGuiDotNetListItemConverter
			//All of these use the field name _meta
			//This is why the following code is weird, try to replace this with overriding the original function, in the meantime we have this
			Type thisType = GetType();
			FieldInfo? fi = thisType.GetField("_meta");
			if(fi != null)
				return (igMetaObject)fi.GetValue(this);
			else return igArkCore.GetObjectMeta(GetType().Name);
		}
	}
}