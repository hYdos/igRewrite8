using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for opening an igObjectDirectory by path
	/// </summary>
	public sealed class DirectoryOpenerFrame : DirectoryActionFrame
	{
		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public DirectoryOpenerFrame(Window wnd) : base(wnd, "Open Directory", "Open"){}


		/// <summary>
		/// Callback function when the confirmation button is pressed
		/// </summary>
		protected override void OnActionStart()
		{
			DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(_path)!);

			Close();
		}
	}
}