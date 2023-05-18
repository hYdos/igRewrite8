namespace igLibrary.Core
{
	public class igLocalizedStringData : igObject
	{
		public string _string;
		public igHandle _object;
		public igStringMetaField _field;
	}
	public class igLocalizedStringDataList : igTObjectList<igLocalizedStringData> {}
}