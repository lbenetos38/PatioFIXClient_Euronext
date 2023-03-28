using System;
using System.Windows.Forms;

namespace PatioFix.Broker
{
    public partial class ShowExceptionForm : Form
    {
        public ShowExceptionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public Exception LoggedException { get; set; }


        private void ShowExceptionForm_Load(object sender, EventArgs e)
        {
            this.treeView1.Nodes.Clear();


            TreeNode node = new TreeNode();
            if (LoggedException.Message.Length > 72)
            {
                node.Text = LoggedException.Message.Substring(0, 69) + "...";
            }
            else
            {
                node.Text = LoggedException.Message;
            }
            node.Tag = LoggedException;
            this.treeView1.Nodes.Add(node);

            Exception _ex = LoggedException.InnerException;
            while (_ex != null)
            {
                TreeNode _node = new TreeNode();
                if (_ex.Message.Length > 72)
                {
                    _node.Text = _ex.Message.Substring(0, 69) + "...";
                }
                else
                {
                    _node.Text = _ex.Message;
                }
                _node.ToolTipText = string.Empty;
                _node.Tag = _ex;
                node.Nodes.Add(_node);
                node = _node;

                _ex = _ex.InnerException;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Exception _ex = (Exception)e.Node.Tag;
            textBox1.Text = _ex.Message;
        }
    }
}
