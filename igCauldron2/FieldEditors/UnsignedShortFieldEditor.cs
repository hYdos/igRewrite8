using System.Windows.Forms;
using System.Reflection;
using igLibrary.Core;

namespace igCauldron2.FieldEditors
{
	public class UnsignedShortFieldEditor : FieldEditor
	{
		public UnsignedShortFieldEditor(igObject target, igUnsignedShortMetaField metaField, FieldInfo dnField) : base(target, metaField, dnField)
		{
			_ctrlFieldValue = new TextBox();
			_ctrlFieldValue.Text = dnField.GetValue(target).ToString();
			_ctrlFieldValue.KeyPress += new KeyPressEventHandler(OnShortKeyPress);
		}
		private void OnShortKeyPress(object? sender, KeyPressEventArgs e)
		{
			string testStr = _ctrlFieldValue.Text + e.KeyChar;
			bool validUshort = ushort.TryParse(testStr, out ushort value);
			e.Handled = validUshort;
			if(validUshort)
			{
				_dnField.SetValue(_target, value);
				_ctrlFieldValue.Text = testStr;
			}
		}
         protected override void OnPaint(PaintEventArgs pe)
         {
             base.OnPaint(pe);
         } 
	}
}