namespace igCauldron3
{
	public abstract class Frame
	{
		protected List<Frame> _children = new List<Frame>();
		public virtual void Render()
		{
			for(int i = 0; i < _children.Count; i++)
			{
				_children[i].Render();
			}
		}
	}
}