namespace igLibrary.Core
{
	//This is sheer genius
	//https://stackoverflow.com/a/29379250

	public interface IHashTraits<T>
	{
		void KeyTraitsInvalidate(igMemory<T> keys);
	}
}