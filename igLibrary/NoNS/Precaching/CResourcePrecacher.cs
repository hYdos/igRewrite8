using System.Xml.Serialization;

namespace igLibrary
{
	public class CResourcePrecacher : igObject
	{
		public static EMemoryPoolID mDestMemoryPoolId;
		public virtual void Precache(string filePath)
		{

		}
		public virtual void Recache(string filePath)
		{

		}
		public virtual void Uncache(EMemoryPoolID poolId)
		{

		}
	}
	public class CResourcePrecacherList : igTObjectList<CResourcePrecacher>{}
}