using System.Drawing;
using igLibrary.Core;

namespace igCauldron2
{
    public partial class Form1 : Form
    {
        public igObjectDirectory _dir;
        public Form1()
        {
            InitializeComponent();
            OnResize(null, null);
        }
        public void OnOpenFileClick(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported game files|*.igz;*.lng|All Files (*.*)|*.*";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    _dir = igObjectStreamManager.Singleton.Load("textures:/ColorMap,textures@!default_c`png,111.igz");
                    RepopulateTree();
                }
            }
        }
        private void RepopulateTree()
        {
            _treeObjects.Nodes.Clear();
            TreeNode parentNode = _treeObjects.Nodes.Add("Objects");
            long objIndex = 0;
            for(int i = 0; i < _dir._objectList._count; i++)
            {
                PopulateObjectNode(parentNode, ref objIndex, _dir._objectList[i]);
            }
        }
        private void PopulateObjectNode(TreeNode parent, ref long objIndex, igObject obj)
        {
            Console.WriteLine($"Adding object of type {obj.GetMeta()._name}...");
            parent.Nodes.Add((objIndex++).ToString(), obj.GetMeta()._name);
        }
        private void OnResize(object sender, EventArgs e)
        {
            _treeObjects.Location = new Point(12, 40);
			_treeObjects.Size = new Size(ClientSize.Width / 2 - 18, ClientSize.Height - 52);
			_panelInspector.Location = new Point(ClientSize.Width / 2 + 6, _treeObjects.Location.Y);
			_panelInspector.Size = new Size(ClientSize.Width / 2 - 18, ClientSize.Height - 52);
        }
    }
}