using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Battle_Script_Pro
{
    public partial class Form3 : Form
    {
        private bool success;
        private int value;

        public bool Success
        {
            get { return success; }
        }

        public int LineNumber
        {
            get { return value; }
        }

        public Form3()
        {
            InitializeComponent();
            value = 0;
            success = false;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (Keys.Enter):
                    {
                        if (this.ActiveControl.Name.Equals("textBox1"))
                        {
                            button1_Click(sender, e);
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            success = Int32.TryParse(textBox1.Text, out value);
            this.Close();
        }
    }
}
