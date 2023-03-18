namespace igLibrary.Core
{
	public class igTObjectList<T> : igTDataList<igObject> where T : igObject
	{
		public Type GetElementType() => typeof(T);
	}
	public class igObjectList : igTObjectList<igObject>{}
}