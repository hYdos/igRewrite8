namespace igLibrary.Core
{
	public abstract class igObjectLoader : igObject
	{
		public static Dictionary<string, igObjectLoader> _loaders = new Dictionary<string, igObjectLoader>();
		static uint _testFileMaxSize;
		public static void RegisterLoader<T>() where T : igObjectLoader, new()
		{
			T loader = new T();
			_loaders.TryAdd(loader.GetLoaderType(), loader);
		}
		public virtual string GetLoaderExtension() => "";
		public virtual string GetLoaderType() => "";
		public virtual string GetLoaderName() => "";
		public virtual void ReadFile(string filePath, igBlockingType blockingType){}
	}
}