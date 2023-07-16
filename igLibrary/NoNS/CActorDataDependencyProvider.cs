namespace igLibrary
{
	public class CActorDataDependencyProvider : igDependencyProvider
	{
		public int _version = 7;
		public override void GetBuildDependencies(igObject obj, out igStringRefList? output)
		{
			CActorData actorData = (CActorData)obj;
			output = new igStringRefList();
			string path = _directory._path;
			if(_directory._path.EndsWith("_CharacterData.igz"))
			{
				string excludeRulesPath = path.ReplaceBeginning("data:/", "data:/ExcludeRules/").ReplaceEnd("_CharacterData.igz", "_rule.igz");
				output.Append(excludeRulesPath);
				return;
			}
			else if(_directory._path.StartsWith("maps:/") && !string.IsNullOrWhiteSpace(actorData._characterAnimations))
			{
				throw new NotImplementedException();
				//string animsPath = string.Format("data:/anims/{0}+{1}.hka", actorData._characterAnimations, )
			}

		}
	}
}