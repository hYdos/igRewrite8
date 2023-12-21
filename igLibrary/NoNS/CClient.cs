using System.Reflection;

namespace igLibrary
{
	public class CClient : igSingleton<CClient>
	{
		public igVector<CManager> _managers = new igVector<CManager>();
		public void AddManager<T>() where T : CManager, new()
		{
			T manager = new T();
			FieldInfo? instanceField = typeof(T).GetField("_Instance");
			if(instanceField == null) throw new Exception($"{typeof(T).FullName} class was incorrectly declared, and is missing an _Instance field");
			instanceField.SetValue(null, manager);
			_managers.Append(manager);
			manager.Intialize();
		}
	}
}