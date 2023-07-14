using igLibrary.Core;

namespace igLibrary
{
	public class CBehaviorComponentDataDependencyProvider : igDependencyProvider
	{
		public override void GetBuildDependencies(igObject obj, out igStringRefList? output)
		{
			output = null;
			CBehaviorComponentData behavior = (CBehaviorComponentData)obj;
			if(!string.IsNullOrEmpty(behavior._behaviorFile))
			{
				output = new igStringRefList();
				string animDep = behavior._behaviorFile;
				if(animDep.StartsWith("behaviors:"))
				{
					animDep = "data:/anims" + behavior._behaviorFile.Substring(10);
				}
				animDep = Path.TrimEndingDirectorySeparator(animDep);
				animDep = Path.ChangeExtension(animDep, ".hka");
				output.Append(animDep);
			}
		}
	}
}