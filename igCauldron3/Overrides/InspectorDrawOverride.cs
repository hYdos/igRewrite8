using igLibrary.Core;

namespace igCauldron3
{
	public abstract class InspectorDrawOverride
	{
		public Type _t { get; protected set; }
		public abstract void Draw(ObjectManagerFrame objFrame, igObject obj, igMetaObject meta);
	}
}