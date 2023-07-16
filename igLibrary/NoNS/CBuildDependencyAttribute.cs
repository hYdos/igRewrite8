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
				string cSharpValue = depStrings[i].Replace("%s", "{0}");
				string depName = string.Format(cSharpValue, value).Replace('\\', '/');
				saver.AddBuildDependency(depName);
			}
		}
	}
}