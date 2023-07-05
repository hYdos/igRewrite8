namespace igLibrary
{
	public class CBuildDependencyAttribute : igBaseDependenciesAttribute
	{
		public string _value;
		public bool _replaceExtension;
		public void GenerateBuildDependancies(igIGZSaver saver, object value)
		{
			string[] depStrings = _value.Split(';');
			for(int i = 0; i < depStrings.Length; i++)
			{
				string cSharpValue = _value.Replace("%s", "{0}");
				string depName = string.Format(cSharpValue, value).Replace('\\', '/');
				if(saver._buildDependancies.Any(x => x.Item2 == depName));
				saver._buildDependancies.Add((Path.GetFileNameWithoutExtension(depName), depName));
			}
		}
	}
}