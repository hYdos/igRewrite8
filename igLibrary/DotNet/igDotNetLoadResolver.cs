namespace igLibrary.DotNet
{
	public class igDotNetLoadResolver : igObject
	{
		public StreamHelper _stringTable;
		public DotNetRuntime _runtime;
		public Dictionary<string, string> _aliases = new Dictionary<string, string>();
		public Dictionary<string, igBaseMeta> _pending = new Dictionary<string, igBaseMeta>();
		public void AppendMeta(igBaseMeta meta)
		{
			_pending.Add(meta._name, meta);
		}
	}
}