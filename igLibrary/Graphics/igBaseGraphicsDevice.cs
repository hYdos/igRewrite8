using igLibrary.Gfx;

namespace igLibrary.Graphics
{
	public abstract class igBaseGraphicsDevice : igTContext<igBaseGraphicsDevice>
	{
		public igBaseGraphicsDevice() : base() {}
		public virtual int CreateTexture(igResourceUsage usage, igImage2 image) => throw new NotImplementedException("Create texture unimplemented");
	}
}