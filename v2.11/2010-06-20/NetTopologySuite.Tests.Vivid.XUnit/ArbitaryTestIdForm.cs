using System;
using System.Windows.Forms;

namespace NetTopologySuite.Tests.Vivid.XUnit
{
    public partial class ArbitaryTestIdForm : Form
    {
        public ArbitaryTestIdForm()
        {
            InitializeComponent();
        }

        public Int32 TestId
        {
            get { return !String.IsNullOrEmpty(txtTestId.Text) ? int.Parse(txtTestId.Text) : -1; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}