using System.Windows.Forms;
using System.Reflection;
using igLibrary.Core;

namespace igCauldron2.FieldEditors
{
	public abstract class FieldEditor : FlowLayoutPanel
	{
		public Label _lblFieldName;
		public Control _ctrlFieldValue;
		public FieldInfo _dnField;
		public igMetaField _metaField;
		public igObject? _target;

		public FieldEditor(igObject? target, igMetaField metaField, FieldInfo dnField) : base()
		{
			AutoSize = true;
			FlowDirection = FlowDirection.LeftToRight;

			_lblFieldName = new Label();
			_lblFieldName.Text = metaField._name;
			_metaField = metaField;
			_dnField = dnField;
			_target = target;
		}
		public void Finalize()
		{
			Controls.Add(_lblFieldName);
			Controls.Add(_ctrlFieldValue);
		}
		
	}
}