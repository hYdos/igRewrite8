namespace igLibrary.Core
{
	public abstract class igTContext<T> where T : igTContext<T>
	{
		public static T _instance;
	}
}