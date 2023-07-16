namespace igLibrary.Entity
{
	public class igComponentDependencyProvider : igDependencyProvider
	{
		public override void GetBuildDependencies(igObject obj, out igStringRefList? output)
		{
			base.GetBuildDependencies(obj, out output);
		}
	}
}