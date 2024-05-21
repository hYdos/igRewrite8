namespace igLibrary.Sg
{
	public class igTraversalMetaObject : igMetaObject
	{
		[Obsolete("This exists for the reflection system, do not use.")] public object? _nodeProperties;             //igVector<igTraversalNodeProperties>
		[Obsolete("This exists for the reflection system, do not use.")] public object? _propagatedNodeProperties;   //igVector<igTraversalNodeProperties>
		[Obsolete("This exists for the reflection system, do not use.")] public bool _root;
	}
}