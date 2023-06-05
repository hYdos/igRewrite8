namespace igLibrary.DotNet
{
	public class DotNetTypeDetails : igObject
	{
		public igDotNetTypeReference _baseType;
		public int _interfaceOffset;
		public int _interfaceCount;
		public int _templateParameterOffset;
		public int _templateParameterCount;
		public igBaseMeta _targetMeta;
		public bool _ownsMeta = true; 
	}
	public class DotNetTypeDetailsList : igTObjectList<DotNetTypeDetails>{}
}