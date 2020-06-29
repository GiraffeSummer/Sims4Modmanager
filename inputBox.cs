using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sims_Mod_manager
{
    public partial class inputBox : Form
    {
        public string text = "";
        public inputBox(string text)
        {
            InitializeComponent();
            this.label1.Text = text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            text = this.textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
