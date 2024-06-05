namespace igCauldron3
{
	public abstract class Frame
	{
		protected List<Frame> _children = new List<Frame>();
		protected Window _wnd;
		public Frame(Window wnd)
		{
			_wnd = wnd;
		}
		public virtual void Render()
		{
			for(int i = 0; i < _children.Count; i++)
			{
				_children[i].Render();
			}
		}
		public virtual void Close()
		{
			_wnd.frames.Remove(this);
		}
		public void AddChild(Frame child)
		{
			_children.Add(child);
		}
	}
}