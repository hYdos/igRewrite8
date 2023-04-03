using System.Windows.Forms;
using System.Reflection;
using igLibrary.Core;

namespace igCauldron2.FieldEditors
{
	public class StringFieldEditor : FieldEditor
	{
		public StringFieldEditor(igObject target, igStringMetaField metaField, FieldInfo dnField) : base(target, metaField, dnField)
		{
			_ctrlFieldValue = new TextBox();
			_ctrlFieldValue.Text = (string)dnField.GetValue(target);
			_ctrlFieldValue.KeyPress += new KeyPressEventHandler(OnShortKeyPress);
		}
		private void OnShortKeyPress(object? sender, KeyPressEventArgs e)
		{
			if(_target != null) _dnField.SetValue(_target, _ctrlFieldValue.Text);
		}
         protected override void OnPaint(PaintEventArgs pe)
         {
             base.OnPaint(pe);
         } 
	}
}