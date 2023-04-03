namespace igLibrary.Core
{
	//This isn't accurate, igTObjectList<T> actually inherits from igTDataList<igObject>, but this really feels like an oversight
	public class igTObjectList<T> : igTDataList<T> where T : igObject
	{
		public Type GetElementType() => typeof(T);
	}
	public class igObjectList : igTObjectList<igObject>{}
}