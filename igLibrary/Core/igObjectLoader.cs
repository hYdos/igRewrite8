namespace igLibrary.Core
{
	public class igObjectLoader : igObject
	{
		static igObjectLoaderTable _loaders = new igObjectLoaderTable();
		static uint _testFileMaxSize;
		public virtual string GetExtension() => "";
		public virtual string GetType() => "";
		public virtual string GetName() => "";
		public virtual void ReadFile(){}
	}
}